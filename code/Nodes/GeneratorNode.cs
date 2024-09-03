using Sandbox;

public sealed class GeneratorNode : Node
{
	[Property] public float Rate {get;set;} = 3;
	protected override void OnFixedUpdate()
	{
		if(Input.Down("attack1"))
			ManaOutput(null,Rate*Time.Delta);
	}
}
