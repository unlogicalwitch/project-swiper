using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;

public class MainMenuScreen : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI startText;
    [SerializeField] private Button startButton;

    private void OnEnable()
    {
        Time.timeScale = 1f;
        
        // Kill any existing tweens on the text, reset alpha, then restart the animation
        startText.DOKill();
        var c = startText.color;
        c.a = 1f;
        startText.color = c;

        startText.DOFade(0.25f, 1.5f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    private void OnDisable()
    {
        startText.DOKill();
    }
}
