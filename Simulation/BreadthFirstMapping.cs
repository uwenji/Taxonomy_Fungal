using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino;
using Rhino.Geometry;

namespace Taxonomy_Fungal.Simulation
{
    class BreadthFirstMapping
    {
        public Mesh refMesh = new Mesh();
        public bool[] VisitedFaces;
        public bool[] VisitedIndices;
        public List<int> faceOrder = new List<int>();
        public List<int> SpanningIndices = new List<int>();
        int FirstFace = -1;
        public BreadthFirstMapping(Mesh mesh)
        {
            refMesh = mesh;
            VisitedFaces = new bool[mesh.Faces.Count];
            VisitedIndices = new bool[mesh.Vertices.Count];
            List<int> firstCorners = TwoAnchors(mesh);
            FirstFace = AdjacentFace(mesh.Vertices.GetVertexFaces(firstCorners[0]), mesh.Vertices.GetVertexFaces(firstCorners[1]));
            VisitedFaces[FirstFace] = true;
        }

        public List<int> NextFaces(int ID)
        {
            List<int> NewNext = new List<int>();
            MeshFace face = refMesh.Faces[ID];
            VisitedFaces[ID] = true;
            int[] A_faces = refMesh.Vertices.GetVertexFaces(face.A);
            int[] B_faces = refMesh.Vertices.GetVertexFaces(face.B);
            int[] C_faces = refMesh.Vertices.GetVertexFaces(face.C);

            int AB_face = AdjacentFace(A_faces, B_faces);
            int BC_face = AdjacentFace(B_faces, C_faces);
            int CA_face = AdjacentFace(C_faces, A_faces);
            if (AB_face != -1 && !VisitedFaces[AB_face])
            {
                VisitedFaces[AB_face] = true;
                faceOrder.Add(AB_face);
                NewNext.Add(AB_face);
                AddAllVextex(AB_face);
            }
            if (BC_face != -1)
            {
                VisitedFaces[BC_face] = true;
                faceOrder.Add(BC_face);
                NewNext.Add(BC_face);
                AddAllVextex(BC_face);

            }
            if (CA_face != -1)
            {
                VisitedFaces[CA_face] = true;
                faceOrder.Add(CA_face);
                NewNext.Add(CA_face);
                AddAllVextex(CA_face);
            }
            return NewNext;
        }
        public void Spanning()
        {
            int count = 1;
            faceOrder.Add(FirstFace);
            List<int> current = NextFaces(FirstFace);
            List<int> nexts = new List<int>();
            do
            {
                for (int i = 0; i < current.Count; i++)
                {
                    nexts.AddRange(NextFaces(current[i]));
                }
                count += current.Count;
                current = nexts;
                nexts = new List<int>();

            } while (count < refMesh.Faces.Count);
        }
        public static List<int> TwoAnchors(Mesh mesh)
        {
            bool[] nakedStatus = mesh.GetNakedEdgePointStatus();
            List<int> anchors = new List<int>();
            for (int i = 0; i < nakedStatus.Length; i++)
                if (nakedStatus[i])
                {
                    anchors.Add(i);
                    if (anchors.Count > 1)
                        break;
                }
            return anchors;
        }

        public int AdjacentFace(int[] FacesA, int[] FacesB)
        {
            /*
             * if no Previous face just give -1;
             */
            int adjacent = -1;
            foreach (int i in FacesA)
                foreach (int j in FacesB)
                    if (i == j && !VisitedFaces[i])
                    {
                        adjacent = i;
                        break;
                    }
            return adjacent;
        }

        public int AddAllVextex(int FaceID, int A, int B)
        {
            int C = -1;
            MeshFace face = refMesh.Faces[FaceID];
            List<int> Indices = new List<int> { face.A, face.B, face.C };
            foreach (int i in Indices)
                if (i != A && i != B)
                    C = i;
            return C;
        }
        public void AddAllVextex(int FaceID)
        {
            MeshFace face = refMesh.Faces[FaceID];
            List<int> Indices = new List<int> { face.A, face.B, face.C, face.D };
            foreach (int i in Indices)
                if (!VisitedIndices[i])
                {
                    VisitedIndices[i] = true;
                    SpanningIndices.Add(i);
                }
        }
    }
}
