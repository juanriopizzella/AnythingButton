using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;
using AnythingButton.Properties;
using System.Drawing;
using Grasshopper.Kernel;
using System.Net.Http.Headers;
using System.IO.Compression;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace AnythingButton
{
    public class Fetch : GH_Component
    {
        public Fetch() : base("Fetch", "Fetch", "Downloads latest .gha from GitHub", "AnythingButton", "Fetch") { }

        public override Guid ComponentGuid => new Guid("e3639c14-d86d-45e6-8213-2dd9aba42cba");

        protected override Bitmap Icon => UtilityIcon.ResizeIcon(Resources.IconCanvasCleanPlus);

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Update Now", "Run", "Run the update check", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Result", "R", "Result of update process", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool run = false;
            DA.GetData(0, ref run);

            if (!run)
            {
                DA.SetData(0, "Waiting...");
                return;
            }

            string result = DownloadAndInstall().GetAwaiter().GetResult();
            DA.SetData(0, result);
        }

        private async Task<string> DownloadAndInstall()
        {
            string owner = "juanriopizzella";
            string repo = "AnythingButton_Results";
            string artifactName = "AnythingButtonPlugin";
            string installPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Grasshopper\Libraries\AnythingButton.gha");

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("GHPluginUpdater", "1.0"));
                // Optional: If private repo, use token
                // client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", "ghp_XXXX");

                string runUrl = $"https://api.github.com/repos/{owner}/{repo}/actions/runs?per_page=1&status=completed";
                var runJson = await client.GetStringAsync(runUrl);
                var runs = JObject.Parse(runJson)["workflow_runs"];
                if (runs == null || !runs.Any()) return "No workflow runs found.";

                var latest = runs[0];
                if ((string)latest["conclusion"] != "success") return "Last build was not successful.";

                string runId = (string)latest["id"];
                string artifactUrl = $"https://api.github.com/repos/{owner}/{repo}/actions/runs/{runId}/artifacts";
                var artifactsJson = await client.GetStringAsync(artifactUrl);
                var artifacts = JObject.Parse(artifactsJson)["artifacts"];

                if (artifacts == null) return "No artifacts found.";

                string downloadUrl = null;
                foreach (var artifact in artifacts)
                {
                    if ((string)artifact["name"] == artifactName)
                    {
                        downloadUrl = (string)artifact["archive_download_url"];
                        break;
                    }
                }

                if (downloadUrl == null) return "Artifact not found.";

                var zipBytes = await client.GetByteArrayAsync(downloadUrl);
                string tempZip = Path.GetTempFileName();
                File.WriteAllBytes(tempZip, zipBytes);

                using (var zip = ZipFile.OpenRead(tempZip))
                {
                    foreach (var entry in zip.Entries)
                    {
                        if (entry.FullName.EndsWith(".gha"))
                        {
                            entry.ExtractToFile(installPath, true);
                            return $"✅ Downloaded to {installPath}";
                        }
                    }
                }

                return "No .gha file in artifact.";
            }
        }
    }
}