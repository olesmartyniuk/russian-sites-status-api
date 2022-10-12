using RussianSitesStatus.Services;
using System.Threading.Tasks;
using Xunit;

namespace RussianSitesStatus.UnitTests.Services
{
    public class IncourseTradeSiteSourceTest
    {
        [Fact]
        public async Task GetAll_HappyPath()
        {
            var incourseTradeSiteSource = new IncourseTradeSiteSource();
            var result = await incourseTradeSiteSource.GetAll();
            Assert.NotEmpty(result);
        }
    }
}
