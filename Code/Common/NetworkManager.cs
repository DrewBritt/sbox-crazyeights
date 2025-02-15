namespace CrazyEights.Common;

public sealed class NetworkManager : Component, Component.INetworkListener
{
	[Property] public GameObject PlayerPrefab { get; set; }

	protected override void OnStart()
	{
		if ( Networking.IsActive ) return;

		var lobbyConfig = new LobbyConfig
		{
			DestroyWhenHostLeaves = false,
			AutoSwitchToBestHost = true,
			MaxPlayers = 8,
			Privacy = LobbyPrivacy.Private,
		};
		Networking.CreateLobby( lobbyConfig );
	}

	public void OnActive( Connection connection )
	{
		var player = PlayerPrefab.Clone();
		player.Name = $"Player - {connection.DisplayName}";
		player.Network.SetOwnerTransfer( OwnerTransfer.Fixed );
		player.NetworkSpawn( connection );
	}
}
