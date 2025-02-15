namespace CrazyEights.Player;

[Title( "Crazy Eights - First Person Camera" ), Category( "Crazy Eights - Player" )]
public sealed class FirstPersonCamera : Component
{
	[Property] public GameObject Target { get; set; }
	[Property, ToggleGroup( "ClampPitch" )] public bool ClampPitch { get; set; } = true;
	[Property, ToggleGroup( "ClampPitch" )] public Vector2 PitchRange { get; set; } = new Vector2( -90.0f, 90.0f );
	[Property, ToggleGroup( "ClampYaw" )] public bool ClampYaw { get; set; } = false;
	[Property, ToggleGroup( "ClampYaw" )] public Vector2 YawRange { get; set; } = new Vector2( -90.0f, 90.0f );

	private Angles _viewAngles;

	protected override void OnUpdate()
	{
		if ( IsProxy ) return;
		if ( Target is null ) return;

		_viewAngles += Input.AnalogLook;

		if ( ClampPitch )
			_viewAngles = _viewAngles.WithPitch( _viewAngles.pitch.Clamp( PitchRange.x, PitchRange.y ) );

		if ( ClampYaw )
			_viewAngles = _viewAngles.WithYaw( _viewAngles.yaw.Clamp( YawRange.x, YawRange.y ) );

		WorldPosition = Target.WorldPosition + Vector3.Up * 64f;
		WorldRotation = Target.WorldRotation * _viewAngles.ToRotation();
	}
}
