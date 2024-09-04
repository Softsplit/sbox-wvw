using Sandbox;

public sealed class GeneratorNode : Node
{
	[Property] public float Rate {get;set;} = 3;
	bool generate;
	public override void Tick( int index )
	{
		generate = false;
	}
	protected override void OnFixedUpdate()
	{
		if(Input.Down("attack1") && generate)
			ManaOutput(null,Rate*Time.Delta);
		generate = true;
	}
}
