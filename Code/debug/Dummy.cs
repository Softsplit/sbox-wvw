using Sandbox;

public sealed class Dummy : Component
{
	bool ResetingHealth;
	public async void ResetHealth()
	{
		ResetingHealth = true;
		await Task.DelaySeconds(1);
		healthComponent.Health = healthComponent.MaxHealth;
		ResetingHealth = false;
	}

	HealthComponent healthComponent;
	protected override void OnStart()
	{
		healthComponent = Components.Get<HealthComponent>();
	}
	protected override void OnUpdate()
	{
		if(healthComponent.Health <= 0&&!ResetingHealth)
			ResetHealth();
	}
}
