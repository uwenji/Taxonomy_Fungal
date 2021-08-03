using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Taxonomy_Fungal.GHComponent
{
    public class BreadthFirstMapping : GH_Component
    {
        public BreadthFirstMapping()
          : base("BreadthFirstMapping", "BFM",
              "Breadth-First Mapping same topology mesh but disorder in verices.",
              "FungalTaxonomy", "Utility")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("RefColor3DMesh", "C3D", "Reference for colored mesh", GH_ParamAccess.item);
            pManager.AddMeshParameter("RefTop3DMesh", "T3D", "Reference for 3d mesh", GH_ParamAccess.item);
            pManager.AddMeshParameter("RefTop2DMesh", "T2D", "Flatten mesh", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("ColorMesh", "CM", "Color mesh", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh ref3DMesh = new Mesh();
            Mesh refColorM = new Mesh();
            Mesh targetM = new Mesh();
            Mesh ColoredM = new Mesh();
            DA.GetData("RefTop3DMesh", ref ref3DMesh);
            DA.GetData("RefColor3DMesh", ref refColorM);
            DA.GetData("RefTop2DMesh", ref targetM);
            Simulation.BreadthFirstMapping rBFM = new Simulation.BreadthFirstMapping(ref3DMesh);
            Simulation.BreadthFirstMapping dBFM = new Simulation.BreadthFirstMapping(targetM);
            rBFM.Spanning();
            dBFM.Spanning();
            Dictionary<int, Point3f> unsorted = new Dictionary<int, Point3f>();
            
            for (int i = 0; i < dBFM.SpanningIndices.Count; i++)
                unsorted.Add(rBFM.SpanningIndices[i], dBFM.refMesh.Vertices[dBFM.SpanningIndices[i]]);

            List<Point3f> sorted = unsorted.OrderBy(x => x.Key).Select(v => v.Value).ToList();
            for(int i = 0; i < sorted.Count; i++)
            {
                ColoredM.Vertices.Add(sorted[i]);
                ColoredM.VertexColors.Add(refColorM.VertexColors[i]);
            }
            for (int i = 0; i < refColorM.Faces.Count; i++)
                ColoredM.Faces.AddFace(refColorM.Faces[i]);
            
            DA.SetData("ColorMesh", ColoredM);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return Taxonomy_Fungal.Properties.Resources.mapping;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("0b5c6f73-e566-4223-81be-e5b6d108d754"); }
        }
    }
}