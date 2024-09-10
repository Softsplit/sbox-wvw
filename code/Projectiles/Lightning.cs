using Sandbox;
using Softsplit;

public sealed class LightningProjectile : Projectile
{
	[Property] public VectorLineRenderer vectorLineRenderer {get;set;}
	[Property] public Vector2 Damage {get;set;}
	[Property] public Vector2 Distance {get;set;}
	[Property] public Vector2 Spread {get;set;}
	[Property] public bool ScaleDamage {get;set;}
	protected override void OnStart()
	{
		var spread = MathX.Lerp(Spread.x,Spread.y,Strength);
		TargetPos += Vector3.Random * (Game.Random.Next(0,100)/100f) * spread;
		var dir = (TargetPos - Transform.Position).Normal;
		var dis = MathX.Lerp(Distance.x,Distance.y,Strength);
		var ray = Scene.Trace.Ray(Transform.Position, Transform.Position+dir*dis).IgnoreGameObjectHierarchy(Shooter).UseHitboxes().Run();
		Vector3 endPoint;
		if(ray.Hit)
		{
			endPoint = ray.EndPosition;
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
				float damage = MathX.Lerp(Damage.x,Damage.y,Strength) * damageMult;
				
				healthComponent.DoDamage(damage, Network.OwnerId);
			}
		}
		else return;

		vectorLineRenderer.Points = new List<Vector3>
		{
			Transform.Position,
			endPoint
		};

		GameObject.DestroyAsync(0.1f);
	}
}
