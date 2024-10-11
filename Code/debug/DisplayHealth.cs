using Sandbox;
using System;

public sealed class DisplayHealth : Component
{
	HealthComponent healthComponent;
	GameObject target;
	[Property] public Vector3 DisplayPos {get;set;}
	[Property] public float InfoBuffer {get;set;} = 0.3f;
	[Property] public float DamageInfo {get;set;}
	[Property] public float Size {get;set;} = 60;
	[Property] public float DistanceReference {get;set;} = 130;


	protected override void OnStart()
	{
		healthComponent = Components.Get<HealthComponent>();
		target = Scene.Camera.GameObject;
		lastHP = healthComponent.MaxHealth;
	}
	float lastHP;

	float resetDamageITime;
	protected override void OnUpdate()
	{
		resetDamageITime += Time.Delta;
		if(lastHP != healthComponent.Health)
		{
			resetDamageITime = 0;
			DamageInfo -= lastHP - healthComponent.Health;
		}
		lastHP = healthComponent.Health;
		if(resetDamageITime > InfoBuffer)
		{
			resetDamageITime = 0;
			DamageInfo = 0;
		}
		if(!target.IsValid())
		{
			this.Destroy();
			return;
			
		}
		float disM = Vector3.DistanceBetween(Transform.World.PointToWorld(DisplayPos),target.Transform.Position)/DistanceReference;
		Gizmo.Draw.Color = Color.White;
		Gizmo.Draw.WorldText($"{MathF.Round(healthComponent.Health)}", new Transform(Transform.World.PointToWorld(DisplayPos), 
		Rotation.LookAt(Rotation.LookAt(target.Transform.Position - Transform.World.PointToWorld(DisplayPos)).Up)* new Angles(0,90,180), 0.1f
		),"Roboto", Size*disM);
		if(DamageInfo==0) return;
		Gizmo.Draw.Color = DamageInfo >= 0 ? Color.Green : Color.Red;
		Gizmo.Draw.WorldText($"{MathF.Round(MathF.Abs(DamageInfo))}", new Transform(Transform.World.PointToWorld(DisplayPos+(Vector3.Up*5f*disM)), 
		Rotation.LookAt(Rotation.LookAt(target.Transform.Position - Transform.World.PointToWorld(DisplayPos+(Vector3.Up*5f* disM))).Up)* new Angles(0,90,180), 0.1f
		),"Roboto", Size*0.5f*disM);
	}
}
