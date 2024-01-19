using Android.App;
using Android.OS;
using Android.Views;
using AndroidX.ViewPager2.Widget;
using ColorMC.Android.GLRender;
using Google.Android.Material.Tabs;
using DialogFragment = AndroidX.Fragment.App.DialogFragment;

namespace ColorMC.Android.UI;

public class TabsDialogFragment : DialogFragment, TabLayoutMediator.ITabConfigurationStrategy
{
    public ushort Width, Height;
    public GameRender.DisplayType ShowType;
    public bool FlipY;

    public GameRender Game { get; init; }
    public GLSurface Surface { get; init; }

    private TabLayout tabLayout;
    private ViewPager2 viewPager;

    public TabsDialogFragment(GLSurface surface)
    {
        Surface = surface;
        Game = surface.NowGame;
        if (Game == null)
        {
            return;
        }

        Width = Game.GameWidth;
        Height = Game.GameHeight;
        FlipY = Game.FlipY;
        ShowType = Game.ShowType;
    }

    public void OnConfigureTab(TabLayout.Tab p0, int p1)
    {
        switch (p1)
        {
            case 0:
                p0.SetText(Resource.String.tabs_text4);
                break;
            case 1:
                p0.SetText(Resource.String.tabs_text1);
                break;
            case 2:
                p0.SetText(Resource.String.tabs_text2);
                break;
            case 3:
                p0.SetText(Resource.String.tabs_text3);
                break;
        }

    }

    public override Dialog OnCreateDialog(Bundle? savedInstanceState)
    {
        Dialog dialog = base.OnCreateDialog(savedInstanceState);
        dialog.RequestWindowFeature((int)WindowFeatures.NoTitle);
        return dialog;
    }

    public override View OnCreateView(LayoutInflater? inflater,
        ViewGroup? container, Bundle? savedInstanceState)
    {
        var view = inflater!.Inflate(Resource.Layout.dialog_with_tabs, container, false)!;
        tabLayout = view.FindViewById<TabLayout>(Resource.Id.tabs)!;
        viewPager = view.FindViewById<ViewPager2>(Resource.Id.view_pager)!;
        SetupViewPager(viewPager);
        new TabLayoutMediator(tabLayout, viewPager, this).Attach();
        return view;
    }

    public void SetWindow()
    {
        if (Game == null)
        {
            return;
        }

        if (Width != Game.GameWidth || Height != Game.GameHeight)
        {
            Game.SetSize(Width, Height);
        }

        Game.ShowType = ShowType;
        Game.FlipY = FlipY;
        Dismiss();
    }

    public void SetGame(GameRender item)
    {
        Surface.SetGame(item);
        Dismiss();
    }

    private void SetupViewPager(ViewPager2 viewPager)
    {
        var adapter = new ViewPagerAdapter(this);
        // 添加Fragment
        adapter.AddFragment(new Tab4Fragment()
        {
            Tabs = this
        });
        adapter.AddFragment(new Tab1Fragment()
        {
            Tabs = this
        });
        adapter.AddFragment(new Tab2Fragment()
        {
            Tabs = this
        });
        adapter.AddFragment(new Tab3Fragment()
        {
            Tabs = this
        });
        viewPager.Adapter = adapter;
    }
}
