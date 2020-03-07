using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace tManagerAgent.Core
{
    /// <summary>
    /// Container for all information necessary for <see cref="ValueController"/>
    /// to find a game variable.
    /// </summary>
    public class MemberRoute
    {
        /// <summary>
        /// Context in which <see cref="AccessorKey"/> applies. Includes parent
        /// type and, if applicable, instance.
        /// </summary>
        public MemberContext Context { get; set; }
        /// <summary>
        /// String key that determines targeted member within a context.
        /// </summary>
        public string AccessorKey { get; set; }
        /// <summary>
        /// Whether or not the route applies to an instance of the 
        /// context's type.
        /// </summary>
        public bool Instance => !Context.Static;

        #region Constructor
        public MemberRoute(MemberContext context, string key)
        {
            Context = context;
            AccessorKey = key;
        }
        #endregion

        #region Override
        public override bool Equals(object obj)
        {
            if(obj is MemberRoute route)
            {
                return Context.Equals(route.Context) &&
                    AccessorKey == route.AccessorKey;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Context.GetHashCode() + AccessorKey.GetHashCode();
        }
        #endregion
    }

    /// <summary>
    /// Container for all information necessary to determine context
    /// when using <see cref="ValueController"/> to find game variables.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class MemberContext
    {
        #region Public properties
        [JsonProperty]
        public MemberContextType ContextType { get; set; }

        /// <summary>
        /// Used to resolve instance of context type.
        /// If null, implies static context.
        /// </summary>
        [JsonProperty]
        public string ContextKey { get; set; }
        public object ResolvedContext
        {
            get
            {
                if (string.IsNullOrEmpty(ContextKey)) return null;
                return ResolveContext(ContextKey);
            }
        }

        /// <summary>
        /// False if 
        /// </summary>
        public bool Static => ContextKey is null;
        #endregion

        #region Constructors
        public MemberContext(MemberContextType type, string contextKey)
        {
            ContextType = type;
            ContextKey = contextKey;
        }
        #endregion

        #region Helper Methods
        private object ResolveContext(string key)
        {
            switch (ContextType)
            {
                case MemberContextType.World:
                    return Terraria.Main.Map;
                case MemberContextType.Player:
                    // TODO - Have the Mod maintain its own list of keys instead of doing this
                    return Terraria.Main.player.FirstOrDefault(player => player.name == key);
                case MemberContextType.Main:
                default:
                    return null;
            }
        }
        #endregion

        #region Common Contexts
        /// <summary>
        /// Context for static members of <see cref="Terraria.Main"/>
        /// </summary>
        public static readonly MemberContext StaticMainMemberContext = new MemberContext(MemberContextType.Main, null);

        /// <summary>
        /// Context for static members of <see cref="Terraria.Map.WorldMap"/>
        /// </summary>
        public static readonly MemberContext StaticWorldMemberContext = new MemberContext(MemberContextType.World, null);

        /// <summary>
        /// Context for static members of <see cref="Terraria.Player"/>
        /// </summary>
        public static readonly MemberContext StaticPlayerMemberContext = new MemberContext(MemberContextType.Player, null);
        #endregion

        #region Override
        public override bool Equals(object obj)
        {
            if(obj is MemberContext other)
            {
                return ContextType == other.ContextType &&
                    ContextKey == other.ContextKey;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return ContextType.GetHashCode() + ContextKey.GetHashCode();
        }
        #endregion
    }

    /// <summary>
    /// All supported game variable contexts (parent classes).
    /// </summary>
    [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    public enum MemberContextType
    {
        Main,
        World,
        Player
    }
}
