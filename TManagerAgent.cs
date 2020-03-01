using Terraria.ModLoader;

namespace TManagerAgent
{
    public class TManagerAgent : Mod
    {
        public TManagerAgent()
        {
            

        }

        public override void PreSaveAndQuit()
        {
            base.PreSaveAndQuit();

            TManagerAgentWorld world = ModContent.GetInstance<TManagerAgentWorld>();
            world.PreCloseWorld();
        }
    }
}