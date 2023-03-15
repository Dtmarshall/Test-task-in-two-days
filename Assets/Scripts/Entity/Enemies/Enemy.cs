using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Enemy : Entity
{

    static readonly byte[] walkAnim = {2, 14, 17};
    public float moveSpeed;

    [Header("Fight")]
    Slime player;
    Transform playerTransform;

    [Header("Death")]
    public GameObject bone;
    public GameObject skull;

    public virtual void CustomStart(HealthPoints hpBar, GameLoop gameManager, Slime player, float difficulty)
    {
        damage = Mathf.RoundToInt(damage * Mathf.Pow(difficulty, 0.5f));
        maxHP = Mathf.RoundToInt(maxHP * difficulty);

        base.CustomStart(hpBar, gameManager);

        this.hpBar.Mob();

        this.player = player;
        playerTransform = player.transform;

        animator.SetFloat("Attack Speed", attackSpeed);
    }

    public void StartFight()
    {
        animator.SetInteger("animation", walkAnim[Random.Range(0, walkAnim.Length)]);
        StartCoroutine(Fight());
    }

    public void EndFight()
    {
        animator.SetInteger("animation", 2);
        thisTransform.DORotate(new Vector3(0, 0, 0), 0.5f);
    }

    IEnumerator Fight()
    {
        while (Mathf.Abs(thisTransform.position.x - playerTransform.position.x) > 0.8f)
        {
            thisTransform.position += Vector3.right * Time.deltaTime * moveSpeed;
            yield return null;
        }

        animator.SetInteger("animation", 6);
    }

    void Attack()
    {
        base.Attack(player);
    }

    public override bool TakeDamage(int count)
    {
        bool death = base.TakeDamage(count);
        // if (!death) animator.SetInteger("animation", 7);
        return death;
    }

    public override void Death()
    {
        base.Death();
        gameManager.EndFight();
        StartCoroutine(DieAnim());
    }

    IEnumerator DieAnim()
    {
        animator.SetInteger("animation", 9);

        yield return new WaitForSeconds(0.33f);

        for (int i = 0; i < 5; i++) Instantiate(bone, thisTransform.position, Quaternion.identity);
        Instantiate(skull, thisTransform.position, Quaternion.identity);
        gameManager.SpawnCoins(thisTransform.position);

        Destroy(hpBar.gameObject);
        Destroy(gameObject);
    }
}
