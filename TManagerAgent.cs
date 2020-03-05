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
        public const ushort DEFAULT_OVERSEER_TCP_PORT = 3007;
        public const ushort DEFAULT_OVERSEER_UDP_PORT = 3008;
        #endregion

        #region Config
        public string OverseerAddress { get; private set; }
        public ushort OverseerTCPPort { get; private set; }
        public ushort OverseerUDPPort { get; private set; }

        public ushort LocalTCPPort { get; private set; }

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
            OverseerTCPPort = (ushort)(config?.OverseerTCPPort ?? DEFAULT_OVERSEER_TCP_PORT);
            OverseerUDPPort = (ushort)(config?.OverseerUDPPort ?? DEFAULT_OVERSEER_UDP_PORT);

            if (config is null)
            {
                Logger.Info("Config not loaded! Using defaults.");
            }
        }
        #endregion
    }
}