using System;
using AdsAppView.DTO;
using UnityEngine;

public class WebViewPresenter : MonoBehaviour, IViewPresenter
{
    public bool Enable => throw new NotImplementedException();

    public event Action Enabled;
    public event Action Disabled;

    public void Show(PopupData spriteData) => throw new NotImplementedException();
}
