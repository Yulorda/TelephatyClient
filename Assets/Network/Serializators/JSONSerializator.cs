using Newtonsoft.Json;
using Newtonsoft.Json.UnityConverters.Math;

using System;
using System.Text;

using UnityEngine;

namespace Serializator
{
    internal class JSONSerializator : ISerializator
    {
        private JsonSerializerSettings serializerSettings;
        public JSONSerializator()
        {
            serializerSettings = new JsonSerializerSettings();
            //TODO: add all converters you need
        }

        public byte[] Serialize(object obj)
        {
            if (obj == null)
            {
                Debug.LogError("try send null");
                return new byte[0];
            }
            else
            {
                return Encoding.UTF8.GetBytes($"{obj.GetType().ToString()},{JsonConvert.SerializeObject(obj, serializerSettings)}");
            }
        }

        public bool TryDeserialize(byte[] message, out Type type, out object obj)
        {
            try
            {
                var json = Encoding.UTF8.GetString(message);
                var jsonsplit = json.Split(new char[] { ',' }, 2);
                type = Type.GetType(jsonsplit[0]);
                obj = JsonConvert.DeserializeObject(jsonsplit[1], type, serializerSettings);
                return true;
            }
            catch (Exception e)
            {
                Debug.Log("ERROR");
                obj = null;
                type = null;
                return false;
            }
        }
    }
}