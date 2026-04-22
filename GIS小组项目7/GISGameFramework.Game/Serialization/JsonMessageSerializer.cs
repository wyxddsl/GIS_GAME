using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using GISGameFramework.Core;
using GISGameFramework.Core.Contracts;

namespace GISGameFramework.Game.Serialization
{
    public class JsonMessageSerializer : IMessageSerializer
    {
        public ResponseResult<T> Deserialize<T>(string content) where T : class
        {
            try
            {
                var serializer = new DataContractJsonSerializer(typeof(T));
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(content)))
                {
                    return ResponseFactory.Ok((T)serializer.ReadObject(stream), "Deserialize succeeded.");
                }
            }
            catch (Exception ex)
            {
                return ResponseFactory.Fail<T>(ErrorCodes.SerializationFailed, "Deserialize failed: " + ex.Message);
            }
        }

        public ResponseResult<string> Serialize(MessageEnvelope envelope)
        {
            try
            {
                var serializer = new DataContractJsonSerializer(typeof(MessageEnvelope));
                using (var stream = new MemoryStream())
                {
                    serializer.WriteObject(stream, envelope);
                    return ResponseFactory.Ok(Encoding.UTF8.GetString(stream.ToArray()), "Serialize succeeded.");
                }
            }
            catch (Exception ex)
            {
                return ResponseFactory.Fail<string>(ErrorCodes.SerializationFailed, "Serialize failed: " + ex.Message);
            }
        }
    }
}
