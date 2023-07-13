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
using System.IO;
using System.IO.Compression;
using System.Text;
using Microsoft.Azure.NotificationHubs;

namespace RoyalCaribbeanChecker
{
    public class checker
    {
        [FunctionName("checker")]
        public static async Task Run([TimerTrigger("0 0 15 * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation("C# Timer trigger function executed at: {time}", DateTime.Now);

            var httpClient = new HttpClient();
            var url = "https://www.royalcaribbean.com/mcb/booking/create/header?CAS=true&country=USA&destinationCode=BAHAM&groupId=AL03PCN-3464559352&loyaltyNumber=380604273&packageCode=AL3BH068&sailDate=2024-02-09&selectedCurrencyCode=USD&shipCode=AL&landing=true&language=en&market=usa&browser=chrome&device=desktop";

            try
            {

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("accept-language", "en-US,en;q=0.9");
                request.Headers.Add("Host", "www.royalcaribbean.com");
                request.Headers.Add("Connection", "keep-alive");
                request.Headers.Add("Postman-Token", "fa74804d-7eb4-4e3b-ad11-bcec2f4abe93");
                request.Headers.Add("Cache-Control", "no-cache");
                request.Headers.Add("User-Agent", "PostmanRuntime/7.26.10");
                request.Headers.Add("Accept", "*/*");
                request.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                request.Headers.Add("Cookie", "SSOExchange=0000000000; country=USA; currency=USD; language=en; rcclGuestCookie=%7B%22dateCreated%22%3Anull%2C%22minDate%22%3Anull%2C%22maxDate%22%3Anull%2C%22searchUrl%22%3Anull%2C%22numberOfNights%22%3A%5B%5D%2C%22sailingTo%22%3A%5B%5D%2C%22leavingFrom%22%3A%5B%5D%2C%22ships%22%3A%5B%5D%2C%22vacationTypes%22%3A%5B%5D%2C%22accessibility%22%3Anull%2C%22topSearchResults%22%3A%7B%7D%2C%22packageCode%22%3A%22AL3BH068%22%2C%22shipCode%22%3A%22AL%22%2C%22sailDate%22%3A%222024-02-09%22%2C%22startDate%22%3A%222024-02-09%22%2C%22departureCode%22%3Anull%2C%22itineraryName%22%3A%223+Night++Bahamas+%26+Perfect+Day%22%2C%22nights%22%3A0%2C%22stateroomPricing%22%3A%22LowestAvailable%22%2C%22itineraryUrl%22%3A%22https%3A%2F%2Fwww.royalcaribbean.com%2Fbooking%2Flanding%3FgroupId%3DAL03PCN-3464559352%26sailDate%3D2024-02-09%26shipCode%3DAL%26packageCode%3DAL3BH068%26destinationCode%3DBAHAM%26selectedCurrencyCode%3DUSD%26country%3DUSA%26loyaltyNumber%3D380604273%26CAS%3Dtrue%26step%3Dstateroom%22%2C%22numberOfAdults%22%3A2%2C%22numberOfChildren%22%3A0%2C%22numberOfRooms%22%3A1%2C%22itineraryNumber%22%3Anull%2C%22bookingStatus%22%3Anull%2C%22groupId%22%3A%22AL03PCN-3464559352%22%7D; rwd_id=2c24eb44-7792-49b4-9963-124d4c780489; wuc=USA; AWSALB=PsDr1YCgtxuwuh6SVZPeKDvgVPeI0pn3+s/dvC27KD3UH3oNE2VILZTgPh5pMpkhpB6VIziZJg80oBnQpoCtC6d6I/IoMb+sRcLscY/PBjmYDAxZ0a8PsReNF3jy; AWSALBCORS=PsDr1YCgtxuwuh6SVZPeKDvgVPeI0pn3+s/dvC27KD3UH3oNE2VILZTgPh5pMpkhpB6VIziZJg80oBnQpoCtC6d6I/IoMb+sRcLscY/PBjmYDAxZ0a8PsReNF3jy; akacd_PRC-RCI-GDP-PRD=3866640750~rv=26~id=36dba6ec4f78fe3baab1cef847d0a5bb; bgv=b2; jsess=ED0DF8474AE11F77BE8AEF9DB06F7C94");

                var response = httpClient.SendAsync(request).GetAwaiter().GetResult();
                response.EnsureSuccessStatusCode();
                
                var responseContent = await response.Content.ReadAsStringAsync();
                //log.LogInformation($"response contanet: {responseContent}");
                //log.LogInformation($"response: {response}");

                //--------------------------
                var responseBodyBytes = await response.Content.ReadAsByteArrayAsync();

            if (response.Content.Headers.ContentEncoding.Contains("gzip"))
            {
                responseBodyBytes = await DecompressGzip(responseBodyBytes);
            }

            var responseBodyString = Encoding.UTF8.GetString(responseBodyBytes);
                //-------------------------


                //log.LogInformation($"responseBodyString: {responseBodyString}");

                var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                //log.LogInformation("in 3");
                string jsonType = json.GetType().ToString();
                //log.LogInformation($"json type: {jsonType}");
                //log.LogInformation($"json: {json}");
                
                dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(responseBodyString);
                //log.LogInformation("in 4");
                //log.LogInformation($"data: {data}");
                int rawPrice = data?.sailDate?.price?.rawPrice ?? 0;
                //log.LogInformation("in 4");

                log.LogInformation($"Raw Price: {rawPrice}");

                if (rawPrice < 352) {
                    SendNotification(rawPrice, log);
                }
            }
            catch (Exception ex)
            {
                log.LogError($"Error occurred while processing the request: {ex}");
            }
        }

        private static void SendNotification(int price, ILogger log)
        {

            var httpClient = new HttpClient();
            var url = "https://prod-79.eastus.logic.azure.com:443/workflows/6168ca463c6a4adda2d47c6e528343ad/triggers/manual/paths/invoke?api-version=2016-10-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=_ABmcTg0QoXBJ9mjJPhvI90HnncrNfh3CA6wvjo6TwU";

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            var response = httpClient.SendAsync(request).GetAwaiter().GetResult();

            log.LogInformation($"Sent the email and the price was: {price}");
        }

        private static async Task<byte[]> DecompressGzip(byte[] compressedBytes)
        {
            using (var compressedStream = new MemoryStream(compressedBytes))
            using (var decompressedStream = new MemoryStream())
            using (var gzipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
            {
                await gzipStream.CopyToAsync(decompressedStream);
                return decompressedStream.ToArray();
            }
        }
        
    }
}