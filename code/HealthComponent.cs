using System;
using Sandbox.Diagnostics;
public partial class HealthComponent : Component
{
	[Property,Sync] public float Health {get;set;}
    [Property] public float MaxHealth {get;set;}
    [Property,Sync] public Guid LastAttacker {get;set;}

    [Broadcast]
    public void DoDamage(float damage, Guid from)
    {
        Health -= damage;
    }
}

