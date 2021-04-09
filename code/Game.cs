﻿using Sandbox;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HiddenGamemode
{
	[ClassLibrary( "sbox-hidden", Title = "Hidden" )]
	partial class Game : Sandbox.Game
	{
		public static Game Instance
		{
			get => Current as Game;
		}

		public HiddenTeam HiddenTeam { get; set; }
		public IrisTeam IrisTeam { get; set; }
		public Hud Hud { get; set; }

		[Net] public BaseRound Round { get; private set; }

		private List<BaseTeam> _teams;

		[ServerVar( "hdn_min_players", Help = "The minimum players required to start.", Name = "Minimum Players" )]
		private int _minPlayers => 2;

		public Game()
		{
			_teams = new();

			if ( IsServer )
			{
				Hud = new Hud();
			}

			HiddenTeam = new HiddenTeam();
			IrisTeam = new IrisTeam();

			AddTeam( HiddenTeam );
			AddTeam( IrisTeam );

			_ = StartTickTimer();
		}

		public void AddTeam( BaseTeam team )
		{
			_teams.Add( team );
			team.Index = _teams.Count;
		}

		public BaseTeam GetTeamByIndex( int index )
		{
			return _teams[index - 1];
		}

		public List<Player> GetTeamPlayers<T>(bool isAlive = false) where T : BaseTeam
		{
			var output = new List<Player>();

			Sandbox.Player.All.ForEach( ( p ) =>
			{
				if ( p is Player player && player.Team is T )
				{
					if ( !isAlive || player.LifeState == LifeState.Alive )
					{
						output.Add( player );
					}
				}
			} );

			return output;
		}

		public void ChangeRound(BaseRound round)
		{
			Assert.NotNull( round );

			Round?.Finish();
			Round = round;
			Round?.Start();
		}

		public async Task StartSecondTimer()
		{
			while (true)
			{
				await Task.DelaySeconds( 1 );
				OnSecond();
			}
		}

		public async Task StartTickTimer()
		{
			while (true)
			{
				await Task.Delay( 100 );
				OnTick();
			}
		}

		public override void DoPlayerNoclip( Sandbox.Player player )
		{
			// Do nothing. The player can't noclip in this mode.
		}

		public override void DoPlayerSuicide( Sandbox.Player player )
		{
			// Do nothing. The player can't suicide in this mode.
			base.DoPlayerSuicide( player );
		}

		public override void PostLevelLoaded()
		{
			_ = StartSecondTimer();

			base.PostLevelLoaded();
		}

		public override void PlayerKilled( Sandbox.Player player )
		{
			Round?.OnPlayerKilled( player as Player );

			base.PlayerKilled( player );
		}

		public override void PlayerDisconnected( Sandbox.Player player, NetworkDisconnectionReason reason )
		{
			Log.Info( player.Name + " left, checking minimum player count..." );

			Round?.OnPlayerLeave( player as Player );

			base.PlayerDisconnected( player, reason );
		}

		public override Player CreatePlayer() => new();

		private void OnSecond()
		{
			CheckMinimumPlayers();
			Round?.OnSecond();
		}

		private void OnTick()
		{
			if ( IsClient )
			{
				Sandbox.Player.All.ForEach( ( player ) =>
				{
					if ( player is not Player hiddenPlayer ) return;

					if ( hiddenPlayer.TeamIndex != hiddenPlayer.LastTeamIndex )
					{
						hiddenPlayer.Team = GetTeamByIndex( hiddenPlayer.TeamIndex );
						hiddenPlayer.LastTeamIndex = hiddenPlayer.TeamIndex;
					}
				} );
			}
		}

		private void CheckMinimumPlayers()
		{
			if ( Sandbox.Player.All.Count >= _minPlayers)
			{
				if ( Round is LobbyRound || Round == null )
				{
					ChangeRound( new HideRound() );
				}
			}
			else if ( Round is not LobbyRound )
			{
				ChangeRound( new LobbyRound() );
			}
		}
	}
}
