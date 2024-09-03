using Sandbox;

public sealed class BodyIkTest : Component
{
	[Property] public GameObject LookThing {get;set;}
	[Property] public float MoveX {get;set;}
	[Property] public float MoveY {get;set;}
	[Property] public bool Attacking {get;set;}
	[Property] public bool SettingSpell {get;set;}
	[Property] public List<GameObject> Lookers {get;set;}
	[Property] public Angles LookRotOffsetNorm {get;set;} = new Angles(0,0,90);
	[Property] public Angles LookRotOffsetAttack {get;set;} = new Angles(0,0,90);
	[Property] public float AttackToSpeed {get;set;} = 0.566f;
	SkinnedModelRenderer skinnedModelRenderer;
	protected override void OnStart()
	{
		skinnedModelRenderer = Components.Get<SkinnedModelRenderer>();
		LookOffset = LookRotOffsetNorm;
	}
	Angles LookOffset;
	protected override void OnUpdate()
	{
		skinnedModelRenderer.Set("MoveX",MoveX);
		skinnedModelRenderer.Set("MoveY",MoveY);
		skinnedModelRenderer.Set("Attacking", Attacking);
		skinnedModelRenderer.Set("SettingSpell", SettingSpell);

		LookOffset = Angles.Lerp(LookOffset, Attacking ? LookRotOffsetAttack : LookRotOffsetNorm, Time.Delta * (1/AttackToSpeed));

		foreach(GameObject g in Lookers)
		{
			Vector3 dir = LookThing.Transform.Position-g.Transform.Position;


			g.Transform.Rotation = Rotation.LookAt(dir);

			g.Transform.Rotation *= LookOffset;
		}
	}
}
