using Sandbox;

public sealed class GeneratorNode : Node
{
	[Property] public float Rate {get;set;} = 3;
	bool generate;
	SpellMaker SpellMaker;

	protected override void OnStart()
	{
		base.OnStart();
		SpellMaker = Components.GetInAncestors<SpellMaker>(true);
	}
	public override void Tick( int index )
	{
		generate = false;
	}
	protected override void OnFixedUpdate()
	{
		if(SpellMaker.Enabled) return;
		if(generate)
			ManaOutput(null,(Rate*Time.Delta)/Outputs[0].Connections.Count);
		generate = true;
	}
}
