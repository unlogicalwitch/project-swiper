using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class MainMenuScreen : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI startText;

    private void Start()
    {
        startText.DOFade(0.25f, 1.5f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }
}
