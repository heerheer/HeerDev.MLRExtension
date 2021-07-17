using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HeerDev.MLRExtension.Helper
{
    internal class Http
    {
        /// <summary>
        /// 使用dic发送Post请求，FormUrlEncoded
        /// </summary>
        /// <param name="url"></param>
        /// <param name="dic"></param>
        /// <returns></returns>
        public static async Task<string> Post(string url, Dictionary<string, string> dic)
        {

            using (var c = new HttpClient())
            {
                var result = await c.PostAsync(url, new FormUrlEncodedContent(dic));
                return await result.Content.ReadAsStringAsync();
            }
        }

        /// <summary>
        /// 使用jsonBody发送Post请求
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static async Task<string> Post<T>(string url, T obj)
        {
            using (var c = new HttpClient())
            {
                var content = new StringContent(JsonSerializer.Serialize(obj));
                content.Headers.Add("Content-Type", "application/json");
                var result = await c.PostAsync(url, content);
                return await result.Content.ReadAsStringAsync();
            }
        }
        public static async Task<string> Post(string url, string json)
        {
            using (var c = new HttpClient())
            {
                var content = new StringContent(json);
                content.Headers.Remove("Content-Type");
                content.Headers.Add("Content-Type", "application/json");
                var result = await c.PostAsync(url, content);
                return await result.Content.ReadAsStringAsync();
            }
        }
        /// <summary>
        /// 使用dic发送Get请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="dic"></param>
        /// <returns></returns>
        public static async Task<string> Get(string url, Dictionary<string, string> dic)
        {
            using (var c = new HttpClient())
            {
                var result = await c.GetAsync($"{url}?{string.Join('&', dic.ToList().Select(x => x.Key + "=" + x.Value))}");

                return await result.Content.ReadAsStringAsync();
            }
        }

        /// <summary>
        /// Get请求
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static async Task<string> Get(string url)
        {
            using (var c = new HttpClient())
            {
                var result = await c.GetAsync(url);
                return await result.Content.ReadAsStringAsync();
            }
        }

        /// <summary>
        /// Get请求
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static async Task<string> Get(string url,KeyValuePair<string,string> header)
        {
            using (var c = new HttpClient())
            {
                c.DefaultRequestHeaders.Add(header.Key,header.Value);
                var result = await c.GetAsync(url);
                return await result.Content.ReadAsStringAsync();
            }
        }
    }
}
