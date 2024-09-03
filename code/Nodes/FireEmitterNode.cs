using Sandbox;

public sealed class EmitterNode : Node
{
	WizardAnimator WizardAnimator;
	SpellMaker SpellMaker;

	protected override void OnStart()
	{
		WizardAnimator = Components.GetInAncestors<WizardAnimator>(true);
		SpellMaker = Components.GetInAncestors<SpellMaker>(true);
		MaxMana = 30;
	}
	protected override void OnFixedUpdate()
	{
		NumberOutput(Mana/MaxMana);
	}

	public override void Tick( int index )
	{
		if(Mana < 0.25f || SpellMaker.Enabled) return;
		WizardAnimator.Attack();
		Log.Info($"Fire {Mana}");
		Mana = 0;
	}
}
