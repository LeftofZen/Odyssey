using Odyssey.Networking.Messages;

namespace Odyssey.Networking
{
	public class MessageLookup : IMessageLookup<NetworkMessageType>
	{
		static readonly Dictionary<uint, Type> _ToType = new()
		{
			{ (uint)NetworkMessageType.LoginRequest, typeof(LoginRequest) },
			{ (uint)NetworkMessageType.LoginResponse, typeof(LoginResponse) },
			{ (uint)NetworkMessageType.LogoutRequest, typeof(LogoutRequest) },
			{ (uint)NetworkMessageType.LogoutResponse, typeof(LogoutResponse) },
			{ (uint)NetworkMessageType.InputUpdate, typeof(InputUpdate) },
			{ (uint)NetworkMessageType.PlayerUpdate, typeof(PlayerUpdate) },
			{ (uint)NetworkMessageType.WorldUpdate, typeof(WorldUpdate) },
			{ (uint)NetworkMessageType.ChatMessage, typeof(ChatMessage) },
			{ (uint)NetworkMessageType.Broadcast, typeof(BroadcastMessage) },
			{ (uint)NetworkMessageType.KeepAlive, typeof(KeepAliveMessage) },
		};

		static readonly Dictionary<Type, NetworkMessageType> _ToNetwork = new()
		{
			{ typeof(LoginRequest), NetworkMessageType.LoginRequest },
			{ typeof(LoginResponse), NetworkMessageType.LoginResponse },
			{ typeof(LogoutRequest), NetworkMessageType.LogoutRequest },
			{ typeof(LogoutResponse), NetworkMessageType.LogoutResponse },
			{ typeof(InputUpdate), NetworkMessageType.InputUpdate },
			{ typeof(PlayerUpdate), NetworkMessageType.PlayerUpdate },
			{ typeof(WorldUpdate), NetworkMessageType.WorldUpdate },
			{ typeof(ChatMessage), NetworkMessageType.ChatMessage },
			{ typeof(BroadcastMessage), NetworkMessageType.Broadcast },
			{ typeof(KeepAliveMessage), NetworkMessageType.KeepAlive },
		};

		public IDictionary<uint, Type> ToType
			=> _ToType;

		public IDictionary<Type, NetworkMessageType> ToNetwork
			=> _ToNetwork;
	}

}
