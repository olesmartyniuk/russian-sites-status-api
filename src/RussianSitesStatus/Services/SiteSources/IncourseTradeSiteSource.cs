using RestSharp;
using RussianSitesStatus.Services.Contracts;
using System.Text.Json;

namespace RussianSitesStatus.Services
{
    public class IncourseTradeSiteSource : ISiteSource
    {
        public async Task<IEnumerable<string>> GetAllAsync()
        {
            var client = new RestClient("http://46.4.63.238/");
            var request = new RestRequest("sites.json", Method.Get);
            var response = await client.GetAsync(request);

            var allSites = JsonSerializer.Deserialize<IEnumerable<IncourseSiteResponce>>(response.Content);
            
            var relevantSites = allSites
                .Where(s => AsBoolean(s.atack) == true)
                .ToList();

            const int MaxNumberOfSites = 100;
            var notRelevantSites = allSites
                .Where(s => AsBoolean(s.atack) == false)
                .Take(MaxNumberOfSites - relevantSites.Count);

            relevantSites.AddRange(notRelevantSites);
            
            return relevantSites.Select(s => s.url);
        }

        private bool AsBoolean(object atack)
        {
            if (bool.TryParse(atack.ToString(), out var boolResult))
            {
                return boolResult;
            }

            if (int.TryParse(atack.ToString(), out var intResult))
            {
                return intResult == 1;
            }

            return false;
        }

        private class IncourseSiteResponce
        {
            public int id { get; set; }
            public string url { get; set; }
            public int need_parse_url { get; set; }
            public string page { get; set; }
            public object page_time { get; set; }
            public object atack { get; set; }
        }
    }
}
