using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    protected GameLoop gameManager;

    [Header("Attack")]
    public int damage;
    public float attackSpeed;

    [Header("HP")]
    public int maxHP;
    protected int hp;
    public Transform head;
    public float hpBarHeight;
    protected HealthPoints hpBar;
    Transform hpBarTransform;
    [HideInInspector]
    public bool die;

    protected Transform thisTransform;
    protected Animator animator;
    //Cache

    public virtual void CustomStart(HealthPoints hpBar, GameLoop gameManager)
    {
        thisTransform = transform;
        animator = GetComponent<Animator>();
        this.gameManager = gameManager;
        //Cache

        hp = maxHP;
        this.hpBar = Instantiate(hpBar, head.position + Vector3.up * hpBarHeight, Quaternion.identity);
        hpBarTransform = this.hpBar.transform;
        this.hpBar.SetHP(hp, maxHP);
        //HP
    }

    protected virtual void Update()
    {
        hpBarTransform.position = head.position + Vector3.up * hpBarHeight;
    }

    public virtual bool TakeDamage(int count)
    {
        hp -= count;
        hpBar.SetHP(maxHP, hp);
        hpBar.StartCoroutine(hpBar.Damage(count, false));
        if (hp > 0)
        {
            TakeDamage();
            return false;
        }

        Death();
        return true;
    }

    protected virtual void TakeDamage() { }

    public virtual void Death()
    {
        die = true;
    }

    protected virtual bool Attack(Entity target)
    {
        return target.TakeDamage(damage);
    }
}
