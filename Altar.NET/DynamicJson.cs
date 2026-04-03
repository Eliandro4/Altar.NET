using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static Newtonsoft.Json.Linq.JTokenType;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;

namespace Altar
{
    public class DynamicJson : DynamicObject
    {
        readonly static string[] EmptyStrArr = { };

        readonly JToken j;

        public DynamicJson(JToken json)
        {
            j = json;
        }

        static JToken ToJD(object value)
        {
            if (value is JToken)
                return (JToken)value;
            if (value is DynamicJson)
                return ((DynamicJson)value).j;

            return JToken.FromObject(value);
        }

        public override bool Equals(object obj) => j.Equals(obj);
        public override int GetHashCode() => j.GetHashCode() + 1;
        public override string ToString() => (string)j;

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            switch (j.Type)
            {
                case JTokenType.Object:
                    return ((IDictionary)j).Keys.ToGeneric<string>();
                // array?
            }

            return EmptyStrArr;
        }

        bool TryConvert(Type to, out object result)
        {
            if (to == typeof(void) || to.FullName == "Microsoft.FSharp.Core.Unit")
            {
                result = null;
                return j.Type == JTokenType.None;
            }

            if (to == typeof(string))
            {
                result = (string)j;
                return true;
            }
            if (to.IsArray)
            {
                if (to.GetElementType() == typeof(JToken))
                {
                    result = j.ToArray();
                    return true;
                }
                if (typeof(DynamicJson).Is(to))
                {
                    result = j.ToArray().Select(jd => new DynamicJson(jd));
                    return true;
                }
            }
            if (to.IsGenericType)
            {
                // won't be a non-constructed typedef, for obvious reasons
                if (to.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    var inner = to.GetGenericArguments()[0];

                    if (j.Type == JTokenType.None)
                    {
                        result = Activator.CreateInstance(to); // without inner value, hasValue is false
                        return true;
                    }
                    object value;
                    if (TryConvert(inner, out value))
                    {
                        result = Activator.CreateInstance(to, value);
                        return true;
                    }

                    result = null;
                    return false;
                }
            }

            if (typeof(Dictionary<string, DynamicJson>).Is(to))
            {
                result = ((JObject)j).Properties().ToDictionary(kvp => kvp.Name, kvp => new DynamicJson(kvp.Value));
                return true;
            }
            if (typeof(IDictionary<string, JToken>).Is(to))
            {
                result = ((JObject)j).Properties().ToDictionary(p => p.Name, p => (JToken)p.Value);
                return true;
            }
            if (typeof(Dictionary<string, JToken>).Is(to))
            {
                result = ((JObject)j).Properties().ToDictionary(kvp => kvp.Name, kvp => (JToken)kvp.Value);
                return true;
            }
            if (typeof(List<DynamicJson>).Is(to))
            {
                result = ((JArray)j).ToObject<List<DynamicJson>>();
                return true;
            }
            if (typeof(List<DynamicJson>).Is(to))
            {
                result = ((JArray)j).Select(jd => new DynamicJson(jd)).ToList();
                return true;
            }
            if (typeof(JToken).Is(to))
            {
                result = j;
                return true;
            }

            try
            {
                result = ((IConvertible)j).ToType(to, CultureInfo.CurrentCulture);
                return true;
            }
            catch (InvalidCastException) { }

            result = null;
            return false;
        }
        public override bool TryConvert(ConvertBinder binder, out object result) => TryConvert((Type)binder.Type, out result);

#pragma warning disable RECS0133 // Parameter name differs in base declaration -> fixed the bogus plural
        public override bool TryDeleteIndex(DeleteIndexBinder binder, object[] indices)
        {
            if (indices.Length != 1 || (j.Type != JTokenType.Array && j.Type != JTokenType.Object))
                return false;

            if (j.Type == JTokenType.Array && indices[0] is int)
            {
                var i = (int)indices[0];

                if (i < 0 || i >= ((JArray)j).Count)
                    return false;

                ((IList)j).RemoveAt(i);

                return true;
            }

            if (!(indices[0] is string))
                return false;

            return false; // Removed: JToken doesn't support direct Remove
        }
        public override bool TryGetIndex(GetIndexBinder binder, object[] indices, out object result)
        {
            result = null;

            if (indices.Length != 1 || (j.Type != JTokenType.Array && j.Type != JTokenType.Object))
                return false;

            if (indices[0] is int && j.Type == JTokenType.Array)
            {
                var i = (int)indices[0];

                if (i < 0 || i >= ((JArray)j).Count)
                    return false;

                result = new DynamicJson(j[i]);
                return true;
            }
            if (!(indices[0] is string))
                return false;

            var k = (string)indices[0];

            if (Has(j,k))
            {
                result = new DynamicJson(j[k]);
                return true;
            }

            return false;
        }
        public override bool TrySetIndex(SetIndexBinder binder, object[] indices, object value)
        {
            if (indices.Length != 1 || (j.Type != JTokenType.Array && j.Type != JTokenType.Object))
                return false;

            if (indices[0] is int && j.Type == JTokenType.Array)
            {
                var i = (int)indices[0];

                if (i < 0 || i >= ((JArray)j).Count)
                    return false;

                j[i] = ToJD(value);
                return true;
            }
            if (!(indices[0] is string))
                return false;

            var k = (string)indices[0];

            if (Has(j,k))
            {
                j[k] = ToJD(value);
                return true;
            }

            return false;
        }
#pragma warning restore RECS0133

        public override bool TryDeleteMember(DeleteMemberBinder binder)
        {
            if (j.Type != JTokenType.Object)
                return false;

            var name = binder.Name;

            return false; // Removed: JToken doesn't support direct Remove
        }
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = null;

            if (j.Type != JTokenType.Object)
                return false;

            if (Has(j,binder.Name))
            {
                result = new DynamicJson(j[binder.Name]);
                return true;
            }

            return false;
        }
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (j.Type != JTokenType.Object)
                return false;

            if (Has(j,binder.Name))
            {
                j[binder.Name] = ToJD(value);
                return true;
            }

            return false;
        }

        static bool Has(JToken t, string k) => t?[k] != null;
    }
}
