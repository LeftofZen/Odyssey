namespace Odyssey.Messaging
{
	public enum NetworkMessageType
	{
		#region Server-Specific
		NULL = 0,
		LoginResponse,
		LogoutResponse,
		WorldUpdate,
		PlayerUpdate,
		GameStateUpdate,
		Broadcast,
		#endregion

		#region Client-Specific
		LoginRequest,
		LogoutRequest,
		InputUpdate,
		#endregion

		#region TwoWay
		ChatMessage,
		KeepAlive,
		#endregion
	}
}
