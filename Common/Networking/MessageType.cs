namespace Odyssey.Messaging
{
	public enum NetworkMessageType
	{
		#region Server-Specific
		LoginResponse,
		LogoutResponse,
		WorldUpdate,
		PlayerUpdate,
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
