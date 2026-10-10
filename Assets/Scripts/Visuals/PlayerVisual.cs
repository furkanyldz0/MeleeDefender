using DG.Tweening;
using System;
using UnityEngine;

// Oyuncunun küp görselini zırhlı bir şövalye ile değiştirir ve hareketine göre canlandırır.
public class PlayerVisual : MonoBehaviour {

    private static readonly Color Steel = new Color(0.66f, 0.71f, 0.78f);
    private static readonly Color DarkSteel = new Color(0.35f, 0.39f, 0.48f);
    private static readonly Color Tabard = new Color(0.18f, 0.36f, 0.92f);
    private static readonly Color Cape = new Color(0.78f, 0.19f, 0.24f);
    private static readonly Color Gold = new Color(0.95f, 0.76f, 0.31f);
    private static readonly Color Leather = new Color(0.42f, 0.27f, 0.16f);
    private static readonly Color VisorGlow = new Color(0.44f, 0.89f, 1f);

    private Transform body;
    private Transform capePivot;
    private Transform leftLeg;
    private Transform rightLeg;

    private Vector3 lastPosition;
    private Vector3 smoothedVelocity;
    private float walkCycle;

    private void Start() {
        VisualFactory.HideRenderer(transform.Find("Mesh"));
        Build();

        lastPosition = transform.position;

        Player.Instance.OnAttack += Player_OnAttack;
        Player.Instance.OnDash += Player_OnDash;
        Player.Instance.OnSkillUsed += Player_OnSkillUsed;
    }

    private void Build() {
        Transform root = VisualFactory.CreateRoot(transform, "KnightVisual");
        body = VisualFactory.CreateRoot(root, "Body");

        Material steel = VisualFactory.Solid(Steel);
        Material darkSteel = VisualFactory.Solid(DarkSteel);
        Material tabard = VisualFactory.Solid(Tabard);
        Material cape = VisualFactory.Solid(Cape);
        Material gold = VisualFactory.Solid(Gold);
        Material leather = VisualFactory.Solid(Leather);

        // Bacaklar (yürürken sallanır)
        leftLeg = VisualFactory.CreateRoot(root, "LeftLeg");
        leftLeg.localPosition = new Vector3(-0.1f, 0.3f, 0f);
        VisualFactory.Part(PrimitiveType.Cube, leftLeg, new Vector3(0f, -0.15f, 0f), new Vector3(0.14f, 0.3f, 0.17f), darkSteel);
        VisualFactory.Part(PrimitiveType.Cube, leftLeg, new Vector3(0f, -0.27f, 0.03f), new Vector3(0.15f, 0.07f, 0.22f), leather);

        rightLeg = VisualFactory.CreateRoot(root, "RightLeg");
        rightLeg.localPosition = new Vector3(0.1f, 0.3f, 0f);
        VisualFactory.Part(PrimitiveType.Cube, rightLeg, new Vector3(0f, -0.15f, 0f), new Vector3(0.14f, 0.3f, 0.17f), darkSteel);
        VisualFactory.Part(PrimitiveType.Cube, rightLeg, new Vector3(0f, -0.27f, 0.03f), new Vector3(0.15f, 0.07f, 0.22f), leather);

        // Gövde
        VisualFactory.Part(PrimitiveType.Capsule, body, new Vector3(0f, 0.52f, 0f), new Vector3(0.44f, 0.24f, 0.32f), tabard);
        VisualFactory.Part(PrimitiveType.Cube, body, new Vector3(0f, 0.6f, 0f), new Vector3(0.4f, 0.18f, 0.3f), steel);
        VisualFactory.Part(PrimitiveType.Cube, body, new Vector3(0f, 0.34f, 0f), new Vector3(0.46f, 0.06f, 0.33f), leather);
        VisualFactory.Part(PrimitiveType.Cube, body, new Vector3(0f, 0.34f, 0.17f), new Vector3(0.08f, 0.07f, 0.02f), gold);
        VisualFactory.Part(PrimitiveType.Cube, body, new Vector3(0f, 0.5f, 0.165f), new Vector3(0.1f, 0.1f, 0.02f), gold, new Vector3(0f, 0f, 45f));

        // Omuzluklar
        VisualFactory.Part(PrimitiveType.Sphere, body, new Vector3(-0.26f, 0.68f, 0f), new Vector3(0.2f, 0.16f, 0.22f), steel);
        VisualFactory.Part(PrimitiveType.Sphere, body, new Vector3(0.26f, 0.68f, 0f), new Vector3(0.2f, 0.16f, 0.22f), steel);
        VisualFactory.Part(PrimitiveType.Cube, body, new Vector3(-0.26f, 0.62f, 0f), new Vector3(0.18f, 0.03f, 0.2f), gold);
        VisualFactory.Part(PrimitiveType.Cube, body, new Vector3(0.26f, 0.62f, 0f), new Vector3(0.18f, 0.03f, 0.2f), gold);

        // Miğfer
        VisualFactory.Part(PrimitiveType.Cylinder, body, new Vector3(0f, 0.88f, 0f), new Vector3(0.3f, 0.12f, 0.3f), steel);
        VisualFactory.Part(PrimitiveType.Sphere, body, new Vector3(0f, 0.99f, 0f), new Vector3(0.3f, 0.24f, 0.3f), steel);
        VisualFactory.Part(PrimitiveType.Cube, body, new Vector3(0f, 0.92f, 0.13f), new Vector3(0.24f, 0.06f, 0.06f), darkSteel);
        VisualFactory.GlowPart(PrimitiveType.Cube, body, new Vector3(0f, 0.92f, 0.152f), new Vector3(0.2f, 0.025f, 0.03f), VisorGlow, 4f);
        VisualFactory.Part(PrimitiveType.Cube, body, new Vector3(0f, 1.0f, 0f), new Vector3(0.04f, 0.2f, 0.32f), gold);

        // Sorguç
        VisualFactory.Part(PrimitiveType.Cube, body, new Vector3(0f, 1.15f, -0.05f), new Vector3(0.05f, 0.14f, 0.22f), cape, new Vector3(-15f, 0f, 0f));

        // Pelerin (üstten sabitlenip sallanır)
        capePivot = VisualFactory.CreateRoot(body, "CapePivot");
        capePivot.localPosition = new Vector3(0f, 0.74f, -0.16f);
        VisualFactory.Part(PrimitiveType.Cube, capePivot, new Vector3(0f, -0.27f, -0.02f), new Vector3(0.42f, 0.54f, 0.03f), cape);
        VisualFactory.Part(PrimitiveType.Cube, capePivot, new Vector3(0f, -0.02f, 0f), new Vector3(0.44f, 0.05f, 0.05f), gold);
    }

    private void Update() {
        float dt = Time.deltaTime;
        if (dt <= 0f || body == null) return;

        Vector3 velocity = (transform.position - lastPosition) / dt;
        lastPosition = transform.position;
        smoothedVelocity = Vector3.Lerp(smoothedVelocity, velocity, 1f - Mathf.Exp(-12f * dt));

        // Yerel uzayda hız: karakter fareye dönük olduğu için yatay hareket yana yatma olarak görünür
        Vector3 localVelocity = transform.InverseTransformDirection(smoothedVelocity);
        float speed = Mathf.Clamp(smoothedVelocity.magnitude, 0f, 10f);
        float moveFactor = Mathf.Clamp01(speed / 5f);

        walkCycle += dt * Mathf.Lerp(3f, 14f, moveFactor);

        float bob = Mathf.Sin(walkCycle * 2f) * Mathf.Lerp(0.012f, 0.035f, moveFactor);
        body.localPosition = new Vector3(0f, bob, 0f);
        body.localRotation = Quaternion.Euler(Mathf.Clamp(localVelocity.z * 1.5f, -8f, 8f), 0f, Mathf.Clamp(-localVelocity.x * 2.5f, -14f, 14f));

        float legSwing = Mathf.Sin(walkCycle) * 35f * moveFactor;
        leftLeg.localRotation = Quaternion.Euler(legSwing, 0f, 0f);
        rightLeg.localRotation = Quaternion.Euler(-legSwing, 0f, 0f);

        float capeAngle = 8f + moveFactor * 25f + Mathf.Sin(Time.time * 3f) * 3f;
        capePivot.localRotation = Quaternion.Euler(capeAngle, 0f, Mathf.Clamp(localVelocity.x * 3f, -20f, 20f));
    }

    private void Player_OnAttack(object sender, EventArgs e) {
        if (GameFlow.Instance != null && !GameFlow.Instance.IsPlaying) return;

        body.DOKill(true);
        body.DOPunchScale(new Vector3(0.12f, -0.08f, 0.12f), 0.15f, 8, 0.5f);
    }

    private void Player_OnDash() {
        body.DOKill(true);
        body.DOPunchScale(new Vector3(-0.15f, 0.1f, 0.35f), 0.2f, 6, 0.5f);
    }

    private void Player_OnSkillUsed(object sender, EventArgs e) {
        body.DOKill(true);
        body.DOPunchScale(Vector3.one * 0.25f, 0.4f, 6, 0.6f);
    }

    private void OnDestroy() {
        if (body != null) body.DOKill();

        if (Player.Instance != null) {
            Player.Instance.OnAttack -= Player_OnAttack;
            Player.Instance.OnDash -= Player_OnDash;
            Player.Instance.OnSkillUsed -= Player_OnSkillUsed;
        }
    }
}
