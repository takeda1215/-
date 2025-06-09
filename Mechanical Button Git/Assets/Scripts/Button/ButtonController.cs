using UnityEngine;

public class ButtonController : MonoBehaviour
{
    public enum ButtonType { Keyboard, RemoteControl, Mouse, Sponge, TouchPanel }
    public ButtonType buttonType;                 // ボタンの種類を指定

    public SerialReceiver serialReceiver;         // SerialReceiver からデータを受け取る
    public GameObject fingerObject;               // 指（Cube）オブジェクト
    public float hardness = 119f;                 // Hardness（バネ定数 N/m）
    public float thresholdForce = 0.6011f;        // 押し込み閾値（N）
    public float maxDisplacement = -0.00132047f;  // 最大沈み込み量（m）
    public float damping = 5f;                    // 減衰（ダンパの効果）
    public float restPosition = 0f;               // ボタンの初期位置（Y座標）
    public float scalingFactor = 20f;             // センサー値から力への換算係数

    private bool isPressed = false;
    private Rigidbody rb;
    private AudioSource audioSource;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>(); // AudioSource を取得

        // 指オブジェクトを初期状態で非表示にする
        if (fingerObject != null)
        {
            fingerObject.SetActive(false);
        }
    }

    void Update()
    {
        if (serialReceiver != null && !string.IsNullOrEmpty(serialReceiver.receivedData))
        {
            string processedData = serialReceiver.receivedData.Replace("Weight: ", "").Trim();
            if (float.TryParse(processedData, out float parsedData))
            {
                float appliedForce = parsedData * scalingFactor * 9.81f; // kg → N に変換


                // 指オブジェクトの表示・非表示を切り替え
                if (parsedData > 1f)  //appliedForceに変更
                {
                    ShowFinger();
                }
                else
                {
                    HideFinger();
                }

                // スポンジ・タッチパネルの場合は閾値を無視して沈み込みを適用
                if (buttonType == ButtonType.Sponge || buttonType == ButtonType.TouchPanel)
                {
                    float displacement = Mathf.Clamp(-appliedForce / hardness, maxDisplacement, 0f);
                    Vector3 newPosition = transform.position;
                    newPosition.y = restPosition + displacement;
                    transform.position = newPosition;

                    // 変位量が一定値を超えた場合に音を鳴らす
                    if (Mathf.Abs(displacement) >= 0.001f && !isPressed)
                    {
                        OnButtonPressed();
                    }
                    else if (Mathf.Abs(displacement) < 0.001f && isPressed)
                    {
                        OnButtonReleased();
                    }
                }
                else
                {
                    // 他のボタンは通常の閾値を使った処理
                    if (appliedForce >= thresholdForce && !isPressed)
                    {
                        OnButtonPressed();
                    }
                    else if (appliedForce < thresholdForce && isPressed)
                    {
                        OnButtonReleased();
                    }
                }
            }
            else
            {
                Debug.LogError("Failed to parse received data: " + serialReceiver.receivedData);
            }
        }
    }


    void FixedUpdate()
    {

        if (isPressed)
        {
            Vector3 fixedPosition = transform.position;

            // Touch Panel の場合は沈み込みを無効化して固定
            if (buttonType == ButtonType.TouchPanel)
            {
                fixedPosition.y = restPosition; // 初期位置に固定
                transform.position = fixedPosition;
                rb.velocity = Vector3.zero; // 速度をリセット
                return; // ここで処理を終了（他のボタンの処理をスキップ）
            }

            // 他のボタンは最大沈み込み位置に固定
            fixedPosition.y = restPosition + maxDisplacement;
            transform.position = fixedPosition;
            rb.velocity = Vector3.zero; // 速度をリセットして振動を防止
        }
        else
        {
            // バネのように戻る力を加える（Touch Panel は除外）
            if (buttonType != ButtonType.TouchPanel)
            {
                float springDisplacement = restPosition - transform.position.y;
                float springForceApplied = hardness * springDisplacement - damping * rb.velocity.y;
                rb.AddForce(Vector3.up * springForceApplied);
            }
        }

        // Y軸の上限を 0 に制限
        if (transform.position.y > restPosition)
        {
            Vector3 clampedPosition = transform.position;
            clampedPosition.y = restPosition;
            transform.position = clampedPosition;

            rb.velocity = Vector3.zero; // 上方向の速度をリセット
        }
    }


    void OnButtonPressed()
    {
        isPressed = true;
        Debug.Log("Button Pressed");

        // 音を再生
        if (audioSource != null)
        {
            audioSource.Play();
        }
    }

    void OnButtonReleased()
    {
        isPressed = false;
        Debug.Log("Button Released");
    }

    void ShowFinger()
    {
        if (fingerObject != null && !fingerObject.activeSelf)
        {
            fingerObject.SetActive(true);
        }
    }

    void HideFinger()
    {
        if (fingerObject != null && fingerObject.activeSelf)
        {
            fingerObject.SetActive(false);
        }
    }
}
