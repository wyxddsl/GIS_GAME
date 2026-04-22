using GISGameFramework.Core;
using GISGameFramework.Core.Contracts;
using GISGameFramework.Game.Serialization;

namespace GISGameFramework.Game.Modules
{
    public class DataTransferModule : IDataTransferService, IMessageRouter
    {
        private readonly IMessageSerializer _serializer;
        private readonly GameCoreManager _gameCoreManager;

        public DataTransferModule(GameCoreManager gameCoreManager)
        {
            _serializer = new JsonMessageSerializer();
            _gameCoreManager = gameCoreManager;
        }

        public ResponseResult<MessageEnvelope> BuildPushMessage(MessageType messageType, PayloadBase payload)
        {
            var envelope = new MessageEnvelope
            {
                Header = new MessageHeader { MessageType = messageType },
                Payload = payload
            };
            return ResponseFactory.Ok(envelope, "Push message built.");
        }

        public ResponseResult<MessageEnvelope> DeserializeEnvelope(string json)
        {
            return _serializer.Deserialize<MessageEnvelope>(json);
        }

        public ResponseResult<bool> Initialize()
        {
            return ResponseFactory.Ok(true, "Data transfer module initialized.");
        }

        public ResponseResult<MessageDispatchResult> Receive(MessageEnvelope envelope)
        {
            if (envelope == null || envelope.Header == null || envelope.Payload == null)
            {
                return ResponseFactory.Fail<MessageDispatchResult>(ErrorCodes.InvalidArgument, "Envelope is invalid.");
            }

            return Route(envelope);
        }

        public ResponseResult<MessageDispatchResult> Route(MessageEnvelope envelope)
        {
            switch (envelope.Header.MessageType)
            {
                case MessageType.C2S_POS:
                case MessageType.C2S_QUIZ_RESULT:
                case MessageType.C2S_USE_TR:
                    return _gameCoreManager.ProcessIncomingMessage(envelope);
                case MessageType.S2C_ENCOUNTER:
                case MessageType.S2C_TREASURE:
                case MessageType.S2C_ACH:
                case MessageType.RANKING_LIST:
                case MessageType.QUIZ_DATA:
                case MessageType.SERVER_ACK:
                case MessageType.QUIZ_START:
                case MessageType.PK_START:
                case MessageType.PK_RESULT:
                case MessageType.SCORE_CHANGED:
                case MessageType.NAV_HINT:
                    return ResponseFactory.Fail<MessageDispatchResult>(ErrorCodes.InvalidArgument, "Server message types cannot be routed as client input.");
                default:
                    return ResponseFactory.Fail<MessageDispatchResult>(ErrorCodes.RouteNotFound, "Unsupported message type.");
            }
        }

        public ResponseResult<string> SerializeEnvelope(MessageEnvelope envelope)
        {
            return _serializer.Serialize(envelope);
        }

        public ResponseResult<bool> Shutdown()
        {
            return ResponseFactory.Ok(true, "Data transfer module closed.");
        }
    }
}
