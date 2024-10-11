public sealed class FollowObject : Component
{
	[Property] public GameObject ObjectPos;
	[Property] public GameObject ObjectRot;
	[Property] public bool Pos;
	[Property] public bool Rot;
	protected override void OnPreRender()
	{
		if(Pos)
			Transform.Position = ObjectPos.Transform.Position;
		if(Rot)
			Transform.Rotation = ObjectRot.Transform.Rotation;
	}
}
