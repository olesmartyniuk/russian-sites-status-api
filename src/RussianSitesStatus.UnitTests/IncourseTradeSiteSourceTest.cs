using RussianSitesStatus.Services;
using System.Threading.Tasks;
using Xunit;

namespace RussianSitesStatus.UnitTests.Services
{
    public class IncourseTradeSiteSourceTest
    {
        [Fact]
        public async Task GetAllAsync_HappyPath()
        {
            var incourseTradeSiteSource = new IncourseTradeSiteSource();
            var result = await incourseTradeSiteSource.GetAllAsync();
            Assert.NotEmpty(result);
        }
    }
}
