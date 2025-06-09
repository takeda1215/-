using UnityEngine;

[RequireComponent(typeof(ButtonPhysics), typeof(ButtonView))]
public class ButtonCoordinator : MonoBehaviour
{
    [Header("Receiver & Scaling")]
    public SerialReceiver serialReceiver;    // ���̂܂܎g��
    public float scalingFactor = 0.022f;     // kg��N�ւ̌ŗL�X�P�[��

    [Header("Model Parameters")]
    public ButtonModel model = new ButtonModel(); // Inspector �Őݒ��

    ButtonPhysics physics;
    ButtonView view;

    // ���O�̎�M�������ۑ����āA�����f�[�^�����x���p�[�X���Ȃ��悤�ɂ���
    string _lastReceived = "";

    void Awake()
    {
        physics = GetComponent<ButtonPhysics>();
        view = GetComponent<ButtonView>();

        // ButtonModel�̃p�����[�^�� Inspector �Őݒ�
        // model.buttonType      = ...;
        // model.thresholdForce  = ...;
        // model.hardness        = ...;
        // model.maxDisplacement = ...;
        // model.activationKg    = ...;

        physics.Initialize(model);
        view.Initialize(model, model.maxDisplacement);
    }

    void Update()
    {
        // �V������M�f�[�^������Ώ�������
        var data = serialReceiver.receivedData;
        if (!string.IsNullOrEmpty(data) && data != _lastReceived)
        {
            _lastReceived = data;

            // "Weight: " �v���t�B�b�N�X������ăp�[�X
            var trimmed = data.Replace("Weight:", "").Trim();
            if (float.TryParse(trimmed, out float kg))
            {
                // ButtonModel ���X�V���A�C�x���g�A����Physics/View������
                model.UpdateState(kg, scalingFactor);
            }
            else
            {
                Debug.LogWarning($"Serial parse failed: '{data}'");
            }
        }
    }
}
