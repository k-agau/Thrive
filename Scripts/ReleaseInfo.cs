namespace Scripts;

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json.Nodes;
using ScriptsBase.Utilities;

public class ReleaseInfo
{
    private const string PATCH_NOTES_LOCATION = "PatchNotes.json";
    private const string LIST_URL = "https://api.github.com/repos/Revolutionary-Games/Thrive/releases";
    private readonly Program.ReleaseOptions options;
    private readonly Uri url;

    public ReleaseInfo(Program.ReleaseOptions options)
    {
        this.options = options;
        this.url = new Uri(LIST_URL);
    }

    public async Task<bool> Run(CancellationToken cancellationToken)
    {
        var client = new HttpClient();

        // Set the user agent
        client.DefaultRequestHeaders.Add("User-Agent", "Get release script");

        // Get the response.
        HttpResponseMessage listReponse = await client.GetAsync(this.url);

        // Get the response content.
        HttpContent listResponseContent = listReponse.Content;

        string listOfReleases;

        // Get the stream of the content.
        using (var reader = new StreamReader(await listResponseContent.ReadAsStreamAsync()))
        {
            // Save the output as a string
            listOfReleases = await reader.ReadToEndAsync();
        }

        var releaseJson = JsonArray.Parse(listOfReleases);

        Dictionary<string, string> dict = new Dictionary<string, string>();

        foreach (var release in releaseJson.AsArray())
        {
            // Process the key
            string key = release["name"].ToString().Substring(7);

            int count = key.Split(".").Length - 1;

            if (count == 2)
            {
                key += ".0";
            }

            // Process the body
            string val = release["body"].ToString();

            int idx = val.IndexOf("## Patch Notes");

            if (idx != -1)
            {
                val = val.Substring(idx);
            }

            dict.Add($"{key}", val);
        }

        await JsonWriteHelper.WriteJsonWithBom(PATCH_NOTES_LOCATION, dict, cancellationToken);

        return true;
    }
}