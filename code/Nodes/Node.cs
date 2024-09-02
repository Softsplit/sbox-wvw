public abstract class Node : Component
{
    [Property] public List<NodeOutput> Outputs {get;set;}
    public virtual void Output()
    {
        foreach(NodeOutput nodeOutput in Outputs)
        {
            nodeOutput.SendSignal();
        }
    }
	public virtual void Tick(int index)
    {

    }
}
