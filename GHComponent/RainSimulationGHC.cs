using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;


namespace Taxonomy_Fungal
{
    public class RainSimulationGHC : GH_Component
    {
        
        public RainSimulationGHC()
          : base("RainSimulation", "RainSim",
              "Rain Simulation.",
              "FungalTaxonomy", "Simulation")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Mesh for rain simulation", GH_ParamAccess.item);
            pManager.AddPointParameter("Points", "P", "Drop location", GH_ParamAccess.list);
            pManager.AddNumberParameter("Weight", "W", "Weight of drop", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Step", "S", "Rain Iteration", GH_ParamAccess.item, 1);
            pManager.AddNumberParameter("Hydrophilic", "hydrophilic", "Hydrophilic rate", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("MoistMesh", "MM", "Moisture Map on the Mesh", GH_ParamAccess.item);
            pManager.AddCurveParameter("RainTrail", "T", "Trail of drop", GH_ParamAccess.list);
            pManager.AddNumberParameter("MoistureValue", "V", "Vaule of Moisture Content", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh m = new Mesh();
            double weight = 0.0;
            int iteration = 1;
            double Hydrophilic = 0.0;
            List<Point3d> rain = new List<Point3d>();
            
            DA.GetData("Mesh", ref m);
            DA.GetData("Weight", ref weight);
            DA.GetData("Step", ref iteration);
            DA.GetData("Hydrophilic", ref Hydrophilic);
            DA.GetDataList(1, rain);
            if (iteration < 0)
                iteration = 1;

            Simulation.DrainSim drainSim = new Simulation.DrainSim(m, rain, weight);
            drainSim.Compute(iteration);
            drainSim.PaintMoisture(10);
            DA.SetData("MoistMesh", drainSim.Geo);
            DA.SetDataList("RainTrail", drainSim.Trails);
            DA.SetDataList("MoistureValue", drainSim.MoistureMap);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return Taxonomy_Fungal.Properties.Resources.rainSim;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("6b6e7236-1c6b-4103-a28c-708aa2badde2"); }
        }
    }
}
