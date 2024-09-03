using Sandbox;

public sealed class CameraMovement : Component
{
	[Property] public Vector2 Clamp {get;set;}
	protected override void OnUpdate()
	{
		Transform.LocalRotation *= Input.AnalogLook.WithRoll(0).WithYaw(0);
	}
}
