using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bird : Enemy
{

    public Renderer body;
    public Renderer face;

    public Material[] bodies;
    public Material[] faces;

    public override void CustomStart(HealthPoints hpBar, GameLoop gameManager, Slime player, float difficulty)
    {
        base.CustomStart(hpBar, gameManager, player, difficulty);
        
        body.material = bodies[Random.Range(0, bodies.Length)];
        face.material = faces[Random.Range(0, faces.Length)];
    }
}
