using System;
using System.IO;
using System.Numerics;
using OverlayService = KamiToolKit.UiOverlay.OverlayController;

namespace CurrencyAlert.Classes;

public class OverlayController : IDisposable {
    private readonly OverlayService overlayService;

    public OverlayListNode OverlayListNode { get; }

    private static string ListNodeConfigPath => Path.Combine(Service.PluginInterface.ConfigDirectory.FullName, "ListNode.style.json");

    public readonly CurrencyWarningNode SampleNode = new() {
        Height = 32.0f,
        IsVisible = true,
        TextTooltip = "Overlay from CurrencyAlert plugin",
    };

    public OverlayController() {
        SampleNode.Load();

        OverlayListNode = new OverlayListNode {
            Size = new Vector2(600.0f, 200.0f),
            Position = new Vector2(1920.0f, 1024.0f) / 2.0f,
            IsVisible = true,
        };
        OverlayListNode.Load(ListNodeConfigPath);

        overlayService = new OverlayService();
        overlayService.AddNode(OverlayListNode);
    }

    public void Dispose() {
        overlayService.Dispose();
        SampleNode.Dispose();
    }

    public void Save() {
        OverlayListNode.Save(ListNodeConfigPath);
        SampleNode.Save();
    }
}
