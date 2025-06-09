using UnityEngine;

public class SpringResistance : MonoBehaviour
{
    public Transform fingerTransform; // �w�̃I�u�W�F�N�g
    public float springConstant = 100f; // �΂˒萔�i�����j
    public float dampingFactor = 10f;   // �����萔�i��R�̒����j
    public float maxCompression = 1.0f; // �ő剟�����ݗʁi�΂˂̌��E�j

    private Vector3 initialPosition;  // �΂˃I�u�W�F�N�g�̏����ʒu
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
        // �w�Ƃ΂˂̃I�u�W�F�N�g�Ԃ̋������v�Z
        float distance = Vector3.Distance(transform.position, fingerTransform.position);

        // �΂˂̕ψʗʂ��v�Z�i�����ʒu����ǂꂾ�������ꂽ���j
        float displacement = Mathf.Clamp(initialPosition.y - transform.position.y, 0, maxCompression);

        // �t�b�N�̖@����K�p���Ĕ����͂��v�Z
        float springForce = springConstant * displacement;

        // �����͂�ǉ�
        float dampingForce = dampingFactor * springRigidbody.velocity.y;

        // �����͂Ƃ΂˂̔����͂̃o�����X�����
        float totalForce = springForce - dampingForce;

        // �΂˃I�u�W�F�N�g�ɗ͂�������
        Vector3 force = new Vector3(0, totalForce, 0);
        springRigidbody.AddForce(force);

        // �΂˃I�u�W�F�N�g�̈ʒu�𒲐�
        if (displacement > 0)
        {
            transform.position = Vector3.Lerp(transform.position, fingerTransform.position, Time.deltaTime);
        }
    }
}
