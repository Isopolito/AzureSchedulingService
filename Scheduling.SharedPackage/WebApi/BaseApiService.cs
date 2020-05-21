using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Scheduling.SharedPackage.Exceptions;
using Scheduling.SharedPackage.Extensions;

namespace Scheduling.SharedPackage.WebApi
{
    public abstract class BaseApiService
    {
        private readonly IHttpClientFactory httpClientFactory;

        public string BaseAddress { get; set; }

        protected BaseApiService(IHttpClientFactory httpClientFactory, string baseAddress)
        {
            this.httpClientFactory = httpClientFactory;
            BaseAddress = baseAddress;
        }

        private HttpClient GetHttpClient()
        {
            var client = httpClientFactory.CreateClient("ATIFunction");
            client.BaseAddress = new Uri(BaseAddress);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }

        private static StringContent GetStringHttpContent(string jsonToPost) { return new StringContent(jsonToPost, Encoding.UTF8, "application/json"); }

        private static async Task<TDto> ParseResponse<TDto>(HttpResponseMessage response)
        {
            var responseBody = await response.Content.ReadAsStringAsync();

            switch (response.StatusCode)
            {
                case HttpStatusCode.NotFound:
                    return default(TDto);
                case HttpStatusCode.BadRequest:
                    var brokenRules = JsonConvert.DeserializeObject<IEnumerable<string>>(responseBody);
                    throw new ValidationException(brokenRules);
                case HttpStatusCode.Unauthorized:
                    var uri = response.RequestMessage.RequestUri.AbsoluteUri;
                    throw new UnauthorizedException(uri);
                case HttpStatusCode.InternalServerError:
                    throw new ServiceException(responseBody);
                default:
                    return JsonConvert.DeserializeObject<TDto>(responseBody);
            }
        }

        private static async Task ParseResponse(HttpResponseMessage response)
        {
            var responseBody = await response.Content.ReadAsStringAsync();

            switch (response.StatusCode)
            {
                case HttpStatusCode.BadRequest:
                    var brokenRules = JsonConvert.DeserializeObject<IEnumerable<string>>(responseBody);
                    throw new ValidationException(brokenRules);
                case HttpStatusCode.Unauthorized:
                    var uri = response.RequestMessage.RequestUri.AbsoluteUri;
                    throw new UnauthorizedException(uri);
                case HttpStatusCode.InternalServerError:
                    throw new ServiceException(responseBody);
            }
        }

        private string CreateUri(string functionKey, string path = "")
        {
            var uriPath = functionKey.HasValue() ? $"{path}?code={functionKey}" : path;
            return CombineUri(BaseAddress, uriPath);
        }

        protected async Task<TDto> GetAsync<TDto>(string path, string functionKey = "", List<KeyValuePair<string, string>> headers = null)
        {
            var resource = CreateUri(functionKey, path);
            var client = GetHttpClient();

            headers?.ForEach(kvp => client.DefaultRequestHeaders.Add(kvp.Key, kvp.Value));

            var response = await client.GetAsync(resource);
            return await ParseResponse<TDto>(response);
        }

        protected async Task<TDto> PostAsync<TDto>(string json, string path, string functionKey = "")
        {
            var resource = CreateUri(functionKey, path);
            var content = GetStringHttpContent(json);
            var client = GetHttpClient();
            var response = await client.PostAsync(resource, content);

            return await ParseResponse<TDto>(response);
        }

        protected async Task PostAsync(string json, string path, string functionKey = "")
        {
            var resource = CreateUri(functionKey, path);
            var content = GetStringHttpContent(json);
            var client = GetHttpClient();
            var response = await client.PostAsync(resource, content);

            await ParseResponse(response);
        }

        protected async Task<TDto> PutAsync<TDto>(string json, string path, string functionKey = "")
        {
            var resource = CreateUri(functionKey, path);
            var content = GetStringHttpContent(json);
            var client = GetHttpClient();
            var response = await client.PutAsync(resource, content);

            return await ParseResponse<TDto>(response);
        }

        protected async Task PutAsync(string json, string path, string functionKey = "")
        {
            var resource = CreateUri(functionKey, path);
            var content = GetStringHttpContent(json);
            var client = GetHttpClient();
            var response = await client.PutAsync(resource, content);

            await ParseResponse(response);
        }

        protected async Task DeleteAsync(string path, string functionKey = "")
        {
            var resource = CreateUri(functionKey, path);
            var client = GetHttpClient();
            await client.DeleteAsync(resource);
        }

        public static string CombineUri(params string[] uriParts)
        {
            var uri = string.Empty;
            if (uriParts == null || uriParts.Length <= 0) return uri;

            var trims = new[] { '\\', '/' };
            uri = (uriParts[0] ?? string.Empty).TrimEnd(trims);
            for (var i = 1; i < uriParts.Length; i++)
            {
                uri = $"{uri.TrimEnd(trims)}/{(uriParts[i] ?? string.Empty).TrimStart(trims)}";
            }
            return uri;
        }
    }
}
