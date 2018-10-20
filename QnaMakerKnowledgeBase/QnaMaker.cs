using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QnaMakerKnowledgeBase
{
    public class QnaMaker
    {
        static string host = "https://westus.api.cognitive.microsoft.com";
        static string service = "/qnamaker/v4.0";
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key">Replace this with a valid subscription key.</param>
        ///<param name="knowledgeBaseID"> Replace this with a valid knowledge base ID.</param>
        /// <param name="environnement"> Replace this with "test" or "prod".</param>
        /// <param name=""></param>
        /// <returns></returns>
        public async static Task<string> GetKnouwledgeBase(string key, string knowledgeBaseID, string environnement = "test", string method = "/knowledgebases/{0}/{1}/qna/")
        {
            string result = string.Empty;
            var method_with_id = String.Format(method, knowledgeBaseID, environnement);
            var uri = host + service + method_with_id;
            string uriResponse = await Get(uri, key);
            result = ObjectSerialize(uriResponse);
            return result;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key">Replace this with a valid subscription key.</param>
        ///<param name="knowledgeBaseID"> Replace this with a valid knowledge base ID.</param>
        /// <param name="environnement"> Replace this with "test" or "prod".</param>
        /// <param name=""></param>
        /// <returns></returns>
        public async static Task<string> DeleteKnouwledgeBase(string key, string knowledgeBaseID, string host = "https://westus.api.cognitive.microsoft.com", string method = "/knowledgebases/")
        {
            string result = string.Empty;
            var uri = host + service + method + knowledgeBaseID;
            var uriResponse = await Delete(uri, key);
            result = ObjectSerialize(uriResponse);
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key">Replace this with a valid subscription key.</param>
        ///<param name="knowledgeBaseID"> Replace this with a valid knowledge base ID.</param>
        /// <param name="environnement"> Replace this with "test" or "prod".</param>
        /// <param name=""></param>
        /// <returns></returns>
        public async static Task<string> CreateKnouwledgeBase(string key, string knowledgeBaseID, string method = "/knowledgebases/create")
        {
            string result = string.Empty;
            try
            {
                Response response = await Create(knowledgeBaseID, method, key);
                string operation = response.headers.GetValues("Location").First();

                result = ObjectSerialize(response.response);
                bool done = false;
                while (true != done)
                {
                    response = await GetStatus(operation, key);
                    result = ObjectSerialize(response.response);
                    var fields = JsonConvert.DeserializeObject<Dictionary<string, string>>(response.response);
                    String state = fields["operationState"];
                    if (state.CompareTo("Running") == 0 || state.CompareTo("NotStarted") == 0)
                    {
                        var wait = response.headers.GetValues("Retry-After").First();
                        Thread.Sleep(Int32.Parse(wait) * 1000);
                    }
                    else
                    {
                        done = true;
                    }
                }
            }
            catch
            {
                result = "Error";
            }
            finally
            {
                result = "Error";
            }
            return result;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key">Replace this with a valid subscription key.</param>
        ///<param name="knowledgeBaseID"> Replace this with a valid knowledge base ID.</param>
        /// <param name="environnement"> Replace this with "test" or "prod".</param>
        /// <param name=""></param>
        /// <returns></returns>
        public async static Task<string> UpdateKnouwledgeBase(string key, string knowledgeBaseID, string new_kb, string method = "/knowledgebases/")
        {
            string result = string.Empty;
            try
            {
                var response = await PatchUpdate(knowledgeBaseID, new_kb, method, key);
                var operation = response.headers.GetValues("Location").First();
                result = ObjectSerialize(response.response);
                var done = false;
                while (true != done)
                {
                    // Gets the status of the operation.
                    response = await GetStatus(operation, key);
                    result = ObjectSerialize(response.response);
                    var fields = JsonConvert.DeserializeObject<Dictionary<string, string>>(response.response);
                    String state = fields["operationState"];
                    if (state.CompareTo("Running") == 0 || state.CompareTo("NotStarted") == 0)
                    {
                        var wait = response.headers.GetValues("Retry-After").First();
                        Thread.Sleep(Int32.Parse(wait) * 1000);
                    }
                    else
                    {
                        done = true;
                    }
                }
            }
            catch (Exception ex)
            {
                result = "An error occurred while updating the knowledge base." + ex.InnerException;
            }
            finally
            {
                result = "Error";
            }
            return result;
        }
        #region private
        private struct Response
        {
            public HttpResponseHeaders headers;
            public string response;

            public Response(HttpResponseHeaders headers, string response)
            {
                this.headers = headers;
                this.response = response;
            }
        }
        private async static Task<string> Get(string uri, string key)
        {
            using (HttpClient httpClient = new HttpClient())
            using (HttpRequestMessage httpRequestMessage = new HttpRequestMessage())
            {
                httpRequestMessage.Method = HttpMethod.Get;
                httpRequestMessage.RequestUri = new Uri(uri);
                httpRequestMessage.Headers.Add("Ocp-Apim-Subscription-Key", key);

                var response = await httpClient.SendAsync(httpRequestMessage);
                return await response.Content.ReadAsStringAsync();
            }
        }
        private async static Task<string> Delete(string uri, string key)
        {
            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Delete;
                request.RequestUri = new Uri(uri);
                request.Headers.Add("Ocp-Apim-Subscription-Key", key);

                var response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    return "{'result' : 'Success.'}";
                }
                else
                {
                    return await response.Content.ReadAsStringAsync();
                }
            }
        }
        private async static Task<Response> Create(string kb, string method, string key)
        {
            string uri = host + service + method;
            using (HttpClient httpClient = new HttpClient())
            using (HttpRequestMessage httpRequestMessage = new HttpRequestMessage())
            {
                httpRequestMessage.Method = HttpMethod.Post;
                httpRequestMessage.RequestUri = new Uri(uri);
                httpRequestMessage.Content = new StringContent(kb, Encoding.UTF8, "application/json");
                httpRequestMessage.Headers.Add("Ocp-Apim-Subscription-Key", key);

                var responseAsync = await httpClient.SendAsync(httpRequestMessage);
                var responseBodyAsync = await responseAsync.Content.ReadAsStringAsync();
                return new Response(responseAsync.Headers, responseBodyAsync);
            }
        }
        private async static Task<Response> GetStatus(string operationID, string key)
        {

            string uri = host + service + operationID;
            using (HttpClient httpClient = new HttpClient())
            using (HttpRequestMessage httpRequestMessage = new HttpRequestMessage())
            {
                httpRequestMessage.Method = HttpMethod.Get;
                httpRequestMessage.RequestUri = new Uri(uri);
                httpRequestMessage.Headers.Add("Ocp-Apim-Subscription-Key", key);

                var responseAsync = await httpClient.SendAsync(httpRequestMessage);
                var responseBodyAsync = await responseAsync.Content.ReadAsStringAsync();
                return new Response(responseAsync.Headers, responseBodyAsync);
            }
        }
        async static Task<Response> PatchUpdate(string kb, string new_kb, string method, string key)
        {
            string uri = host + service + method + kb;
            Console.WriteLine("Calling " + uri + ".");

            using (HttpClient httpClient = new HttpClient())
            using (var httpRequestMessage = new HttpRequestMessage())
            {
                httpRequestMessage.Method = new HttpMethod("PATCH");
                httpRequestMessage.RequestUri = new Uri(uri);

                httpRequestMessage.Content = new StringContent(new_kb, Encoding.UTF8, "application/json");
                httpRequestMessage.Headers.Add("Ocp-Apim-Subscription-Key", key);

                var responseAsync = await httpClient.SendAsync(httpRequestMessage);
                var responseBodyAsync = await responseAsync.Content.ReadAsStringAsync();
                return new Response(responseAsync.Headers, responseBodyAsync);
            }
        }
        private static string ObjectSerialize(string s)
        {
            return JsonConvert.SerializeObject(JsonConvert.DeserializeObject(s), Formatting.Indented);
        }

        #endregion


    }


}
