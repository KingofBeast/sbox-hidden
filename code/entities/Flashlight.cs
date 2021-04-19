﻿using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HiddenGamemode
{
	[ClassLibrary("flashlight")]
	public partial class Flashlight : SpotLightEntity
	{
		public Flashlight() : base()
		{
			Transmit = TransmitType.Always;
			InnerConeAngle = 10f;
			OuterConeAngle = 20f;
			Brightness = 0.7f;
			QuadraticAttenuation = 1f;
			LinearAttenuation = 0f;
			Color = new Color( 0.9f, 0.87f, 0.6f );
			Falloff = 4f;
			Enabled = true;
			DynamicShadows = true;
			Range = 512f;
		}

		public bool UpdateFromBattery( float battery )
		{
			Brightness = 0.01f + ((0.69f / 100f) * battery);
			Flicker = (battery <= 10 && battery >= 8);

			return (battery <= 0f);
		}
	}
}
