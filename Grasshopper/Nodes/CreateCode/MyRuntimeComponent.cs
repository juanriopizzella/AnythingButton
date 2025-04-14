using Grasshopper.Kernel;
using System;


namespace AnythingButton
{
    public class MyRuntimeComponent : GH_Component
    {
        public MyRuntimeComponent() : base("RuntimeComp", "RTC", "Spawned at runtime", "AnythingButton", "New") { }

        public override Guid ComponentGuid => new Guid("ABCDEF01-2345-6789-ABCD-1234567890AB");

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Input", "I", "An input", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Output", "O", "An output", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string input = "";
            DA.GetData(0, ref input);
            DA.SetData(0, "Echo: " + input);
        }
    }
}
