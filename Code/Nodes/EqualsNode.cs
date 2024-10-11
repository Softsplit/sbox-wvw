using Sandbox;

public sealed class EqualsNode : Node
{
	float a;
	float b;
	public override void NumberTick(int index, float number)
	{
		if(index == 0) a = number;
		else b = number;
	}

	protected override void OnFixedUpdate()
	{
		if(a == b)
			Output();
		a = -1;
		b = 0;
	}
}
