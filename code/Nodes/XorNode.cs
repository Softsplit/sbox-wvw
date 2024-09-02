public sealed class XorNode : Node
{
    bool a;
    bool b;
	public override void Tick(int index)
	{
        if(index == 0) a = true;
        else b = true;
	}

	protected override void OnFixedUpdate()
	{
		if(a ^ b)
            Output();
        a = false;
        b = false;
	}
}
