using Grasshopper.GUI.Canvas;
using Grasshopper.GUI;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using System;
using System.Drawing;


namespace AnythingButton
{
    public class SpawnerComponent : GH_Component
    {
        private bool buttonClicked = false;

        public SpawnerComponent()
            : base("Spawner", "Spawn", "Spawns a component via button", "AnythingButton", "New") { }

        public override Guid ComponentGuid => new Guid("DEADBEEF-1234-5678-ABCD-999999999999");

        protected override void RegisterInputParams(GH_InputParamManager pManager) { }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager) { }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (buttonClicked)
            {
                SpawnMyComponent();
                buttonClicked = false; // reset
            }
        }

        public override void CreateAttributes()
        {
            m_attributes = new ButtonComponentAttributes(this);
        }

        private void SpawnMyComponent()
        {
            var canvas = Grasshopper.Instances.ActiveCanvas;
            var doc = canvas?.Document;

            if (doc == null) return;

            var comp = new MyRuntimeComponent();
            comp.CreateAttributes();
            comp.Attributes.Pivot = new System.Drawing.PointF(200, 200); // arbitrary location
            doc.AddObject(comp, false);
            canvas.Refresh();
        }

        private class ButtonComponentAttributes : GH_ComponentAttributes
        {
            public ButtonComponentAttributes(SpawnerComponent owner) : base(owner) { }

            protected override void Layout()
            {
                base.Layout();
                var baseBox = Bounds;
                var buttonBox = new System.Drawing.RectangleF(baseBox.X, baseBox.Bottom + 4, 80, 22);
                Bounds = new System.Drawing.RectangleF(baseBox.X, baseBox.Y, baseBox.Width, baseBox.Height + 30);
                ButtonBounds = buttonBox;
            }

            private System.Drawing.RectangleF ButtonBounds;

            protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
            {
                base.Render(canvas, graphics, channel);

                if (channel == GH_CanvasChannel.Objects)
                {
                    var capsule = GH_Capsule.CreateTextCapsule(
                        ButtonBounds,  // bounds of the button
                        ButtonBounds,  // inner bounds (can match outer)
                        GH_Palette.Normal, // style (Normal, Warning, Error, etc.)
                        "Spawn",        // text
                        2,              // corner radius
                        0               // padding
                    );

                    capsule.Render(graphics, Selected, Owner.Locked, false);
                    capsule.Dispose();
                }
            }

            public override GH_ObjectResponse RespondToMouseDown(GH_Canvas sender, GH_CanvasMouseEvent e)
            {
                if (ButtonBounds.Contains(e.CanvasLocation))
                {
                    ((SpawnerComponent)Owner).buttonClicked = true;
                    Owner.ExpireSolution(true);
                    return GH_ObjectResponse.Handled;
                }

                return base.RespondToMouseDown(sender, e);
            }
        }
    }
}
