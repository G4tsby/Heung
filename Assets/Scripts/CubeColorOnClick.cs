using UnityEngine;

/// <summary>
/// UI Button의 On Click()에서 호출해 3D 오브젝트(큐브 등)의 색을 바꿉니다.
/// URP Lit(_BaseColor)과 Built-in 등(_Color)을 모두 시도합니다.
/// </summary>
public class CubeColorOnClick : MonoBehaviour
{
    [SerializeField]
    private Renderer targetRenderer;

    [SerializeField]
    private Color color = Color.cyan;

    [SerializeField]
    private Color secondColor = Color.magenta;

    /// <summary>첫 번째 버튼 On Click()에 연결.</summary>
    public void SetCubeColor()
    {
        if (targetRenderer == null)
        {
            Debug.LogWarning($"{nameof(CubeColorOnClick)}: Target Renderer가 비어 있습니다.", this);
            return;
        }

        ApplyColor(targetRenderer, color);
    }

    /// <summary>두 번째 버튼 On Click()에 연결 (다른 색).</summary>
    public void SetCubeColorSecond()
    {
        if (targetRenderer == null)
        {
            Debug.LogWarning($"{nameof(CubeColorOnClick)}: Target Renderer가 비어 있습니다.", this);
            return;
        }

        ApplyColor(targetRenderer, secondColor);
    }

    /// <summary>런타임에 다른 색으로 바꾸고 싶을 때.</summary>
    public void SetCubeColor(Color c)
    {
        color = c;
        SetCubeColor();
    }

    private static void ApplyColor(Renderer renderer, Color c)
    {
        var mat = renderer.material;
        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", c);
        else if (mat.HasProperty("_Color"))
            mat.color = c;
        else
            mat.color = c;
    }
}
