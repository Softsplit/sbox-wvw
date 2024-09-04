using System;

public abstract class Node : Component
{
    [Property] public List<NodeOutput> Outputs {get;set;}
    [Property] public float Mana {get;set;}
    [Property] public float MaxMana {get;set;}
    [Property] public float Cost {get;set;} = 10f;
    public void AddMana(Node Sender, float mana)
    {
        
        if (Sender != null)
        {
            float adjustedMana = MathF.Max(0f, MathF.Min(mana, Sender.Mana));

            Sender.Mana -= adjustedMana;

            Mana += adjustedMana;
        }
        else
        {
            Mana += mana;
        }
        
        Mana = MathF.Min(Mana, MaxMana);
        
    }
    public virtual void Output()
    {
        foreach(NodeOutput nodeOutput in Outputs)
        {
            if(nodeOutput.outputType != NodeOutput.OutputType.Normal) break;
            nodeOutput.SendSignal();
        }
    }
    public virtual void NumberOutput(float number)
    {
        foreach(NodeOutput nodeOutput in Outputs)
        {
            if(nodeOutput.outputType != NodeOutput.OutputType.Number) break;
            nodeOutput.SendNumberSignal(number);
        }
    }

    public virtual void ManaOutput(Node node, float number)
    {
        foreach(NodeOutput nodeOutput in Outputs)
        {
            if(nodeOutput.outputType != NodeOutput.OutputType.Mana) break;
            nodeOutput.SendMana(node,number);
        }
    }
	public virtual void Tick(int index)
    {

    }

    public virtual void NumberTick(int index, float number)
    {

    }
}
