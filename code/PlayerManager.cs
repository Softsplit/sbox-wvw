using System;
using System.Threading.Channels;
using Sandbox;
using Softsplit;

public sealed class PlayerManager : Component
{
	[Property] public GameObject Camera {get;set;}
	[Property] public GameObject ThirdPersonCam {get;set;}
	[Property] public GameObject SpellCam {get;set;}
	[Property] public GameObject DefaultLookPos {get;set;}
	[Property] public GameObject LeftHand {get;set;}
	[Property] public GameObject RightHand {get;set;}
	ModelPhysics modelPhysics;
	[Property] public float TransitionSpeed {get;set;} = 10f;
	[Property] public bool InSpell {get;set;}
	[Property] public bool Transitioning {get;set;}
	[Property] public float MinLookDis {get;set;}
	public PlayerController playerController;
	WizardAnimator WizardAnimator;
	SpellMaker SpellMaker;
	SpellUI SpellUI;
	ModdedNetworkHelper NetworkHelper;
	HealthComponent HealthComponent;

	[Button("Balls")] public void KillButton() => Kill(Vector3.Zero);

	[Broadcast]
	public async void Kill(Vector3 vel)
	{
		WizardAnimator.UnProcedualLookers();
		WizardAnimator.Enabled = false;
		modelPhysics.Enabled = true;
		foreach(PhysicsBody physicsBody in modelPhysics.PhysicsGroup.Bodies)
        {
            physicsBody.ApplyForce(vel);
        }
		modelPhysics.GameObject.SetParent(null);
		modelPhysics.Renderer.UseAnimGraph = false;
		GameObject.Destroy();
		modelPhysics.GameObject.DestroyAsync(10);
		if ( !Networking.IsHost ) return;
		NetworkHelper.AddRespawn(Network.OwnerId);
		
	}

	protected override void OnStart()
	{
		HealthComponent = Components.Get<HealthComponent>();
		NetworkHelper = Scene.Components.GetInChildren<ModdedNetworkHelper>();
		Camera = Scene.Camera.GameObject;
		playerController = Components.Get<PlayerController>(true);
		WizardAnimator = Components.GetInChildrenOrSelf<WizardAnimator>(true);
		modelPhysics = WizardAnimator.Components.Get<ModelPhysics>(true);
		SpellMaker = Components.GetInDescendants<SpellMaker>(true);
		SpellUI = Components.Get<SpellUI>(true);
		if(Time.Now > 5) Transitioning = true;
	}
	protected override void OnPreRender()
	{
		if(IsProxy)
			return;

		if(HealthComponent.Health <= 0)
		{
			Kill(playerController.Velocity);
			return;
		}
		Vector3 TargetPos = Vector3.Zero;
		Rotation TargetRot = Rotation.Identity;
		playerController.Enabled = !InSpell;
		SpellMaker.Enabled = InSpell && !Transitioning;
		SpellUI.Enabled = SpellMaker.Enabled;
		WizardAnimator.SettingSpell = InSpell;
		Mouse.Visible = InSpell;
		if(Input.Pressed("Score") && playerController.IsOnGround)
		{
			InSpell = !InSpell;
		}

		if(InSpell)
		{
			TargetPos = SpellCam.Transform.Position;
			TargetRot = SpellCam.Transform.Rotation;
			WizardAnimator.MoveX = 0;
			WizardAnimator.MoveY = 0;
			if(!lastInSpell)
				Transitioning = true;
		}
		else
		{
			TargetPos = ThirdPersonCam.Transform.Position;
			TargetRot = ThirdPersonCam.Transform.Rotation;
			if(lastInSpell)
				Transitioning = true;
			Ray rawRay = new Ray(Scene.Camera.Transform.Position,Scene.Camera.Transform.World.Forward);
			var ray = Scene.Trace.Ray(Scene.Camera.Transform.Position,Scene.Camera.Transform.Position+Scene.Camera.Transform.World.Forward*1024).IgnoreGameObjectHierarchy(GameObject).Run();
			if(ray.Hit)
			{
				WizardAnimator.LookPos = ray.Distance > MinLookDis ? ray.HitPosition : rawRay.Project(MinLookDis);
				DefaultLookPos.Transform.Position = ray.HitPosition;
			}
			else WizardAnimator.LookPos = DefaultLookPos.Transform.Position;

			Gizmo.Draw.SolidSphere(WizardAnimator.LookPos,1);
		}

		if(!Transitioning)
		{
			Camera.Transform.Position = TargetPos;
			Camera.Transform.Rotation = TargetRot;
		}
		else
		{
			Camera.Transform.Position = Vector3.Lerp(Camera.Transform.Position,TargetPos,Time.Delta * TransitionSpeed);
			Camera.Transform.Rotation = Rotation.Lerp(Camera.Transform.Rotation,TargetRot,Time.Delta * TransitionSpeed);
			if(Vector3.DistanceBetween(Camera.Transform.Position,TargetPos) < 1f)
				Transitioning = false;
		}
		
		lastInSpell = InSpell;

		
	}
	
	bool lastInSpell;
}
