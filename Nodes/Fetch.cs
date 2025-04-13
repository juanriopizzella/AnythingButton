using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnythingButton.Nodes
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.IO.Compression;
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;

    private async Task<string> DownloadGhaFromGitHub()
    {
        string owner = "YOUR_GITHUB_USERNAME";
        string repo = "YOUR_REPO_NAME";
        string token = ""; // If private repo: "ghp_XXXX"
        string artifactName = "AnythingButtonPlugin";
        string ghaInstallPath = @"C:\Users\GarethVolka\AppData\Roaming\Grasshopper\Libraries\AnythingButton.gha";

        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("GHPluginFetcher", "1.0"));
            if (!string.IsNullOrWhiteSpace(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", token);

            // Get the latest workflow run
            var runUrl = $"https://api.github.com/repos/{owner}/{repo}/actions/runs?per_page=1&status=completed";
            var runResp = await client.GetStringAsync(runUrl);
            var runs = JObject.Parse(runResp)["workflow_runs"];
            if (runs == null || runs.Count() == 0) return "No workflow runs found.";

            var latestRun = runs[0];
            if ((string)latestRun["conclusion"] != "success")
                return "Latest build did not succeed.";

            string runId = latestRun["id"].ToString();

            // Get artifacts
            var artifactUrl = $"https://api.github.com/repos/{owner}/{repo}/actions/runs/{runId}/artifacts";
            var artifactResp = await client.GetStringAsync(artifactUrl);
            var artifacts = JObject.Parse(artifactResp)["artifacts"];

            if (artifacts == null || artifacts.Count() == 0)
                return "No artifacts found.";

            string downloadUrl = null;

            foreach (var artifact in artifacts)
            {
                if ((string)artifact["name"] == artifactName)
                {
                    downloadUrl = (string)artifact["archive_download_url"];
                    break;
                }
            }

            if (downloadUrl == null)
                return $"Artifact '{artifactName}' not found.";

            var artifactZip = await client.GetByteArrayAsync(downloadUrl);
            string tempZipPath = Path.GetTempFileName();
            File.WriteAllBytes(tempZipPath, artifactZip);

            using (var archive = ZipFile.OpenRead(tempZipPath))
            {
                foreach (var entry in archive.Entries)
                {
                    if (entry.FullName.EndsWith(".gha"))
                    {
                        entry.ExtractToFile(ghaInstallPath, true);
                        return "✅ Plugin downloaded successfully!";
                    }
                }
            }

            return "Artifact downloaded, but no .gha file found.";
        }
    }

    // Main script entry point
    private void RunScript(bool RunDownload, ref object Result)
    {
        if (!RunDownload)
        {
            Result = "Waiting...";
            return;
        }

        var task = DownloadGhaFromGitHub();
        task.Wait();
        Result = task.Result;
    }
}
