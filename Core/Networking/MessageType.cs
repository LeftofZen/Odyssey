namespace Odyssey.Networking
{
	public enum NetworkMessageType
	{
		Header,
		ChatMessage,

		#region Server-Specific
		LoginResponse,
		LogoutResponse,
		WorldUpdate,
		PlayerUpdate,
		#endregion

		#region Client-Specific
		Login,
		Logout,
		NetworkInput,
		#endregion
	}

}
