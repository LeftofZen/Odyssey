using Odyssey.Networking.Messages;

namespace Odyssey.Networking
{
	public class MessageLookup : IMessageLookup<NetworkMessageType>
	{
		static readonly Dictionary<NetworkMessageType, Type> _ToType = new()
		{
			{ NetworkMessageType.LoginRequest, typeof(LoginRequest) },
			{ NetworkMessageType.LoginResponse, typeof(LoginResponse) },
			{ NetworkMessageType.LogoutRequest, typeof(LogoutRequest) },
			{ NetworkMessageType.LogoutResponse, typeof(LogoutResponse) },
			{ NetworkMessageType.InputUpdate, typeof(InputUpdate) },
			{ NetworkMessageType.PlayerUpdate, typeof(PlayerUpdate) },
			{ NetworkMessageType.WorldUpdate, typeof(WorldUpdate) },
			{ NetworkMessageType.ChatMessage, typeof(ChatMessage) },
			{ NetworkMessageType.Broadcast, typeof(BroadcastMessage) },
			{ NetworkMessageType.KeepAlive, typeof(KeepAliveMessage) },
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

		public IDictionary<NetworkMessageType, Type> ToType
			=> _ToType;

		public IDictionary<Type, NetworkMessageType> ToNetwork
			=> _ToNetwork;
	}

}
