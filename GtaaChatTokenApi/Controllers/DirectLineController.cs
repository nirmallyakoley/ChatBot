using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;





namespace GtaaChatTokenApi.Controllers
{
    [Route("directline/token")]
    [ApiController]
    public class DirectLineController : ControllerBase
    {
        [HttpPost]

        [EnableCors("AllowOrigin")]
        public async Task<string> token()
        {
            try
            {
                string token = string.Empty;
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "7Obrf17gZSQ.5e49AwzNQeKWZz-Eohsdgac1o6uia-6r4F6nRRdqGVk");

                    var rsp = await client.PostAsync("https://directline.botframework.com/v3/directline/tokens/generate", new StringContent(string.Empty, Encoding.UTF8, "application/json"));

                    if (rsp.IsSuccessStatusCode)
                    {
                        token = await rsp.Content.ReadAsStringAsync();
                        return token;
                    }
                    else
                    {
                        return token;
                    }
                    
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            } 
        }
    }
}