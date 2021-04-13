﻿using Sandbox;

namespace HiddenGamemode
{
	public partial class SpectateCamera : BaseCamera
	{
		[NetPredicted] public TimeSince TimeSinceDied { get; set; }
		[NetPredicted] public Vector3 DeathPosition { get; set; }

		public Player TargetPlayer { get; set; }

		private Vector3 _focusPoint;

		public override void Activated()
		{
			base.Activated();

			_focusPoint = LastPos - GetViewOffset();

			FieldOfView = 70;
		}

		public override void Update()
		{
			if ( Sandbox.Player.Local is not Player player )
				return;

			if ( TargetPlayer == null || !TargetPlayer.IsValid() || player.Input.Pressed(InputButton.Attack1) )
			{
				var players = Game.Instance.GetTeamPlayers<IrisTeam>(true);

				if ( players != null && players.Count > 0 )
				{
					TargetPlayer = players[Rand.Int( players.Count - 1 )];
				}
			}

			_focusPoint = Vector3.Lerp( _focusPoint, GetSpectatePoint(), Time.Delta * 5.0f );

			Pos = _focusPoint + GetViewOffset();
			Rot = player.EyeRot;

			FieldOfView = FieldOfView.LerpTo( 50, Time.Delta * 3.0f );
			Viewer = null;
		}

		private Vector3 GetSpectatePoint()
		{
			if ( Sandbox.Player.Local is not Player )
				return DeathPosition;

			if ( TargetPlayer == null || !TargetPlayer.IsValid() || TimeSinceDied < 3 )
			{
				if ( DeathPosition != Vector3.Zero )
					return DeathPosition;
			}

			return TargetPlayer.EyePos;
		}

		private Vector3 GetViewOffset()
		{
			if ( Sandbox.Player.Local is not Player player )
				return Vector3.Zero;

			return player.EyeRot.Forward * -150 + Vector3.Up * 10;
		}
	}
}
