using UnityEngine;

[RequireComponent(typeof(Animator))]
public class FingerAnimatorController : MonoBehaviour
{
    Animator anim;
    const string paramName = "BendTime";

    // 0N→1f, 10N→0f（例）で動かしたいなら逆にする
    public float maxForce = 10f;
    [Range(0, 10f)]
    public float testForce = 0f;    // Inspector で値をいじるための変数

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        // ここでは testForce を 0～maxForce で変化させ
        // 0 → 伸びた状態、maxForce → 完全に曲がった状態
        float t = Mathf.Clamp01(testForce / maxForce);
        anim.SetFloat(paramName, t);
    }
}
