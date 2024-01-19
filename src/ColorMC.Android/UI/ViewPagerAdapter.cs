using AndroidX.Fragment.App;
using AndroidX.ViewPager2.Adapter;
using System.Collections.Generic;

namespace ColorMC.Android.UI;

public class ViewPagerAdapter : FragmentStateAdapter
{
    private readonly List<Fragment> mFragmentList = [];

    public override int ItemCount => mFragmentList.Count;

    public ViewPagerAdapter(Fragment fragment) : base(fragment)
    {

    }

    public override Fragment CreateFragment(int position)
    {
        return mFragmentList[position];
    }

    public void AddFragment(Fragment fragment)
    {
        mFragmentList.Add(fragment);
    }
}
