using Android.OS;
using Android.Views;
using AndroidX.Fragment.App;

namespace ColorMC.Android.UI;

public class Tab2Fragment : Fragment
{
    public TabsDialogFragment Tabs { get; init; }

    public override View OnCreateView(LayoutInflater? inflater,
        ViewGroup? container, Bundle? savedInstanceState)
    {
        // 使用 inflater.inflate 方法加载布局文件
        var view = inflater!.Inflate(Resource.Layout.fragment_tab2, container, false)!;

        return view;
    }
}
