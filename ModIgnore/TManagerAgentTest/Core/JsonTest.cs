using Newtonsoft.Json;
using NUnit.Framework;
using tManagerAgent.Core;

namespace tManagerAgentTest.Core
{
    [TestFixture]
    class JsonTest
    {
        private JsonSerializerSettings JsonSerializerSettings;

        private MemberRoute BasicRoute1 = new MemberRoute(MemberContext.StaticMainMemberContext, "myProperty");
        private MemberRoute BasicRoute2 = new MemberRoute(MemberContext.StaticMainMemberContext, "myField");

        [OneTimeSetUp]
        public void Begin()
        {
            JsonSerializerSettings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented
            };
            JsonConvert.DefaultSettings = () => JsonSerializerSettings;
        }


        #region Helper Functions
        private static string SerializeAndPrint(object toSerialize)
        {
            string serialized = JsonConvert.SerializeObject(toSerialize);

            TestContext.WriteLine($"TYPE {toSerialize.GetType()} SERIALIZED TO:");
            TestContext.WriteLine(serialized);

            return serialized;
        }
        #endregion

        #region MemberRoute
        [Test]
        public void Test_MemberRoute_Serialize()
        {
            MemberRoute route = BasicRoute1;

            var output = SerializeAndPrint(route);
            Assert.That(output != null && output.Length > 0);
        }

        [Test]
        public void Test_MemberRoute_Deserialize()
        {
            string input = @"{
  'Context': {
    'ContextType': 'Main',
    'ContextKey': null
  },
  'AccessorKey': 'raining',
  'Instance': false
}";

            MemberRoute route = JsonConvert.DeserializeObject<MemberRoute>(input);

            Assert.That(route.Context.ContextType == MemberContextType.Main);
            Assert.That(route.Context.ResolvedContext == null);
            Assert.That(route.Context.Static);
            Assert.That(route.AccessorKey == "raining");
            Assert.That(!route.Instance);
        }

        [Test]
        public void Test_MemberRoute_Full()
        {
            MemberRoute route = new MemberRoute(MemberContext.StaticMainMemberContext, "raining");
            var sameRoute = JsonConvert.DeserializeObject<MemberRoute>(JsonConvert.SerializeObject(route));

            Assert.That(sameRoute, Is.EqualTo(route));
        }
        #endregion


    }
}
