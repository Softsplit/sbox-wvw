using Sandbox.Network;
using System;
using System.Threading.Tasks;

[Title( "Modified Network Helper" )]
[Category( "Networking" )]
[Icon( "electrical_services" )]
public sealed class ModdedNetworkHelper : Component, Component.INetworkListener
{
	/// <summary>
	/// Create a server (if we're not joining one)
	/// </summary>
	[Property] public bool StartServer { get; set; } = true;

	/// <summary>
	/// The prefab to spawn for the player to control.
	/// </summary>
	[Property] public GameObject PlayerPrefab { get; set; }

	/// <summary>
	/// A list of points to choose from randomly to spawn the player in. If not set, we'll spawn at the
	/// location of the NetworkHelper object.
	/// </summary>
	[Property] public List<GameObject> SpawnPoints { get; set; }
	[Property] public List<Respawn> Respawns  { get; set; }
	[Property] public float RespawnTime { get; set; } = 5f;

	public class Respawn
	{
		public Connection channel {get;set;}
		public float DeathTime {get;set;}
	}

	protected override async Task OnLoad()
	{
		if ( Scene.IsEditor )
			return;

		if ( StartServer && !GameNetworkSystem.IsActive )
		{
			LoadingScreen.Title = "Creating Lobby";
			await Task.DelayRealtimeSeconds( 0.1f );
			GameNetworkSystem.CreateLobby();
		}
	}

	protected override void OnStart()
	{
		
	}

	/// <summary>
	/// A client is fully connected to the server. This is called on the host.
	/// </summary>
	public void OnActive( Connection channel )
	{
		Log.Info( $"Player '{channel.DisplayName}' has joined the game" );

		if ( PlayerPrefab is null )
			return;
		
		Spawn(channel);
	}

	public void Spawn( Connection channel )
	{
		var startLocation = FindSpawnLocation().WithScale( 1 );

		var player = PlayerPrefab.Clone( startLocation, name: $"Player - {channel.DisplayName}" );
		player.NetworkSpawn( channel );
	}

	/// <summary>
	/// Find the most appropriate place to respawn
	/// </summary>
	Transform FindSpawnLocation()
	{
		//
		// If they have spawn point set then use those
		//
		if ( SpawnPoints is not null && SpawnPoints.Count > 0 )
		{
			return Random.Shared.FromList( SpawnPoints, default ).Transform.World;
		}

		//
		// If we have any SpawnPoint components in the scene, then use those
		//
		var spawnPoints = Scene.GetAllComponents<SpawnPoint>().ToArray();
		if ( spawnPoints.Length > 0 )
		{
			return Random.Shared.FromArray( spawnPoints ).Transform.World;
		}

		//
		// Failing that, spawn where we are
		//
		return Transform.World;
	}

	protected override void OnFixedUpdate()
	{
		if ( !Networking.IsHost ) return;
		base.OnFixedUpdate();
		List<Respawn> RemoveRespawns = new List<Respawn>();
		foreach(Respawn respawn in Respawns)
		{
			if(Time.Now - respawn.DeathTime < RespawnTime) continue;
			Spawn(respawn.channel);
			RemoveRespawns.Add(respawn);
		}

		foreach(Respawn respawn in RemoveRespawns)
		{
			Respawns.Remove(respawn);
		}
	}

	[Broadcast]
	public void AddRespawn(Guid channel)
	{
		if ( !Networking.IsHost ) return;
		Respawn respawn = new Respawn
		{
			channel = Connection.Find(channel),
			DeathTime = Time.Now
		};
		Respawns.Add(respawn);
	}
}
