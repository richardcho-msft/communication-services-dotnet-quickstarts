// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using IncomingCallRouting.Models;
using Microsoft.AspNetCore.Mvc;

namespace IncomingCallRouting.Controllers
{
    public class RestTestController : Controller
    {
        private readonly HttpClient _httpClient = new HttpClient();

        [HttpPost]
        [Route("rest-test")]
        public async Task<IActionResult> RestTest([FromBody] TestModel request)
        {
            try
            {
                Console.WriteLine($"Receiving Rest request: Count {request.Count} at {request.TimeStamp}.");

                await _httpClient.PostAsync(
                    "http:5000/server-response",
                    new StringContent(JsonSerializer.Serialize(new TestModel
                    {
                        Id = request.Id,
                        TimeStamp = request.TimeStamp,
                        Count = request.Count,
                    })));
            }
            catch (Exception e)
            {
                return Json(new { Exception = e });
            }

            return Accepted();
        }
    }
}