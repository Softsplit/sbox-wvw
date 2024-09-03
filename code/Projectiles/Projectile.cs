using System;

public abstract class Projectile : Component
{
    [Property] public float Strength {get;set;}
    [Property] public Vector3 InitialVelocity {get;set;}
    [Property] public GameObject Shooter {get;set;}
}
