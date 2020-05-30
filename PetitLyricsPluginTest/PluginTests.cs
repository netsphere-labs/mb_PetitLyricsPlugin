using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MusicBeePlugin.Tests
{
    [TestClass()]
    public class PluginTests
    {
        private Plugin plugin;

        [TestInitialize]
        public void Setup()
        {
            plugin = new Plugin();
        }

        [TestMethod()]
        public void GetProvidersTest()
        {
            string[] expected = { "プチリリ" };
            CollectionAssert.AreEqual(expected, plugin.GetProviders());
        }

        [TestMethod()]
        public void RetrieveLyricsTest()
        {
            string expected = "君が代は\r\n千代に八千代に\r\nさざれ石の\r\nいわおとなりて\r\nこけのむすまで\r\n\r\n";
            Assert.AreEqual(expected, plugin.RetrieveLyrics(null, "", "君が代", "", false, "プチリリ"));
        }
    }
}
