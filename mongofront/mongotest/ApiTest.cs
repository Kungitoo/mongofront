using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using mongofront;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace mongotest
{
    public class ApiTest
    {
        private readonly TestServer _server;
        private readonly HttpClient _client;

        public ApiTest()
        {
            // Arrange
            _server = new TestServer(new WebHostBuilder()
               .UseStartup<Startup>());

            _client = _server.CreateClient();
        }

        [Fact]
        public async Task Test1()
        {
            // Act
            var response = await _client.GetAsync("weatherforecast");
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            // Assert
            Assert.Equal("Hello World!", responseString);
        }
    }
}
