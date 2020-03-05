using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace TManagerAgent
{
    public class TManagerAgent : Mod
    {
        #region Const
        public const string LOCALHOST = "127.0.0.1";

        public const string DEFAULT_OVERSEER_ADDRESS = LOCALHOST;
        public const ushort DEFAULT_OVERSEER_PORT = 3006;

        public const ushort DEFAULT_INCOMING_PORT = 3005;
        #endregion

        #region Config
        public string OverseerAddress { get; private set; }
        public ushort OverseerPort { get; private set; }
        #endregion

        public TManagerAgent()
        {
        }

        #region Override
        public override void Load()
        {
            base.Load();

            LoadConfig(Main.netMode == NetmodeID.Server);
            
        }

        public override void PreSaveAndQuit()
        {
            base.PreSaveAndQuit();

            TManagerAgentWorld world = ModContent.GetInstance<TManagerAgentWorld>();
            world.PreCloseWorld();
        }
        #endregion

        #region Private
        public void LoadConfig(bool server)
        {
            IAgentConfig config = server ?
                (IAgentConfig)RawAgentConfig.Load() :
                (IAgentConfig)GetInstance<ClientAgentConfig>();

            OverseerAddress = config?.OverseerAddress ?? DEFAULT_OVERSEER_ADDRESS;
            OverseerPort = (ushort)(config?.OverseerPort ?? DEFAULT_OVERSEER_PORT);

            if (config is null)
            {
                Logger.Info("Config not loaded! Using defaults.");
            }
        }
        #endregion
    }
}