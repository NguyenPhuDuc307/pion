using Microsoft.AspNetCore.Mvc;
using pion_api.Controllers;
using pion_api.Models;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace pion_api.Tests.Controllers
{
    public class WeatherForecastControllerTests
    {
        [Fact]
        public void Get_ReturnsFiveForecasts()
        {
            // Arrange
            var controller = new WeatherForecastController();

            // Act
            var result = controller.Get();

            // Assert
            Assert.NotNull(result);
            var forecasts = result.ToList();
            Assert.Equal(5, forecasts.Count);
            foreach (var forecast in forecasts)
            {
                Assert.InRange(forecast.TemperatureC, -20, 55);
                Assert.False(string.IsNullOrEmpty(forecast.Summary));
            }
        }
    }
}
