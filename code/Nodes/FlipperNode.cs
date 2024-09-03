using Sandbox;

public sealed class FlipperNode : Node
{
	[Property] public ModelRenderer Visual {get;set;}
	[Property] public Color OffColour {get;set;}
	[Property] public Color OnColour {get;set;}
	[Property] public bool On {get;set;}
	bool stateChanged;
	bool ticked;

	public override void Tick(int index)
	{
		if (!stateChanged)
		{
			On = !On;
			stateChanged = true;
		}
		ticked = true;
		Visual.Tint = On ? OnColour : OffColour;
	}

	protected override void OnFixedUpdate()
	{
		if (On)
			Output();
		
		if(!ticked)
		{
			stateChanged = false;
		}

		ticked = false;
	}
}
