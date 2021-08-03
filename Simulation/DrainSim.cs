using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Rhino;
using Rhino.Geometry;

namespace Taxonomy_Fungal.Simulation
{
    class DrainSim
    {
        public double tolerance2Mesh = 0.5;
        public double HydrophilicRate = 0.06;
        public Point3d[] Drop;
        public Vector3d[] Velocity;
        public List<Point3d>[] DrainPath;
        public double[] Weight; //each drop weight
        public Mesh Geo;
        public bool[] Recording;
        public List<Polyline> Trails = new List<Polyline>();
        public bool[] isDripEdge;//test
        public double[] MoistureMap;
        private List<int> GeoBoundary = new List<int>();
        //Viscosity
        public DrainSim(Mesh m, List<Point3d> pts, double W)
        {
            Geo = m;
            Drop = new Point3d[pts.Count];
            Velocity = new Vector3d[pts.Count];
            Weight = new double[pts.Count];
            DrainPath = new List<Point3d>[pts.Count];
            Recording = new bool[pts.Count];
            isDripEdge = new bool[pts.Count]; //test
            MoistureMap = new double[Geo.Faces.Count];
            for (int i = 0; i < pts.Count; i++)
            {
                Drop[i] = new Point3d(pts[i].X, pts[i].Y, pts[i].Z);
                Velocity[i] = -Vector3d.ZAxis;
                Weight[i] = W;
                Recording[i] = false;
                DrainPath[i] = new List<Point3d>();
                isDripEdge[i] = false;//test
            }
            
            for (int i = 0; i < Geo.Faces.Count; i++)
            {
                MoistureMap[i] = 0.0;
                if (Geo.Faces.HasNakedEdges(i))
                    GeoBoundary.Add(i);
            }
            Geo.VertexColors.Add(255, 255, 255);
        }

        public Vector3d drainVector(Point3d pt, out int faceIndex)
        {
            MeshPoint mPt = Geo.ClosestMeshPoint(pt, 5000);
            faceIndex = mPt.FaceIndex;
            Plane pln = new Plane(mPt.Point, Geo.NormalAt(mPt));
            double angle = Vector3d.VectorAngle(pln.XAxis, -Vector3d.ZAxis, pln);
            pln.Rotate(angle, pln.ZAxis);
            double slope = Vector3d.VectorAngle(Vector3d.ZAxis, pln.XAxis, new Plane(pln.Origin, pln.XAxis, pln.ZAxis));
            return pln.XAxis * (slope / Math.PI);
        }

        public void DropUpdate()
        {

            Parallel.For(0, DrainPath.Length, i =>
            {
                Point3d currentDrop = Geo.ClosestPoint(Drop[i]);
                double sqrtDist = currentDrop.DistanceToSquared(Drop[i]);
                if (isDripEdge[i] && Velocity[i].Length < 1.0 && sqrtDist > 1.0)
                    isDripEdge[i] = false;
                if (sqrtDist < tolerance2Mesh && !isDripEdge[i])
                {
                    Recording[i] = true;
                    int escape;
                    Velocity[i] = Velocity[i] * 0.8 + drainVector(Drop[i], out escape) * 0.2; // accerlation
                    MoistureMap[escape] += Weight[i] * HydrophilicRate;
                    Weight[i] = Weight[i] * (1.0 - HydrophilicRate);
                    if (GeoBoundary.Contains(escape))
                        isDripEdge[i] = true;
                    Drop[i] = Drop[i] + Velocity[i];
                    Drop[i] = Geo.PointAt(Geo.ClosestMeshPoint(Drop[i], 2000));
                    DrainPath[i].Add(Drop[i]);
                }
                else
                {
                    //simulating drop the edge
                    Velocity[i] = Velocity[i] * 0.2 + -(Vector3d.ZAxis * 0.8);
                    Drop[i] = Drop[i] + Velocity[i];
                    if (Recording[i])
                        DrainPath[i].Add(Drop[i]);
                }
            });
        }


        public void SmoothMoistureContent(int Level)
        {
            double emission = 0.05;
            for (int l = 0; l < Level; l++)
                for (int i = 0; i < Geo.Faces.Count; i++)
                {
                    int[] connected = Geo.Faces.AdjacentFaces(i);
                    MoistureMap[i] = MoistureMap[i] * (1.0 - emission);
                    for (int j = 0; j < connected.Length; j++)
                        MoistureMap[i] += (MoistureMap[j] * emission) / connected.Length;
                }
        }

        public double[] PaintMoisture(double Weight)
        {
            System.Drawing.Color C0 = System.Drawing.Color.FromArgb(255, 255, 255);
            System.Drawing.Color C1 = System.Drawing.Color.FromArgb(0, 184, 245);
            System.Drawing.Color C2 = System.Drawing.Color.FromArgb(28, 100, 158);
            double[] blue = new double[Geo.Vertices.Count];
            for (int i = 0; i < Geo.Vertices.Count; i++)
            {
                int[] faces = Geo.Vertices.GetVertexFaces(i);
                double w = 0.0;
                System.Drawing.Color c = new System.Drawing.Color();
                for (int j = 0; j < faces.Length; j++)
                    w += MoistureMap[faces[j]] / faces.Length;
                if (w <= Weight / 2)
                    c = ColorAt(C0, C1, w / Weight);
                else if (w > Weight / 2)
                {
                    if (w > Weight)
                        w = Weight;
                    c = ColorAt(C1, C2, w / Weight);
                }
                blue[i] = w;
                Geo.VertexColors.SetColor(i, c);
            }
            return blue;
        }
        public System.Drawing.Color ColorAt(System.Drawing.Color Start, System.Drawing.Color End, double At)
        {
            System.Drawing.Color c = System.Drawing.Color.FromArgb(
              Start.R + (int)((End.R - Start.R) * At),
              Start.G + (int)((End.G - Start.G) * At),
              Start.B + (int)((End.B - Start.B) * At));
            return c;
        }

        public void Compute(int max)
        {
            Trails = new List<Polyline>();
            for (int i = 0; i < max; i++)
                DropUpdate();
            for (int i = 0; i < DrainPath.Length; i++)
            {
                if (Recording[i] && DrainPath[i].Count > 1)
                {
                    Polyline trail = new Polyline(DrainPath[i]);
                    Trails.Add(trail);
                }
            }
        }
    }
}
