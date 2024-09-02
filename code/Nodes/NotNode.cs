using Sandbox;

public sealed class NotNode : Node
{
	bool input;

	public override void Tick( int index )
	{
		input = true;
	}

	protected override void OnFixedUpdate()
	{
		if(!input)
			Output();
		input = false;
	}
}
