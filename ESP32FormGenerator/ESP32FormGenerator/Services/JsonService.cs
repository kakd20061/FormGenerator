using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ESP32FormGenerator.Services
{
    public static class JsonService
    {
        public async static Task<string> GetFile(string path)
        {
            using (var http = new HttpClient())
            {
                var response = await http.GetStringAsync(path);
                return response;
            }
        }

    }
}