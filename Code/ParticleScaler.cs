using Sandbox;

public sealed class ParticleScaler : Component
{
	[Property] public float ScaleMod {get;set;} = 1f;

	ParticleSpriteRenderer particleSpriteRenderer;
	protected override void OnStart()
	{
		particleSpriteRenderer = Components.Get<ParticleSpriteRenderer>();
	}

	protected override void OnUpdate()
	{
		particleSpriteRenderer.Scale = Transform.Scale.Length*ScaleMod;
	}
}
