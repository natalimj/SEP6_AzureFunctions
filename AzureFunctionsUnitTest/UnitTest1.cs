using Xunit;

namespace AzureFunctionsUnitTest
{

    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            Assert.True(true);
        }
        
        public void Test2()
        {
            Assert.True(false);
        }
        
        public void Test3()
        {
            Assert.True(true);
        }
    }
}