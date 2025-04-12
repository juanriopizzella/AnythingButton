using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;

namespace AnythingButton
{
    public class AnythingButtonComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        private static readonly HttpClient client = new HttpClient();
        public AnythingButtonComponent()
          : base("AnythingButton", "Your dream tool!",
            "Description",
            "AnythingButton", "AnythingButton")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Prompt", "Prompt", "GH component you're dreaming about!", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Run", "Run", "Run if ture, use Boolean toggle for this.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Response", "R", "Server response or status", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        private static async Task<string> SendPutRequestAsync(string prompt)
        {
            const string url = "https://httpbin.org/put"; // Replace with your real URL
            var jsonContent = new StringContent($"{{\"prompt\":\"{prompt}\"}}", Encoding.UTF8, "application/json");

            using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(1)); // 1 minute timeout

            try
            {
                var response = await client.PutAsync(url, jsonContent, cts.Token);
                return response.IsSuccessStatusCode
                    ? await response.Content.ReadAsStringAsync()
                    : $"HTTP Error: {response.StatusCode}";
            }
            catch (TaskCanceledException)
            {
                return "Request timed out after 1 minute.";
            }
            catch (Exception ex)
            {
                return $"Exception: {ex.Message}";
            }
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// You can add image files to your project resources and access them like this:
        /// return Resources.IconForThisComponent;
        /// </summary>
        protected override System.Drawing.Bitmap Icon => null;

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>

        public override Guid ComponentGuid => new Guid("61bfd719-e7a0-4bbf-a02c-9c2bc7ac60cc");


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string prompt = string.Empty;
            bool trigger = false;

            if (!DA.GetData(0, ref prompt)) return;
            if (!DA.GetData(1, ref trigger)) return;

            if (trigger)
            {
                var task = SendPutRequestAsync(prompt);
                task.Wait(); // Blocking: not ideal, but simple
                DA.SetData(0, task.Result);
                trigger = false;
            }
        }
    }
}