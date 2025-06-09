using UnityEngine;

[RequireComponent(typeof(ButtonPhysics), typeof(ButtonView))]
public class ButtonCoordinator : MonoBehaviour
{
    [Header("Receiver & Scaling")]
    public SerialReceiver serialReceiver;    // そのまま使う
    public float scalingFactor = 0.022f;     // kg→Nへの固有スケール

    [Header("Model Parameters")]
    public ButtonModel model = new ButtonModel(); // Inspector で設定可

    ButtonPhysics physics;
    ButtonView view;

    // 直前の受信文字列を保存して、同じデータを何度もパースしないようにする
    string _lastReceived = "";

    void Awake()
    {
        physics = GetComponent<ButtonPhysics>();
        view = GetComponent<ButtonView>();

        // ButtonModelのパラメータは Inspector で設定
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
        // 新しい受信データがあれば処理する
        var data = serialReceiver.receivedData;
        if (!string.IsNullOrEmpty(data) && data != _lastReceived)
        {
            _lastReceived = data;

            // "Weight: " プレフィックスを削ってパース
            var trimmed = data.Replace("Weight:", "").Trim();
            if (float.TryParse(trimmed, out float kg))
            {
                // ButtonModel を更新し、イベント連動でPhysics/Viewが動く
                model.UpdateState(kg, scalingFactor);
            }
            else
            {
                Debug.LogWarning($"Serial parse failed: '{data}'");
            }
        }
    }
}
