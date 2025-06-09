using System;
using UnityEngine;

[Serializable]
public class ButtonModel
{
    public enum ButtonType { Keyboard, RemoteControl, Mouse, Sponge, TouchPanel }

    public ButtonType buttonType;
    public float thresholdForce;     // N
    public float hardness;          // N/m
    public float maxDisplacement;   // m
    public float activationKg = 0f; // �w���o���g���K�[�ikg�j

    // ���ۂɊO�����炱�̃��\�b�h���Ăяo���āukg�v��n��
    public void UpdateState(float kg, float scalingFactor)
    {
        float force = kg * scalingFactor * 9.81f;
        bool prevPressed = IsPressed;
        float prevDisp = Displacement;

        if (buttonType == ButtonType.Sponge || buttonType == ButtonType.TouchPanel)
        {
            Displacement = Mathf.Clamp(-force / hardness, maxDisplacement, 0f);
            IsPressed = Mathf.Abs(Displacement) > 0.001f;
        }
        else
        {
            IsPressed = force >= thresholdForce;
            Displacement = IsPressed ? maxDisplacement : 0f;
        }

        // �w�\���g���K�[
        if (kg >= activationKg && !FingerShown)
        {
            FingerShown = true;         // �� ������ǉ�
            OnFingerShow?.Invoke();
        }
        else if (kg < activationKg && FingerShown)
        {
            FingerShown = false;        // �� ������ǉ�
            OnFingerHide?.Invoke();
        }

        // �����^����C�x���g
        if (IsPressed && !prevPressed) OnPressed?.Invoke();
        if (!IsPressed && prevPressed) OnReleased?.Invoke();
        // �ψʕω��C�x���g
        if (Mathf.Abs(Displacement - prevDisp) > 0.00001f)
            OnDisplacementChanged?.Invoke(Displacement);
    }

    // ���
    public bool IsPressed { get; private set; }
    public float Displacement { get; private set; }
    public bool FingerShown { get; private set; }

    // �C�x���g
    public event Action OnPressed;
    public event Action OnReleased;
    public event Action<float> OnDisplacementChanged;
    public event Action OnFingerShow;
    public event Action OnFingerHide;
}
