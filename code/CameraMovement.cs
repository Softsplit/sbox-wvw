using Sandbox;

public sealed class CameraMovement : Component
{
	[Property] public Vector2 Clamp {get;set;}
	PlayerController playerController;
	protected override void OnStart()
	{
		playerController = Components.GetInAncestors<PlayerController>();
	}
	protected override void OnUpdate()
	{
		Transform.LocalRotation = playerController.SmoothLookAngleAngles.WithRoll(0).WithYaw(0);
	}
}
