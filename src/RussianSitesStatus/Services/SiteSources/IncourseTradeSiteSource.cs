﻿using RestSharp;
using RussianSitesStatus.Services.Contracts;
using System.Text.Json;

namespace RussianSitesStatus.Services
{
    public class IncourseTradeSiteSource : ISiteSource
    {
        public async Task<IEnumerable<string>> GetAll()
        {
            var client = new RestClient("http://46.4.63.238/");
            var request = new RestRequest("sites.json", Method.Get);
            var response = await client.GetAsync(request);

            var allSites = JsonSerializer.Deserialize<IEnumerable<IncourseSiteResponce>>(response.Content);
            return allSites.Select(s => s.url);
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
