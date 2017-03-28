#r "Microsoft.WindowsAzure.Storage"
using Microsoft.WindowsAzure.Storage.Table;

// Offer object, to be serialized/deserialized to Service Bus
//
public class OfferQueue
{
    public string id { get; set; }
    public string text { get; set; }
    public string url { get; set; }
    public string price { get; set; }
    public string address { get; set; }
    public string totalArea { get; set; }
}

// Offer object, similar to OfferQueue, ready to be persisted in Azure Storage Table
//
public class Offer : TableEntity 
{
    public string url { get; set; }
    public string price { get; set; }
    public string address { get; set; }
    public string totalArea { get; set; }
}