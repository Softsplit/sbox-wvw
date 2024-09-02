using System.Drawing;
using Sandbox;

public sealed class NodeOutput : Component
{
	[Property] public List<NodeInput> Connected {get;set;} = new List<NodeInput>();
	[Property] public VectorLineRenderer vectorLineRenderer {get;set;}
	[Property] public List<List<Vector3>> Detours {get;set;} = new List<List<Vector3>>();

	protected override void OnStart()
	{
		vectorLineRenderer = Components.GetOrCreate<VectorLineRenderer>();
        vectorLineRenderer.Width = 0.01f;
        vectorLineRenderer.Noise = 0;
        vectorLineRenderer.RunBySelf = false;
	}

    public void DrawLineTo(Vector3 pos, List<Vector3> Detours)
    {
        Vector3 startPoint = Transform.Position;

        foreach (var detour in Detours)
        {
            Gizmo.Draw.Line(startPoint, detour);
            startPoint = detour;
        }

        Gizmo.Draw.Line(startPoint, pos);
    }

	protected override void OnUpdate()
	{
		for(int i = 0; i < Connected.Count && i < Detours.Count; i++)
        {
            DrawLineTo(Connected[i].Transform.Position, Detours[i]);
        }
	}

	public void Send()
    {
        foreach(NodeInput nodeInput in Connected)
        {
            nodeInput.node.Tick();
        }
    }


}