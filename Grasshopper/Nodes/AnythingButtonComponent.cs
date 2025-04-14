using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;
using AnythingButton.Properties;
using System.Drawing;

namespace AnythingButton
{
    public class AnythingButtonComponent : GH_Component
    {

        private static readonly HttpClient client = new HttpClient();
        public AnythingButtonComponent()
          : base("AnythingButton", "Your dream tool!",
            "Description",
            "AnythingButton", "AnythingButton")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Prompt", "Prompt", "GH component you're dreaming about!", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Run", "Run", "Run if ture, use Boolean toggle for this.", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Response", "Result", "Server response or status", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {

            string pre_prompt = "create a grasshopper component in csharp inherit GH_Component, with category as \"AnythingButton\" and subcategory as \"AnythingButton\". The component ";
            string post_prompt = ". set protected override Bitmap Icon => to the bitmap of a generated base64 24x24 icon";
            string prompt = string.Empty;
            bool trigger = false;

            if (!DA.GetData(0, ref prompt)) return;
            if (!DA.GetData(1, ref trigger)) return;

            if (trigger)
            {
                // Run the async task on a background thread
                Task.Run(async () =>
                {
                    string result = await SendPostRequestAsync(pre_prompt + prompt);

                    RunGitPull("%USERPROFILE%\\Desktop\\Code\\AnythingButton_Results");
                    // Safely update the output on the main thread
                    Rhino.RhinoApp.InvokeOnUiThread(() =>
                    {
                        DA.SetData(0, result);
                    });
                });

                // Reset the trigger to prevent repeated execution
                //trigger = false;
            }
        }

        private static async Task<string> SendPostRequestAsync(string prompt)
        {
            const string url = "https://ai.aria.run/prompt";
            var jsonContent = new StringContent($"{{\"prompt\":\"{prompt}\"}}", Encoding.UTF8, "application/json");

            using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2));

            try
            {
                var response = await client.PostAsync(url, jsonContent, cts.Token);
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

        public void RunGitPull(string repoPath)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = "pull",
                WorkingDirectory = repoPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = Process.Start(psi))
            {
                string output = process.StandardOutput.ReadToEnd();
                string errors = process.StandardError.ReadToEnd();
                process.WaitForExit();

                Rhino.RhinoApp.WriteLine("Git Pull Output:\n" + output);
                if (!string.IsNullOrWhiteSpace(errors))
                    Rhino.RhinoApp.WriteLine("Git Pull Errors:\n" + errors);
            }
        }

        protected override Bitmap Icon => UtilityIcon.ResizeIcon(Resources.IconCanvasCustom);

        public override Guid ComponentGuid => new Guid("61bfd719-e7a0-4bbf-a02c-9c2bc7ac60cc");
    }
}