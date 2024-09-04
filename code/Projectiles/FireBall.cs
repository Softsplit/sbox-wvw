using Sandbox;

public sealed class FireBall : Projectile
{
	[Property] public Vector2 DamageRange {get;set;}
	[Property] public Vector2 SpeedRange {get;set;}
	[Property] public Curve Width {get;set;}
	[Property] public GameObject Visual {get;set;}
	[Property] public float Life {get;set;} = 5f;

	Rigidbody Rigidbody;
	float startTime;
	protected override void OnStart()
    {
		startTime = Time.Now;
		Rigidbody = Components.GetOrCreate<Rigidbody>();
        lastPos = Transform.Position;
    }
	bool hitSomething;
	Vector3 lastPos;
	protected override void OnFixedUpdate()
	{
		//poss.Add(Transform.Position);
		if(hitSomething) return;

		if(Time.Now-startTime >= Life)
		{
			GameObject.Destroy();
			return;
		}

		Rigidbody.Velocity = (Transform.World.Forward * MathX.Lerp(SpeedRange.x,SpeedRange.y,Strength));

		float w = Width.Evaluate((Time.Now-startTime)/Life);

		Visual.Transform.Scale = Vector3.One*w;
		
        var ray = Scene.Trace.Ray(lastPos,Transform.Position).Radius(w*50).UseHitboxes().IgnoreGameObjectHierarchy(Shooter).Run();

		if(ray.Hit)
		{
			
			hitSomething = true;
			
			//HealthComponent healthComponent = ray.GameObject.Components.Get<HealthComponent>();
			if(false)//healthComponent != null)
			{
				float damageMult = 1;
				if(ray.Hitbox != null)
				{
					IEnumerable<string> tags = ray.Hitbox.Tags.TryGetAll();
					
					foreach(string s in tags)
					{
						if(float.TryParse(s, out damageMult)) break;
					}
				}
				float damage = MathX.Lerp(DamageRange.x,DamageRange.y,Strength) * damageMult;
				
				//healthComponent.DoDamage(damage, owner);
			}
			if(ray.Surface.Sounds.ImpactHard != null) Sound.Play(ray.Surface.Sounds.ImpactHard,ray.HitPosition);
			GameObject.Destroy();
		}
        lastPos = Transform.Position;
	}
}
