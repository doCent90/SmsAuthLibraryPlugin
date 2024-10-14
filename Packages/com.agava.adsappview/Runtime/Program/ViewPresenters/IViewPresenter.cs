using System;
using AdsAppView.DTO;

public interface IViewPresenter
{
    bool Enable { get; }

    event Action Enabled;
    event Action Disabled;

    void Show(SpriteData spriteData);
}
