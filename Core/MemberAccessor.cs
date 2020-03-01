using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TManagerAgent.Core
{
    public interface IMemberAccessor<T>
    {
        string MemberName { get; }
        Type MemberType { get; }

        object Get(T context);
        void Set(T context, object value);
    }


    public class PropertyAccessor<T> : IMemberAccessor<T>
    {
        public PropertyInfo Accessor { get; }

        #region IMemberAccessor Properties
        public string MemberName => Accessor?.Name;
        public Type MemberType => Accessor?.PropertyType;
        #endregion

        #region Constructor
        public PropertyAccessor(string name)
        {
            Accessor = typeof(T).GetProperty(name);
            if (Accessor is null)
            {
                throw new ArgumentException($"Property '{name}' cannot be found in type '{typeof(T)}'");
            }
        }
        #endregion

        #region IMemberAccessor Methods
        public object Get(T context) => Accessor.GetValue(context);

        public void Set(T context, object objectalue) => Accessor.SetValue(context, objectalue);
        #endregion
    }

    public class FieldAccessor<T> : IMemberAccessor<T>
    {
        public FieldInfo Accessor { get; }

        #region IMemberAccessor Properties
        public string MemberName => Accessor?.Name;
        public Type MemberType => Accessor?.FieldType;
        #endregion

        #region Constructor
        public FieldAccessor(string name)
        {
            Accessor = typeof(T).GetField(name);
            if (Accessor is null)
            {
                throw new ArgumentException($"Field '{name}' cannot be found in type '{typeof(T)}'");
            }
        }
        #endregion

        #region IMemberAccessor Methods
        public object Get(T context) => Accessor.GetValue(context);

        public void Set(T context, object objectalue) => Accessor.SetValue(context, objectalue);
        #endregion
    }

    /// <summary>
    /// Accessor that allows for custom getter and setter within the
    /// given context type. Can be used to get values that are within
    /// structs within the context. Example:
    /// 
    /// Player.lastDeathPosition.x (Context is Player)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CustomAccessor<T> : IMemberAccessor<T>
    {
        #region IMemberAccessor Properties
        public string MemberName { get; }
        public Type MemberType { get; }
        #endregion

        #region private
        // object Get(context)
        private Func<T, object> Getter { get; }

        // void Set(context, value)
        private Action<T, object> Setter { get; }
        #endregion

        #region Constructor
        public CustomAccessor(Func<T, object> getter, Action<T, object> setter, string name, Type memberType)
        {
            MemberName = name;
            MemberType = memberType;
            Getter = getter;
            Setter = setter;
        }
        #endregion

        #region IMemberAccessor Methods
        public object Get(T context) => Getter.Invoke(context);

        public void Set(T context, object objectalue) => Setter.Invoke(context, objectalue);
        #endregion
    }
}
