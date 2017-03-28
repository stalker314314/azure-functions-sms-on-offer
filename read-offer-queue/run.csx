#r "Newtonsoft.Json"
#r "Microsoft.WindowsAzure.Storage"
#load "..\offers.csx"

using System;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// integrated Twilio is not working with Azure Functions, so using regular one
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

public static void Run(string inputQueue, IQueryable<Offer> inputTable, ICollector<Offer> outputTable,
    TraceWriter log)
{
    // Only needed in testing, so that empty inputQueue don't trigger various exceptions
    //
    if (String.IsNullOrEmpty(inputQueue) || inputQueue == "Service Bus Message")
    {
        log.Info("Empty inputQueue");
        return;
    }

    // Get one offer from Service Bus
    OfferQueue inputOffer = JsonConvert.DeserializeObject<OfferQueue>(inputQueue);

    TwilioClient.Init(
        "ACb72942bfb53a4401bbd18024b5bbe5c7",
        Environment.GetEnvironmentVariable("TWILIO_KEY", EnvironmentVariableTarget.Process)
    );

    // Azure Storage Table does not support Any(), so had to use this
    //
    bool found = inputTable.Where(p => p.RowKey == inputOffer.text).ToList().Count > 0;
    if (!found)
    {
        log.Info($"New entry: {inputOffer.text}");
        outputTable.Add(
            new Offer() {
                // This is web site of offer, but handy to be partition key for future
                PartitionKey = "raiffeisen", 
                RowKey = inputOffer.id,
                url = inputOffer.url,
                price = inputOffer.price,
                address = inputOffer.address,
                totalArea = inputOffer.totalArea
            });
        
        // Send SMS now
        MessageResource.Create(
            from: new PhoneNumber("+16283000024"),
            to: new PhoneNumber("+381693141592"),
            body: $"{inputOffer.url}, cena: '{inputOffer.price}', oglas: '{inputOffer.text}', ");
    }
}
