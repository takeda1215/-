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
        model = new ButtonModel(); // �������{�̃X�N���v�g����Q�Ƃ��󂯎��܂�
    }

    public void Initialize(ButtonModel model)
    {
        this.model = model;
        model.OnDisplacementChanged += UpdateDisplacement;
    }

    void UpdateDisplacement(float disp)
    {
        // disp �͕��̒l�i�������j�ł�
        // �����ł͏u���� transform �𓮂��� or FixedUpdate�ŕ����������s�����I�����܂�
        Vector3 pos = transform.localPosition;
        pos.y = restPosition + disp;
        transform.localPosition = pos;
        rb.velocity = Vector3.zero;
    }

    // �����o�l�������Ŗ߂������ꍇ�� FixedUpdate ���g���݌v���\�ł�
}
