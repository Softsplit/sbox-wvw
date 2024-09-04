using Sandbox;

public sealed class PlayerManager : Component
{
	[Property] public GameObject Camera {get;set;}
	[Property] public GameObject ThirdPersonCam {get;set;}
	[Property] public GameObject SpellCam {get;set;}
	[Property] public GameObject DefaultLookPos {get;set;}
	[Property] public GameObject LeftHand {get;set;}
	[Property] public GameObject RightHand {get;set;}
	[Property] public float TransitionSpeed {get;set;} = 10f;
	[Property] public bool InSpell {get;set;}
	bool Transitioning;
	public PlayerController playerController;
	WizardAnimator WizardAnimator;
	SpellMaker SpellMaker;
	SpellUI SpellUI;
	protected override void OnStart()
	{
		if(IsProxy)
		{
			Camera.Enabled = false;
			return;
		}
		playerController = Components.Get<PlayerController>(true);
		WizardAnimator = Components.GetInChildrenOrSelf<WizardAnimator>(true);
		SpellMaker = Components.GetInDescendants<SpellMaker>(true);
		SpellUI = Components.Get<SpellUI>(true);
	}
	protected override void OnPreRender()
	{
		if(IsProxy)
			return;
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
			var ray = Scene.Trace.Ray(Scene.Camera.Transform.Position,Scene.Camera.Transform.Position+Scene.Camera.Transform.World.Forward*1024).Run();
			if(ray.Hit)
			{
				WizardAnimator.LookPos = ray.HitPosition;
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
