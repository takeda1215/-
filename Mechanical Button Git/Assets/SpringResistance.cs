using UnityEngine;

public class SpringResistance : MonoBehaviour
{
    public Transform fingerTransform; // 指のオブジェクト
    public float springConstant = 100f; // ばね定数（強さ）
    public float dampingFactor = 10f;   // 減衰定数（抵抗の調整）
    public float maxCompression = 1.0f; // 最大押し込み量（ばねの限界）

    private Vector3 initialPosition;  // ばねオブジェクトの初期位置
    private Rigidbody springRigidbody;

    void Start()
    {
        initialPosition = transform.position;
        springRigidbody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        ApplySpringResistance();
    }

    void ApplySpringResistance()
    {
        // 指とばねのオブジェクト間の距離を計算
        float distance = Vector3.Distance(transform.position, fingerTransform.position);

        // ばねの変位量を計算（初期位置からどれだけ押されたか）
        float displacement = Mathf.Clamp(initialPosition.y - transform.position.y, 0, maxCompression);

        // フックの法則を適用して反発力を計算
        float springForce = springConstant * displacement;

        // 減衰力を追加
        float dampingForce = dampingFactor * springRigidbody.velocity.y;

        // 押す力とばねの反発力のバランスを取る
        float totalForce = springForce - dampingForce;

        // ばねオブジェクトに力を加える
        Vector3 force = new Vector3(0, totalForce, 0);
        springRigidbody.AddForce(force);

        // ばねオブジェクトの位置を調整
        if (displacement > 0)
        {
            transform.position = Vector3.Lerp(transform.position, fingerTransform.position, Time.deltaTime);
        }
    }
}
