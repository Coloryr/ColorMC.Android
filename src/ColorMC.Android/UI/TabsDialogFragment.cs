using Android.App;
using Android.OS;
using Android.Views;
using AndroidX.ViewPager2.Widget;
using ColorMC.Android.GLRender;
using Google.Android.Material.Tabs;
using static Google.Android.Material.Tabs.TabLayoutMediator;
using DialogFragment = AndroidX.Fragment.App.DialogFragment;

namespace ColorMC.Android.UI;

public class TabsDialogFragment : DialogFragment, ITabConfigurationStrategy
{
    public int Width, Height;
    public GLSurface.DisplayType ShowType;
    public bool FlipY;

    public GLSurface Render { get; init; }

    private TabLayout tabLayout;
    private ViewPager2 viewPager;

    public void OnConfigureTab(TabLayout.Tab p0, int p1)
    {
        switch (p1)
        {
            case 0:
                p0.SetText(Resource.String.tabs_text1);
                break;
            case 1:
                p0.SetText(Resource.String.tabs_text2);
                break;
            case 2:
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
        if (Render == null)
        {
            return;
        }


    }

    private void SetupViewPager(ViewPager2 viewPager)
    {
        var adapter = new ViewPagerAdapter(this);
        // 添加Fragment
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
