using System;
using System.Diagnostics;
using Sandbox;
using Sandbox.Citizen;

[Title("Sauce Character Controller")]
[Category("Physics")]
[Icon("directions_walk")]
[EditorHandle("materials/gizmo/charactercontroller.png")]

public sealed class PlayerController : Component
{

    [Property, ToggleGroup("UseCustomGravity", Label = "Use Custom Gravity")] private bool UseCustomGravity {get;set;} = true;
    [Property, ToggleGroup("UseCustomGravity"), Description("Does not change scene gravity, this is only for the player."), Title("Gravity")] public Vector3 CustomGravity {get;set;} = new Vector3(0, 0, -800f);
    public Vector3 Gravity = new Vector3(0, 0, -800f);
    
    [Property, ToggleGroup("UseCustomFOV", Label = "Use Custom Field Of View")] private bool UseCustomFOV {get;set;} = true;
    [Property, ToggleGroup("UseCustomFOV"), Title("Field Of View"), Range(60f, 120f)] public float CustomFOV {get;set;} = 90f;

    // Movement Properties
    [Property, Group("Movement Properties"), Description("CS2 Default: 285.98f")] public float MaxSpeed {get;set;} = 285.98f;
    [Property, Group("Movement Properties"), Description("CS2 Default: 250f")] public float MoveSpeed {get;set;} = 250f;
    [Property, Group("Movement Properties"), Description("CS2 Default: 130f")] public float ShiftSpeed {get;set;} = 130f;
    [Property, Group("Movement Properties"), Description("CS2 Default: 85f")] public float CrouchSpeed {get;set;} = 85f;
    [Property, Group("Movement Properties"), Description("CS2 Default: 80f")] public float StopSpeed {get;set;} = 80f;
    [Property, Group("Movement Properties"), Description("CS2 Default: 5.2f")] public float Friction {get;set;} = 5.2f;
    [Property, Group("Movement Properties"), Description("CS2 Default: 5.5f")] public float Acceleration {get;set;} = 5.5f;
    [Property, Group("Movement Properties"), Description("CS2 Default: 12f")] public float AirAcceleration {get;set;} = 12f;
    [Property, Group("Movement Properties"), Description("CS2 Default: 30f")] public float MaxAirWishSpeed {get;set;} = 30f;
    [Property, Group("Movement Properties"), Description("CS2 Default: 301.993378f")] public float JumpForce {get;set;} = 301.993378f;
    [Property, Group("Movement Properties"), Description("CS2 Default: false")] private bool AutoBunnyhopping {get;set;} = false;
    
    // Stamina Properties
    [Property, Range(0f, 100f), Group("Stamina Properties"), Description("CS2 Default: 80f")] public float MaxStamina {get;set;} = 80f;
    [Property, Range(0f, 100f), Group("Stamina Properties"), Description("CS2 Default: 60f")] public float StaminaRecoveryRate {get;set;} = 60f;
    [Property, Range(0f, 1f), Group("Stamina Properties"), Description("CS2 Default: 0.08f")] public float StaminaJumpCost {get;set;} =  0.08f;
    [Property, Range(0f, 1f), Group("Stamina Properties"), Description("CS2 Default: 0.05f")] public float StaminaLandingCost {get;set;} =  0.05f;
    
    // Crouch Properties
    [Property, Group("Crouch Properties")] public bool ToggleCrouch {get;set;} = false;
    [Property, Range(0f, 1f), Group("Crouch Properties")] public float MinCrouchTime {get;set;} = 0.1f;
    [Property, Range(0f, 1f), Group("Crouch Properties")] public float MaxCrouchTime {get;set;} = 0.5f;
    [Property, Range(0f, 2f), Group("Crouch Properties")] public float CrouchRecoveryRate {get;set;} = 0.33f;
    [Property, Range(0f, 1f), Group("Crouch Properties")] public float CrouchCost {get;set;} = 0.1f;

    // Other Properties
    [Property, Title("Speed Multiplier"), Description("Useful for weapons that slow you down.")] public float Weight {get;set;} =  1f;
    [Property, Description("Add 'player' tag to disable collisions with other players.")] public TagSet IgnoreLayers { get; set; } = new TagSet();
    [Property] public GameObject Body {get;set;}
    [Property] public BoxCollider CollisionBox {get;set;}
    [Property] public WizardAnimator WizardAnimator {get;set;}

    // State Bools
    public bool IsWalking = false;
    [Sync] public bool IsOnGround {get;set;} = false;

    // Internal objects
	private ModelRenderer BodyRenderer;

    // Internal Variables
    public float Stamina = 80f;
    private float CrouchTime = 0.1f;
    private float jumpStartHeight = 0f;
    private float jumpHighestHeight = 0f;
    private bool AlreadyGrounded = true;
    public Vector2 SmoothLookAngle = Vector2.Zero; // => localLookAngle.LerpTo(LookAngle, Time.Delta / 0.1f);
    public Angles SmoothLookAngleAngles => new Angles(SmoothLookAngle.x, SmoothLookAngle.y, 0);
    private Angles LookAngleAngles => new Angles(LookAngle.x, LookAngle.y, 0);
    private float StaminaMultiplier => Stamina / MaxStamina;
    
    // Size
    [Property, Group("Size"), Description("CS2 Default: 16f")] private float Radius {get;set;} = 16f;
    [Property, Group("Size"), Description("CS2 Default: 72f")] private float StandingHeight {get;set;} = 72f;
    [Property, Group("Size"), Description("CS2 Default: 54f")] private float CroucingHeight {get;set;} = 54f;
    [Sync] private float Height {get;set;} = 72f;
    private BBox BoundingBox => new BBox(new Vector3(-Radius * GameObject.Transform.Scale.x, -Radius * GameObject.Transform.Scale.y, 0f), new Vector3(Radius * GameObject.Transform.Scale.x, Radius * GameObject.Transform.Scale.y, Height * GameObject.Transform.Scale.z));
    private int _stuckTries;

    // Synced internal varss
    [Sync] private float InternalMoveSpeed {get;set;} = 250f;
    [Sync] private Vector3 LastSize {get;set;} = Vector3.Zero;
    [Sync] public Vector3 WishDir {get;set;} = Vector3.Zero;
    [Sync] public Vector3 Velocity {get;set;} = Vector3.Zero;
	[Sync] public Vector2 LookAngle {get;set;} = Vector2.Zero;
    
    // Dynamic Camera Vars
    [Property, ToggleGroup("CameraRollEnabled", Label = "Camera Roll")] bool CameraRollEnabled {get;set;} = false;
    [Property, ToggleGroup("CameraRollEnabled")] float CameraRollDamping {get;set;} = 0.015f;
    [Property, ToggleGroup("CameraRollEnabled")] float CameraRollSmoothing {get;set;} = 0.2f;
    [Property, ToggleGroup("CameraRollEnabled")] float CameraRollAngleLimit {get;set;} = 30f;
    float sidetiltLerp = 0f;

    // Fucntions to make things slightly nicer

    public void Punch(in Vector3 amount) {
        ClearGround();
        Velocity += amount;
    }

    private void ClearGround() {
        IsOnGround = false;
    }

    // Character Controller Functions
    
    private void Move(bool step) {
        if (step && IsOnGround)
        {
            Velocity = Velocity.WithZ(0f);
        }

        if (Velocity.Length < 0.001f)
        {
            Velocity = Vector3.Zero;
            return;
        }

        Vector3 position = base.GameObject.Transform.Position;
        CharacterControllerHelper characterControllerHelper = new CharacterControllerHelper(BuildTrace(position, position), position, Velocity);
        characterControllerHelper.Bounce = 0;
        characterControllerHelper.MaxStandableAngle = 45.5f;
        if (step && IsOnGround)
        {
            characterControllerHelper.TryMoveWithStep(Time.Delta, 18f * GameObject.Transform.Scale.z);
        }
        else
        {
            characterControllerHelper.TryMove(Time.Delta);
        }

        base.Transform.Position = characterControllerHelper.Position;
        Velocity = characterControllerHelper.Velocity;
    }
    
    private void Move()
    {
        if (!TryUnstuck())
        {
            if (IsOnGround)
            {
                Move(step: true);
            }
            else
            {
                Move(step: false);
            }
        }
    }

    private bool TryUnstuck() {
        if (!BuildTrace(base.Transform.Position, base.Transform.Position).Run().StartedSolid)
        {
            _stuckTries = 0;
            return false;
        }

        int num = 20;
        for (int i = 0; i < num; i++)
        {
            Vector3 vector = base.Transform.Position + Vector3.Random.Normal * ((float)_stuckTries / 2f);
            if (i == 0)
            {
                vector = base.Transform.Position + Vector3.Up * 2f;
            }

            if (!BuildTrace(vector, vector).Run().StartedSolid)
            {
                base.Transform.Position = vector;
                return false;
            }
        }

        _stuckTries++;
        return true;
    }

    private void CategorizePosition() {
        Vector3 position = base.Transform.Position;
        Vector3 to = position + Vector3.Down * 2f;
        Vector3 from = position;
        bool isOnGround = IsOnGround;
        if (!IsOnGround && Velocity.z > 40f)
        {
            ClearGround();
            return;
        }
        
        to.z -= (isOnGround ? 18 : 0.1f);
        SceneTraceResult sceneTraceResult = BuildTrace(from, to).Run();
        if (!sceneTraceResult.Hit || Vector3.GetAngle(in Vector3.Up, in sceneTraceResult.Normal) > 45.5)
        {
            ClearGround();
            return;
        }

        IsOnGround = true;
        // GroundObject = sceneTraceResult.GameObject;
        // GroundCollider = sceneTraceResult.Shape?.Collider as Collider;
        if (isOnGround && !sceneTraceResult.StartedSolid && sceneTraceResult.Fraction > 0f && sceneTraceResult.Fraction < 1f)
        { // for some reason this fixes sliding down slopes when standing still, idek
            base.Transform.Position = sceneTraceResult.EndPosition + sceneTraceResult.Normal * 0f; 
        }
    }

    private SceneTrace BuildTrace(Vector3 from, Vector3 to) {
        return BuildTrace(base.Scene.Trace.Ray(in from, in to));
    }

    private SceneTrace BuildTrace(SceneTrace source) {
        BBox hull = BoundingBox;
        return source.Size(in hull).WithoutTags(IgnoreLayers).IgnoreGameObjectHierarchy(base.GameObject);
    }
    
    private void GatherInput() {
        WishDir = 0;

        var rot = LookAngleAngles.WithPitch(0).ToRotation();
        WishDir = (rot.Forward * Input.AnalogMove.x) + (rot.Left * Input.AnalogMove.y);
        if (!WishDir.IsNearZeroLength) WishDir = WishDir.Normal;

        IsWalking = !Input.Down("Run");

        if (Input.Pressed("Duck") || Input.Released("Duck")) CrouchTime += CrouchCost;
    }

    // Source engine magic functions

    private void ApplyFriction() {
        float speed, newspeed, control, drop;

        speed = Velocity.Length;

        // If too slow, return
        if (speed < 0.1f) return;
        
        drop = 0;

        // Apply ground friction
        if (IsOnGround)
        {
            // Bleed off some speed, but if we have less than the bleed
            // threshold, bleed the threshold amount.
            if (speed < StopSpeed) {
                control = StopSpeed;
            } else {
                control = speed;
            }
            drop += control * Friction * Time.Delta; // Add the amount to the drop amount.
        }

        // Scale the velocity
        newspeed = speed - drop;
        if (newspeed < 0) newspeed = 0;

        if (newspeed != speed)
        {
            newspeed /= speed; // Determine proportion of old speed we are using.
            Velocity *= newspeed; // Adjust velocity according to proportion.
        }
    }

    private void Accelerate(Vector3 wishDir, float wishSpeed, float accel) {
        float addspeed, accelspeed, currentspeed;
        
        currentspeed = Velocity.Dot(wishDir);
        addspeed = wishSpeed - currentspeed;
    
        if (addspeed <= 0) return;
        
        accelspeed = accel * wishSpeed * Time.Delta;
        
        if (accelspeed > addspeed) accelspeed = addspeed;
        
        Velocity += wishDir * accelspeed;
    }

    private void AirAccelerate(Vector3 wishDir, float wishSpeed, float accel) {
        float addspeed, accelspeed, currentspeed;
        
        float wishspd = wishSpeed;

        if (wishspd > MaxAirWishSpeed) wishspd = MaxAirWishSpeed;

        currentspeed = Velocity.Dot(wishDir);
        addspeed = wishspd - currentspeed;
    
        if (addspeed <= 0) return;
        
        accelspeed = accel * wishSpeed * Time.Delta;
        
        if (accelspeed > addspeed) accelspeed = addspeed;
        
        Velocity += wishDir * accelspeed;
    }

    private void GroundMove() {
        if (AlreadyGrounded == IsOnGround) {
            Accelerate(WishDir, WishDir.Length * InternalMoveSpeed, Acceleration);
        }
        if (Velocity.WithZ(0).Length > MaxSpeed) {
            var FixedVel = Velocity.WithZ(0).Normal * MaxSpeed;
            Velocity = Velocity.WithX(FixedVel.x).WithY(FixedVel.y);
        }
        if (Velocity.z < 0) Velocity = Velocity.WithZ(0);

        if ((AutoBunnyhopping && Input.Down("Jump")) || Input.Pressed("Jump")) {
            jumpStartHeight = GameObject.Transform.Position.z;
            jumpHighestHeight = GameObject.Transform.Position.z;
            Punch(new Vector3(0, 0, JumpForce * StaminaMultiplier));
            Stamina -= Stamina * StaminaJumpCost * 2.9625f;
            Stamina = (Stamina * 10).FloorToInt() * 0.1f;
            if (Stamina < 0) Stamina = 0;
        }
    }

    private void AirMove() {
        AirAccelerate(WishDir, InternalMoveSpeed * Weight, AirAcceleration);
    }
    
	// Overrides
    
    protected override void DrawGizmos() {
        BBox box = new BBox(new Vector3(-Radius, -Radius, 0f), new Vector3(Radius, Radius, Height));
        box.Rotate(GameObject.Transform.LocalRotation.Inverse);
        Gizmo.Draw.LineBBox(in box);
    }
    
	protected override void OnAwake() {
        Scene.FixedUpdateFrequency = 64;

        BodyRenderer = Components.GetInChildrenOrSelf<ModelRenderer>();
        
        Height = StandingHeight;
    }

    protected override void OnFixedUpdate() {
        if (CollisionBox == null) return;
        
        if (CollisionBox.Scale != LastSize) {
            CollisionBox.Scale = LastSize;
            CollisionBox.Center = new Vector3(0, 0, LastSize.z * 0.5f);
        }
        
		if ( IsProxy )
			return;
        
        if (UseCustomGravity) {
            Gravity = CustomGravity;
        } else {
            Gravity = Scene.PhysicsWorld.Gravity;
        }

        GatherInput();

        // Crouching
        var InitHeight = Height;
        
        InternalMoveSpeed = MoveSpeed;
        if (IsWalking) InternalMoveSpeed = ShiftSpeed;
        InternalMoveSpeed *= StaminaMultiplier * Weight;

        Height = Height.LerpTo(Height, Time.Delta / CrouchTime.Clamp(MinCrouchTime, MaxCrouchTime));
        
        LastSize = new Vector3(Radius * 2, Radius * 2, Height);
        
        Velocity += Gravity * Time.Delta * 0.5f;
        
        if (AlreadyGrounded != IsOnGround) {
            if (IsOnGround) {
                var heightMult = Math.Abs(jumpHighestHeight - GameObject.Transform.Position.z) / 46f;
                Stamina -= Stamina * StaminaLandingCost * 2.9625f * heightMult.Clamp(0, 1f);
                Stamina = (Stamina * 10).FloorToInt() * 0.1f;
                if (Stamina < 0) Stamina = 0;
            } else {
                jumpStartHeight = GameObject.Transform.Position.z;
                jumpHighestHeight = GameObject.Transform.Position.z;
            }
        } else {
            if(IsOnGround) ApplyFriction();
        }
        
        if(IsOnGround) {
            GroundMove();
        } else {
            AirMove();
        }
        
        AlreadyGrounded = IsOnGround;
        
        CrouchTime -= Time.Delta * CrouchRecoveryRate;
        CrouchTime = CrouchTime.Clamp(0f, MaxCrouchTime);
        
        Stamina += StaminaRecoveryRate * Time.Delta;
        if (Stamina > MaxStamina) Stamina = MaxStamina;
        Velocity *= GameObject.Transform.Scale;
        Move();
        CategorizePosition();
        Velocity /= GameObject.Transform.Scale;
        
        Velocity += Gravity * Time.Delta * 0.5f;
        
        // Terminal velocity
        if (Velocity.Length > 3500) Velocity = Velocity.Normal * 3500;

        if (jumpHighestHeight < GameObject.Transform.Position.z) jumpHighestHeight = GameObject.Transform.Position.z;
    }

    [Broadcast]
    public void SetAnims()
    {
        WizardAnimator.Grounded = IsOnGround;
        Vector3 LocalVel = new Transform(Vector3.Zero,Transform.Rotation).PointToLocal(Velocity);
        WizardAnimator.MoveX = LocalVel.x;
        WizardAnimator.MoveY = LocalVel.y;
    }
    
	protected override void OnUpdate() {
        SetAnims();
        if (Body == null || BodyRenderer == null) return;
        
        SmoothLookAngle = SmoothLookAngle.LerpTo(LookAngle, Time.Delta / 0.035f);

		Body.Transform.Rotation = SmoothLookAngleAngles.WithPitch(0).ToRotation();
        
		if ( IsProxy )
			return;
        
        
        // var ControllerInput = Input.GetAnalog(InputAnalog.Look);
        // if (ControllerInput.Length > 1) ControllerInput = ControllerInput.Normal;
        // ControllerInput *= 25;
        // LookAngle += new Vector2((Input.MouseDelta.y - ControllerInput.y), -(Input.MouseDelta.x + ControllerInput.x)) * Preferences.Sensitivity * 0.022f;
        LookAngle += new Vector2((Input.MouseDelta.y), -(Input.MouseDelta.x)) * Preferences.Sensitivity * 0.022f;
        LookAngle = LookAngle.WithX(LookAngle.x.Clamp(-89f, 89f));
		
        var angles = LookAngleAngles;
	}

}