using System;
using Sandbox;

public sealed class Explosion : Component
{
	[Property] public float Radius {get;set;}
	[Property] public Vector2 Damage {get;set;}
	[Property] public Guid Shooter {get;set;}
	protected override void OnStart()
	{
		if(!Networking.IsHost)
			return;
		var healthComponents = new List<HealthComponent>();
        var origin = Transform.Position;
		float increment = Radius*0.08f;
        float step = 360f / increment;

		float sphereRadius = CalculateSphereRadius(Radius, step);

        for (float pitch = -90f; pitch <= 90f; pitch += step)
        {
            for (float yaw = 0f; yaw < 360f; yaw += step)
            {
                var direction = GetDirectionFromAngles(yaw, pitch);
                var endPoint = origin + direction * Radius;
                
                var trace = Scene.Trace.Ray(origin+direction*sphereRadius, endPoint).Radius(sphereRadius).Run();
				Gizmo.Draw.LineCapsule(new Capsule(origin,endPoint,sphereRadius));
                var hitObject = trace.GameObject;
                if (hitObject != null)
                {
					
                    var healthComponent = hitObject.Components.Get<HealthComponent>();
                    if (healthComponent != null && !healthComponents.Contains(healthComponent))
                    {
                        healthComponents.Add(healthComponent);
						healthComponent.DoDamage(MathX.Lerp(Damage.x,Damage.y,trace.Distance/Radius),Shooter);
                    }
                }
            }
        }
	}
    Vector3 GetDirectionFromAngles(float yaw, float pitch)
    {
        float yawRad = MathX.DegreeToRadian(yaw);
        float pitchRad = MathX.DegreeToRadian(pitch);

        float x = MathF.Cos(pitchRad) * MathF.Cos(yawRad);
        float y = MathF.Sin(pitchRad);
        float z = MathF.Cos(pitchRad) * MathF.Sin(yawRad);

        return new Vector3(x, y, z).Normal;
    }

	float CalculateSphereRadius(float explosionRadius, float angleStep)
    {
        float arcLength = 2 * MathF.PI * explosionRadius * (angleStep / 360f);
        return arcLength / 2f;
    }
}
