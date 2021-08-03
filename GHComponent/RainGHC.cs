using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace Taxonomy_Fungal.GHComponent
{
    public class RainGHC : GH_Component
    {

        public RainGHC()
          : base("Rain", "Rain",
              "Rain in Point4d. XYZ and W, W = drop weight.",
              "FungalTaxonomy", "Simulation")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Trian-Mesh only", GH_ParamAccess.item);
            pManager.AddPlaneParameter("Plane", "P", "Plane for input mesh orientation, default is XY-Plane", GH_ParamAccess.item, Plane.WorldXY);
            pManager.AddNumberParameter("Squared", "m²", "unit size", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "P", "list of point4d", GH_ParamAccess.list);
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh mesh = new Mesh();
            Plane pln = Plane.Unset;
            double w = 0.0;
            double s = 0.0;
            DA.GetData("Mesh", ref mesh);
            DA.GetData("Plane", ref pln);
            DA.GetData("Squared", ref s);

            BoundingBox bbox = mesh.GetBoundingBox(pln);
            List<Point3d> points = new List<Point3d>();
            double zHeight = bbox.PointAt(1, 1, 1).Z + 10;
            double xLength = bbox.PointAt(0, 0, 0).DistanceTo(bbox.PointAt(1, 0, 0)) / s;
            double yLength = bbox.PointAt(0, 0, 0).DistanceTo(bbox.PointAt(0, 1, 0)) / s;
            Point3d O = bbox.PointAt(0, 0, 0);
            for (double i = 0; i < xLength; i += s)
                for (double j = 0; j < yLength; j += s)
                    points.Add(new Point3d(O.X + i * s, O.Y + j * s, zHeight));

            DA.SetDataList("Points", points);
            Message = "C=" + points.Count.ToString();
        }


        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return Taxonomy_Fungal.Properties.Resources.rain;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("20ee7a8a-4518-414a-be58-fd99d2381a4c"); }
        }
    }
}