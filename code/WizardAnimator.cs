using Sandbox;

public sealed class WizardAnimator : Component
{
	[Sync, Property] public Vector3 LookPos {get;set;}
	[Sync, Property] public float MoveX {get;set;}
	[Sync, Property] public float MoveY {get;set;}
	[Sync, Property] public bool SettingSpell {get;set;}
	[Sync, Property] public bool Grounded {get;set;}
	[Property] public List<GameObject> Lookers {get;set;}
	[Property] public Angles LookRotOffsetNorm {get;set;} = new Angles(0,0,90);
	[Property] public Angles LookRotOffsetAttack {get;set;} = new Angles(0,0,90);
	[Property] public float AttackToSpeed {get;set;} = 0.566f;
	[Property] public float MoveDirSmoothing {get;set;} = 100f;
	public SkinnedModelRenderer skinnedModelRenderer;
	const float attackTime = 1.4f;

	float lastAttack;
	protected override void OnStart()
	{
		skinnedModelRenderer = Components.Get<SkinnedModelRenderer>();
		LookOffset = LookRotOffsetNorm;
	}
	Angles LookOffset;

	Vector2 MoveDirSmoothed;
	protected override void OnFixedUpdate()
	{
		MoveDirSmoothed = Vector2.Lerp(MoveDirSmoothed, new Vector2(MoveX, MoveY),Time.Delta*MoveDirSmoothing);
		skinnedModelRenderer.Set("MoveX",MoveDirSmoothed.x);
		skinnedModelRenderer.Set("MoveY",MoveDirSmoothed.y);
		skinnedModelRenderer.Set("SettingSpell", SettingSpell);
		skinnedModelRenderer.Set("Grounded", Grounded);

		LookOffset = Angles.Lerp(LookOffset, Time.Now-lastAttack < attackTime ? LookRotOffsetAttack : LookRotOffsetNorm, Time.Delta * (1/AttackToSpeed));

		foreach(GameObject g in Lookers)
		{
			Vector3 dir = LookPos-g.Transform.Position;


			g.Transform.Rotation = Rotation.LookAt(dir);

			g.Transform.Rotation *= LookOffset;
		}
	}

	[Broadcast]
	public void Attack()
	{
		Log.Info("poo poo");
		skinnedModelRenderer.Set("Attacking",true);
		lastAttack = Time.Now;
	}
}
