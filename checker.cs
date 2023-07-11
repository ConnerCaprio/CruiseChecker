using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using System.Net;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


namespace RoyalCaribbeanChecker
{
    public class checker
    {
        [FunctionName("checker")]
        public void Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            string url = "https://www.royalcaribbean.com/booking/landing?groupId=AL03PCN-3464559352&sailDate=2024-02-09&shipCode=AL&packageCode=AL3BH068&destinationCode=BAHAM&selectedCurrencyCode=USD&country=USA&loyaltyNumber=380604273&CAS=true";
            string spanId = "pricing-promotions__price-value";

        if (!string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(spanId))
        {
            try
            {
                WebClient webClient = new WebClient();
                string html = webClient.DownloadString(url);

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(html);

                HtmlNode spanNode = doc.DocumentNode.Descendants("span")
                    .FirstOrDefault(n => n.Attributes["class"]?.Value == spanClass);

                string spanValue = spanNode?.InnerText;

                if (!string.IsNullOrEmpty(spanValue))
                {
                    return new OkObjectResult(spanValue);
                }
                else
                {
                    return new BadRequestObjectResult("Specified span ID not found in the HTML.");
                }
            }
            catch
            {
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
        else
        {
            return new BadRequestObjectResult("Please provide 'url' and 'span_id' parameters.");
        }
        }
    }
}