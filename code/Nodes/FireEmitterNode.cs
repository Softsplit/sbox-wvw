using System;
using Sandbox;

public sealed class EmitterNode : Node
{
	WizardAnimator WizardAnimator;
	PlayerManager PlayerManager;
	SpellMaker SpellMaker;

	[Property] public List<ProjectileInfo> Projectiles {get;set;}

	public class ProjectileInfo
	{
		[KeyProperty] public GameObject Projectile {get;set;}
		[KeyProperty] public float MaxLevel {get;set;}
	}

	protected override void OnStart()
	{
		WizardAnimator = Components.GetInAncestors<WizardAnimator>(true);
		PlayerManager = Components.GetInAncestors<PlayerManager>(true);
		SpellMaker = Components.GetInAncestors<SpellMaker>(true);
		MaxMana = 30;
	}
	protected override void OnFixedUpdate()
	{
		NumberOutput(Mana);
	}

	public override void Tick( int index )
	{
		if(Mana < 1.5f || SpellMaker.Enabled) return;
		WizardAnimator.Attack();
		Log.Info(MathF.Round(Mana*10f)/10f);
		ProjectileInfo lastProjectile = null;
		foreach(ProjectileInfo projectile in Projectiles)
		{
			if(MathF.Round(Mana*10f)/10f > projectile.MaxLevel) continue;
			Vector3 pos = Vector3.Lerp(PlayerManager.LeftHand.Transform.Position,PlayerManager.RightHand.Transform.Position,0.5f);
			Vector3 dir = (WizardAnimator.LookPos-pos).Normal;
			GameObject proj = projectile.Projectile.Clone();
			proj.Transform.Position = pos;
			proj.Transform.Rotation = Rotation.LookAt(dir);
			Projectile projectileComponent = proj.Components.Get<Projectile>();
			if(projectileComponent.IsValid())
			{
				projectileComponent.Strength = lastProjectile != null ?
					(Mana-lastProjectile.MaxLevel)/(projectile.MaxLevel-lastProjectile.MaxLevel)
					:
					Mana/projectile.MaxLevel;
			
				projectileComponent.Shooter = PlayerManager.GameObject;

				projectileComponent.InitialVelocity = PlayerManager.playerController.Velocity;
			}
			
			
			proj.Network.SetOwnerTransfer( OwnerTransfer.Takeover );
			proj.Network.DropOwnership();
			proj.NetworkSpawn();
			break;
		}

		Mana = 0;
	}

	
}
