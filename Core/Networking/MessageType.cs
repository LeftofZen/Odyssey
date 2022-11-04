using Odyssey.Networking.Messages;

namespace Odyssey.Networking
{
	public enum NetworkMessageType
	{
		#region Server-Specific
		LoginResponse,
		LogoutResponse,
		WorldUpdate,
		PlayerUpdate,
		#endregion

		#region Client-Specific
		LoginRequest,
		LogoutRequest,
		InputUpdate,
		#endregion

		#region TwoWay
		ChatMessage,
		#endregion
	}

}
