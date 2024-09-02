using Sandbox;

public abstract class Node : Component
{
    [Property] public List<NodeOutput> Outputs {get;set;}
    public virtual void Output()
    {
        foreach(NodeOutput nodeOutput in Outputs)
        {
            nodeOutput.Send();
        }
    }
	public virtual void Tick()
    {

    }
}
