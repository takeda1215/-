using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ButtonPhysics : MonoBehaviour
{
    public float restPosition = 0f;
    public float damping = 5f;

    Rigidbody rb;
    ButtonModel model;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        model = new ButtonModel(); // ただし本体スクリプトから参照を受け取ります
    }

    public void Initialize(ButtonModel model)
    {
        this.model = model;
        model.OnDisplacementChanged += UpdateDisplacement;
    }

    void UpdateDisplacement(float disp)
    {
        // disp は負の値（下方向）です
        // ここでは瞬時に transform を動かす or FixedUpdateで物理挙動を行うか選択します
        Vector3 pos = transform.localPosition;
        pos.y = restPosition + disp;
        transform.localPosition = pos;
        rb.velocity = Vector3.zero;
    }

    // もしバネ→物理で戻したい場合は FixedUpdate を使う設計も可能です
}
