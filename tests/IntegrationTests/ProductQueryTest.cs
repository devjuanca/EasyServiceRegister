using Application.Common.Dto;
using Application.Cqrs.Products.Query;
using FluentAssertions;
using NUnit.Framework;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace IntegrationTests
{
    public class ProductQueryTest
    {
        [Test]
        public async Task ShouldGetProductsList()
        {
            //Arrange
            using var app = new TestApplicationFactory();

            var client = app.CreateClient();

            HttpContent content = new StringContent(JsonSerializer.Serialize(new { Name = "Product Name", Description = "Product Description" }), Encoding.UTF8, "application/json");

            await client.PostAsync("/api/addProduct", content);


            //Act
            var response = await client.GetAsync("/api/getProducts");

            var results = JsonSerializer.Deserialize<List<ProductDto>>(await response.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });


            //Assert
            response.IsSuccessStatusCode.Should().BeTrue();

            results.Should().NotBeNull();

            results.Count.Should().Be(1);

            results?[0].Name.Should().Be("Product Name");
        }
    }
}
