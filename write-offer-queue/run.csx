#r "Newtonsoft.Json"
#load "..\offers.csx"

using System;
using HtmlAgilityPack;
using Newtonsoft.Json;

public static void Run(TimerInfo timer, IAsyncCollector<string> outputQueue, TraceWriter log)
{
    // Obtain all offers from local web site (logic to retrieve and parse is highly specific)
    //
    HttpClient client = new HttpClient();
    Dictionary<string, string> values = new Dictionary<string, string> {
        {"city", "0"},
        {"municipality", "0"},
        {"pageNumber", "1"},
        {"priceMax", "7300000"},
        {"priceMin", "0"},
        {"priceOrderingAsc", "true"},
        {"quadratureMax", "20000"},
        {"quadratureMin", "0"},
        {"shouldOrderByPrice", "false"},
        {"subjectType", "Zemljište"},
        {"versionId", "1"}
    };

    var content = new FormUrlEncodedContent(values);
    var response = client.PostAsync("https://www.raiffeisenbank.rs/aspx/property_finder/searchRealEstates.aspx", content).Result;
    var responseString = response.Content.ReadAsStringAsync().Result;

    // Use HtmlAgilityPack library to get all offers from HTML
    //
    HtmlDocument doc = new HtmlDocument();
    doc.LoadHtml(responseString);

    foreach(HtmlNode row in doc.DocumentNode.SelectNodes("//tbody/tr"))
    {
        string onclick = row.Attributes["onclick"].Value;
        string url = "https://www.raiffeisenbank.rs" + onclick.Substring(onclick.IndexOf(" = ") + 4);
        url = url.TrimEnd('\'');
        OfferQueue offer = new OfferQueue {
            id = row.SelectNodes("td")[1].InnerText,
            text = row.SelectNodes("td")[1].InnerText,
            url = url,
            price = row.SelectNodes("td")[2].InnerText,
            address = row.SelectNodes("td")[3].InnerText,
            totalArea = row.SelectNodes("td")[4].InnerText,
        };

        // Actually write one object (serialize to JSON beforehand) to Azure Service Bus
        //
        outputQueue.AddAsync(JsonConvert.SerializeObject(offer));
    }
}