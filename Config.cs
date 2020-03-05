using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using static Terraria.ModLoader.ModContent;

namespace TManagerAgent
{
    public interface IAgentConfig
    {
        string OverseerAddress { get; set; }

        int OverseerPort { get; set; }
    }

    public class RawAgentConfig : IAgentConfig
    {
        #region Constructors
        public RawAgentConfig(string address, ushort port)
        {
            OverseerAddress = address;
            OverseerPort = port;
        }
        #endregion

        #region IAgentConfig
        public string OverseerAddress { get; set; } = TManagerAgent.DEFAULT_OVERSEER_ADDRESS;

        [Range(ushort.MinValue, ushort.MaxValue)]
        public int OverseerPort { get; set; } = TManagerAgent.DEFAULT_OVERSEER_PORT;
        #endregion

        #region Static Util
        public static RawAgentConfig Load()
        {
            // TODO ... actually load something.......
            return new RawAgentConfig(TManagerAgent.DEFAULT_OVERSEER_ADDRESS, TManagerAgent.DEFAULT_OVERSEER_PORT);
        }
        #endregion
    }

    public class ClientAgentConfig : ModConfig, IAgentConfig
    {
        #region Override
        public override ConfigScope Mode => ConfigScope.ClientSide;

        public override void OnChanged()
        {
            base.OnChanged();

            GetInstance<TManagerAgent>().LoadConfig(false);
        }
        #endregion

        #region IAgentConfig
        [Label("Overseer Address")]
        [DefaultValue(TManagerAgent.DEFAULT_OVERSEER_ADDRESS)]
        public string OverseerAddress { get; set; }
        [Label("Overseer Port")]
        [Range(ushort.MinValue, ushort.MaxValue)]
        [DefaultValue(TManagerAgent.DEFAULT_OVERSEER_PORT)]
        public int OverseerPort { get; set; }
        #endregion
    }
}
