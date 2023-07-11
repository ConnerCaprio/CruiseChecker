using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using System.Net;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;

namespace RoyalCaribbeanChecker
{
    public class checker
    {
        [FunctionName("checker")]
        public static async Task Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            string url = "https://www.royalcaribbean.com/booking/landing?groupId=AL03PCN-3464559352&sailDate=2024-02-09&shipCode=AL&packageCode=AL3BH068&destinationCode=BAHAM&selectedCurrencyCode=USD&country=USA&loyaltyNumber=380604273&CAS=true";
        

        // Class name of the span element to scrape
        //string className = "pricing-promotions__price-value";
        string spanId = "landingPage_totalPrice_text";

        // Create an HttpClient instance to send HTTP requests
        using (HttpClient client = new HttpClient())
        {
            // Send a GET request to the website
            HttpResponseMessage response = await client.GetAsync(url).Result;

            // Check if the request was successful
            if (response.IsSuccessStatusCode)
            {
                await Task.Delay(5000);
                // Read the response content as a string
                string htmlContent = await response.Content.ReadAsStringAsync();

                // Load the HTML content into HtmlAgilityPack's HtmlDocument
                HtmlDocument htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(htmlContent);

                // Select the span elements with the specified class name
                //HtmlNodeCollection spanNodes = htmlDoc.DocumentNode.SelectNodes($"//span[contains(@class, '{className}')]");
                HtmlNode spanNode = htmlDoc.DocumentNode.SelectSingleNode($"//span[@id='{spanId}']");

                log.LogInformation($"is span nodes null? {htmlContent}");

                if (spanNode != null)
                {
                    // Extract the inner text of the span element
                    string spanText = spanNode.InnerText;

                    // Do something with the extracted text
                    log.LogInformation($"Scraped span text: {spanText}");
                }
                else
                {
                    log.LogInformation("No span element found with the specified id.");
                }

                // Process the selected span elements
                // if (spanNodes != null)
                // {
                //     foreach (HtmlNode spanNode in spanNodes)
                //     {
                //         // Extract the inner text of the span element
                //         string spanText = spanNode.InnerText;

                //         // Do something with the extracted text
                //         log.LogInformation($"Scraped span text: {spanText}");
                //     }
                // }
                // else
                // {
                //     log.LogInformation("No span elements found with the specified class name.");
                // }
            }
            else
            {
                log.LogError("Failed to retrieve the website content.");
            }
            }
        }
    }
}