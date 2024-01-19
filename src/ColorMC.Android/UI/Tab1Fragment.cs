using Android.OS;
using Android.Text;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
using ColorMC.Android.GLRender;
using System;

namespace ColorMC.Android.UI;

public class Tab1Fragment : Fragment
{
    public TabsDialogFragment Tabs { get; init; }

    private EditText _width, _height;
    private RadioGroup _resolution, _display;
    private CheckBox _filpY;
    private Button _button1;

    public override View OnCreateView(LayoutInflater? inflater,
        ViewGroup? container, Bundle? savedInstanceState)
    {
        // 使用 inflater.inflate 方法加载布局文件
        var view = inflater!.Inflate(Resource.Layout.fragment_tab1, container, false)!;
        _width = view.FindViewById<EditText>(Resource.Id.tab1_width)!;
        _height = view.FindViewById<EditText>(Resource.Id.tab1_height)!;
        _resolution = view.FindViewById<RadioGroup>(Resource.Id.tab1_resolution)!;
        _display = view.FindViewById<RadioGroup>(Resource.Id.tab1_display_type)!;
        _filpY = view.FindViewById<CheckBox>(Resource.Id.tab1_filpY)!;
        _button1 = view.FindViewById<Button>(Resource.Id.tab1_button1)!;

        _width.Text = Tabs.Width.ToString();
        _height.Text = Tabs.Height.ToString();
        _filpY.Checked = Tabs.FlipY;

        Select1();

        _resolution.CheckedChange += Resolution_CheckedChange;
        _display.CheckedChange += Display_CheckedChange;

        _width.AfterTextChanged += Width_AfterTextChanged;
        _height.AfterTextChanged += Height_AfterTextChanged;

        _filpY.Click += FilpY_Click;
        _button1.Click += Button1_Click;

        return view;
    }

    private void Button1_Click(object? sender, EventArgs e)
    {
        Tabs.SetWindow();
    }

    private void FilpY_Click(object? sender, EventArgs e)
    {
        Tabs.FlipY = _filpY.Checked;
    }

    private void Display_CheckedChange(object? sender, RadioGroup.CheckedChangeEventArgs e)
    {
        switch (e.CheckedId)
        {
            case Resource.Id.tab1_group2_1:
                Tabs.ShowType = GameRender.DisplayType.None;
                break;
            case Resource.Id.tab1_group2_2:
                Tabs.ShowType = GameRender.DisplayType.Full;
                break;
            case Resource.Id.tab1_group2_3:
                Tabs.ShowType = GameRender.DisplayType.Scale;
                break;
        }
    }

    private void Resolution_CheckedChange(object? sender, RadioGroup.CheckedChangeEventArgs e)
    {
        switch (e.CheckedId)
        {
            case Resource.Id.tab1_group1_1:
                Tabs.Width = 1280;
                Tabs.Height = 720;
                Update();
                break;
            case Resource.Id.tab1_group1_2:
                Tabs.Width = 1920;
                Tabs.Height = 1080;
                Update();
                break;
            case Resource.Id.tab1_group1_3:
                Tabs.Width = 2560;
                Tabs.Height = 1440;
                Update();
                break;
            case Resource.Id.tab1_group1_4:
                Tabs.Width = 3840;
                Tabs.Height = 2160;
                Update();
                break;
        }
    }

    private void Height_AfterTextChanged(object? sender, AfterTextChangedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_height.Text))
        {
            return;
        }
        if (ushort.TryParse(_height.Text, out var height))
        {
            Tabs.Height = height;
            Select();
        }
        else
        {
            e.Editable?.Replace(0, e.Editable.Length(), "720");
        }
    }

    private void Width_AfterTextChanged(object? sender, AfterTextChangedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_width.Text))
        {
            return;
        }
        if (ushort.TryParse(_width.Text, out var width))
        {
            Tabs.Width = width;
            Select();
        }
        else
        {
            e.Editable?.Replace(0, e.Editable.Length(), "1280");
        }
    }

    private void Update()
    {
        _width.Text = Tabs.Width.ToString();
        _height.Text = Tabs.Height.ToString();
    }

    private void Select1()
    {
        switch (Tabs.ShowType)
        {
            case GameRender.DisplayType.None:
                _display.Check(Resource.Id.tab1_group2_1);
                break;
            case GameRender.DisplayType.Full:
                _display.Check(Resource.Id.tab1_group2_2);
                break;
            case GameRender.DisplayType.Scale:
                _display.Check(Resource.Id.tab1_group2_3);
                break;
        }
    }

    private void Select()
    {
        if (Tabs.Width == 1280 && Tabs.Height == 720)
        {
            _resolution.Check(Resource.Id.tab1_group1_1);
        }
        else if (Tabs.Width == 1920 && Tabs.Height == 1080)
        {
            _resolution.Check(Resource.Id.tab1_group1_2);
        }
        else if (Tabs.Width == 2560 && Tabs.Height == 1440)
        {
            _resolution.Check(Resource.Id.tab1_group1_3);
        }
        else if (Tabs.Width == 3840 && Tabs.Height == 2160)
        {
            _resolution.Check(Resource.Id.tab1_group1_4);
        }
    }
}
