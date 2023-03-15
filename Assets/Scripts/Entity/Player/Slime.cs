using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Slime : Entity
{
    [Header("Slime")]
    public Renderer thisRenderer;

    [Header("Move")]
    public float maxJumpOffset;
    float jumpOffset;
    int jumpCount;

    [Header("Fight")]
    Enemy targetEnemy;
    public GameObject bullet;
    public float bulletSpeed;
    public AudioSource bulletDestroyParticles;
    float shootPitch = 1;
    bool fight;
    float reloadT = 0.5f;

    [Header("Faces")]
    public Texture2D[] faces;
    Material faceMaterial;

    public override void CustomStart(HealthPoints hpBar, GameLoop gameManager)
    {
        faceMaterial = thisRenderer.materials[1];
        //Cache

        base.CustomStart(hpBar, gameManager);
    }

    public void Move(float dist)
    {
        jumpCount = Mathf.Max(Mathf.Abs(Mathf.RoundToInt(dist / maxJumpOffset)), 1);
        jumpOffset = dist / jumpCount;
        EndJump();
    }

    void JumpOffset()
    {
        thisTransform.DOMoveX(thisTransform.position.x + jumpOffset, 0.35f).SetEase(Ease.InOutSine);
    }

    void EndJump()
    {
        jumpCount--;
        if (jumpCount > -1) animator.SetTrigger("Jump");
        else gameManager.EndMove();
    }

    public void StartFight(Enemy enemies)
    {
        targetEnemy = enemies;
        fight = true;
    }

    public void EndFight()
    {
        fight = false;
        reloadT = 0.5f;
        shootPitch = 1;
    }

    public void AddHP(int count)
    {
        maxHP += count;
        hp += count;
        hpBar.SetHP(maxHP, hp);
    }

    public void RecoverHP()
    {
        hp = maxHP;
        hpBar.SetHP(maxHP, hp);
    }

    protected override void TakeDamage()
    {
        base.TakeDamage();
        animator.SetTrigger("Damage");
    }

    public override void Death()
    {
        base.Death();
        animator.SetTrigger("Death");
        gameManager.Death();
    }

    protected override void Update()
    {
        base.Update();

        if (!fight) return;
        reloadT += Time.deltaTime * attackSpeed;
        if (reloadT >= 1)
        {
            reloadT = 0;
            StartCoroutine(Shoot());
        }
    }

    IEnumerator Shoot()
    {
        GameObject bullet = Instantiate(this.bullet, thisTransform.position, Quaternion.identity);
        Transform bulletTransform = bullet.transform;
        Transform targetEnemyTransform = targetEnemy.head;
        Vector3 startPos = bulletTransform.position;
        Vector3 endPoint = targetEnemyTransform.position + (thisTransform.position - targetEnemyTransform.position).normalized * 0.33f;

        float dist = Vector3.Distance(startPos, endPoint) * 0.5f;
        float t = 0;
        while (t < bulletSpeed * dist && !targetEnemy.die)
        {
            t += Time.deltaTime;
            float tt = t / (bulletSpeed * dist);
            endPoint = targetEnemyTransform.position + (thisTransform.position - targetEnemyTransform.position).normalized * 0.33f;
            Vector3 wayPoint = Vector3.Lerp(startPos, endPoint, 0.5f) + Vector3.up * dist;
            bulletTransform.position = Mathf.Pow(1 - tt, 2) * startPos + 2 * (1 - tt) * tt * wayPoint + Mathf.Pow(tt, 2) * endPoint;
            yield return null;
        }

        Instantiate(bulletDestroyParticles, bulletTransform.position, Quaternion.identity).pitch = shootPitch;
        shootPitch += 0.033f;
        Destroy(bullet);
        if (!targetEnemy.die) Attack(targetEnemy);
    }

    void SetFace(int n)
    {
        faceMaterial.SetTexture("_MainTex", faces[n]);
    }
}
