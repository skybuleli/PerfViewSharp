using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using PerfViewSharp.Core;
using System;
using System.Collections.Generic;

namespace PerfViewSharp.UI;

public partial class MainWindow : Window
{
    private SdlView? _sdlView;
    private MinimapView? _minimap;

    public MainWindow()
    {
        InitializeComponent();
        
        var mainGrid = this.FindControl<Grid>("MainGrid");
        if (mainGrid != null)
        {
            _sdlView = new SdlView(App.GpuContext!);
            _sdlView.SelectionChanged += OnSelectionChanged;
            _sdlView.ViewStateChanged += () => _minimap?.InvalidateVisual();
            _sdlView.VisibleHotspotsChanged += UpdateHotspotList;
            mainGrid.Children.Add(_sdlView);
        }

        var miniGrid = this.FindControl<Grid>("MinimapGrid");
        if (miniGrid != null && _sdlView != null)
        {
            _minimap = new MinimapView();
            _minimap.SetData(_sdlView.Metadata, _sdlView.State);
            _minimap.RequestOffsetChange += (offset) => _sdlView.ApplyOffset(offset);
            miniGrid.Children.Add(_minimap);
        }

        var searchBox = this.FindControl<TextBox>("GlobalSearchBox");
        if (searchBox != null)
        {
            searchBox.TextChanged += (s, e) => {
                if (_sdlView != null) {
                    _sdlView.SearchText = searchBox.Text ?? "";
                    _sdlView.InvalidateVisual();
                }
            };
        }

        var jumpBtn = this.FindControl<Button>("JumpButton");
        if (jumpBtn != null) jumpBtn.Click += (s, e) => _sdlView?.TriggerManualJump();
    }

    private void UpdateHotspotList(List<MethodStats> stats)
    {
        var list = this.FindControl<ListBox>("HotspotList");
        if (list == null) return;

        list.Items.Clear();
        foreach (var s in stats)
        {
            var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("*, Auto"), Margin = new Thickness(0, 2) };
            
            var nameBtn = new Button { 
                Content = s.Name, 
                Background = Brushes.Transparent, 
                Foreground = Brushes.White,
                Padding = new Thickness(0),
                BorderThickness = new Thickness(0),
                FontSize = 13,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left
            };
            nameBtn.Click += (sender, e) => _sdlView?.FocusMethod(s.Name);

            var timeTxt = new TextBlock { 
                Text = $"{s.TotalDurationMSec:F1}ms", 
                Foreground = Brushes.Gray, 
                FontSize = 11, 
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center 
            };

            Grid.SetColumn(nameBtn, 0);
            Grid.SetColumn(timeTxt, 1);
            grid.Children.Add(nameBtn);
            grid.Children.Add(timeTxt);

            list.Items.Add(new ListBoxItem { Content = grid, Padding = new Thickness(5, 2) });
        }
    }

    private void OnSelectionChanged(TraceBlock? block)
    {
        var nameLabel = this.FindControl<TextBlock>("MethodNameLabel");
        var timeLabel = this.FindControl<TextBlock>("TotalTimeLabel");
        var percentLabel = this.FindControl<TextBlock>("PercentLabel");
        var progress = this.FindControl<ProgressBar>("CpuProgressBar");

        if (block == null || _sdlView?.Metadata == null)
        {
            if (nameLabel != null) nameLabel.Text = "请选择一个方法块...";
            return;
        }

        var stats = _sdlView.Metadata.GetStatsForMethod(block.Name);
        if (nameLabel != null) nameLabel.Text = stats.Name;
        if (timeLabel != null) timeLabel.Text = $"{stats.TotalDurationMSec:F2} ms ({stats.CallCount} 次调用)";
        if (percentLabel != null) percentLabel.Text = $"{stats.Percentage:F1} %";
        if (progress != null) progress.Value = stats.Percentage;
    }
}