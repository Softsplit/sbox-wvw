using Sandbox;

public sealed class FlipperNode : Node
{
	[Property] public ModelRenderer Visual {get;set;}
	[Property] public Color OffColour {get;set;}
	[Property] public Color OnColour {get;set;}
	[Property] public bool On {get;set;}

	public override void Tick(int index)
	{
		On = !On;
		Visual.Tint = On ? OnColour : OffColour;
	}

	protected override void OnFixedUpdate()
	{
		if(On)
			Output();
	}
}
