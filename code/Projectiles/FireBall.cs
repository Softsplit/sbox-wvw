using System;
using Sandbox;

public sealed class FireBall : Projectile
{
	[Property] public Curve DamageCurve {get;set;}
	[Property] public Curve SpeedCurve {get;set;}
	[Property] public Curve Width {get;set;}
	[Property] public GameObject Visual {get;set;}
	[Property] public float Life {get;set;} = 5f;
	[Property] public bool ScaleDamage {get;set;} = true;
	[Property] public bool Explode {get;set;}
	[Property] public Vector2 ExplosionRadius {get;set;} = new Vector2(150,300);
	[Property] public Vector2 MaxExplosionDamage {get;set;} = new Vector2(70,100);
	[Property] public Vector2 MinExplosionDamage {get;set;} = new Vector2(20,25);

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

		Transform.Position += Transform.World.Forward * SpeedCurve.Evaluate(Strength) * Time.Delta;

		float w = Width.Evaluate((Time.Now-startTime)/Life);

		Visual.Transform.Scale = Vector3.One*w;
		
        var ray = Scene.Trace.Ray(lastPos,Transform.Position).Radius(w*50).UseHitboxes().IgnoreGameObjectHierarchy(Shooter).Run();

		if(ray.Hit)
		{
			if(Explode)
			{
				GameObject gameObject = new GameObject();
				gameObject.Transform.Position = ray.HitPosition;
				Explosion explosion = gameObject.Components.Create<Explosion>();
				explosion.Shooter = Network.OwnerId;
				explosion.Radius = MathX.Lerp(ExplosionRadius.x,ExplosionRadius.y,Strength);
				explosion.Damage = new Vector2 (
					MathX.Lerp(MinExplosionDamage.x,MinExplosionDamage.y,Strength),
					MathX.Lerp(MaxExplosionDamage.x,MaxExplosionDamage.y,Strength)
				);

				gameObject.NetworkSpawn();
			}
			else
			{
				hitSomething = true;
				HealthComponent healthComponent = ray.GameObject.Components.Get<HealthComponent>();
				if(healthComponent != null)
				{
					float damageMult = 1;
					if(ray.Hitbox != null && ScaleDamage)
					{
						IEnumerable<string> tags = ray.Hitbox.Tags.TryGetAll();
						
						foreach(string s in tags)
						{
							if(float.TryParse(s, out damageMult)) break;
						}
					}
					float damage = DamageCurve.Evaluate(Strength) * damageMult;
					
					healthComponent.DoDamage(damage, Network.OwnerId);
				}
			}
			if(ray.Surface.Sounds.ImpactHard != null) Sound.Play(ray.Surface.Sounds.ImpactHard,ray.HitPosition);
			GameObject.Destroy();
		}
        lastPos = Transform.Position;
	}
}
