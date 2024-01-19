using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
using ColorMC.Android.UI.Activity;

namespace ColorMC.Android.UI;

public class Tab4Fragment : Fragment
{
    public TabsDialogFragment Tabs { get; init; }

    private LinearLayout linearLayout;

    public override View OnCreateView(LayoutInflater? inflater,
        ViewGroup? container, Bundle? savedInstanceState)
    {
        // 使用 inflater.inflate 方法加载布局文件
        var view = inflater!.Inflate(Resource.Layout.fragment_tab4, container, false)!;
        linearLayout = view.FindViewById<LinearLayout>(Resource.Id.tab4_list)!;

        foreach (var item in MainActivity.Games.Values)
        {
            var item1 = inflater!.Inflate(Resource.Layout.game_item, linearLayout, false)!;
            var name = item1.FindViewById<TextView>(Resource.Id.game_item_name)!;
            var image = item1.FindViewById<ImageView>(Resource.Id.game_item_img)!;

            name.Text = item.Name;
            image.SetImageBitmap(item.Icon);

            item1.Click += (a, b)=> 
            {
                Tabs.SetGame(item);
            };

            var layoutParams = new LinearLayout.LayoutParams(
                    ViewGroup.LayoutParams.MatchParent,
                    ViewGroup.LayoutParams.WrapContent);

            int marginInDp = 3; // 假设我们要设置16dp的外边距
            float scale = Context.Resources.DisplayMetrics.Density;
            int marginInPx = (int)(marginInDp * scale + 0.5f);

            layoutParams.SetMargins(marginInPx, marginInPx, marginInPx, marginInPx);

            // 将LayoutParams应用到你的视图上
            item1.LayoutParameters = layoutParams;

            if (item == Tabs.Game)
            {
                // 创建一个GradientDrawable对象
                GradientDrawable border = new GradientDrawable();
                border.SetColor(0x0081e4); // 设置内部填充颜色
                border.SetStroke(2, Color.ParseColor("#0064b1")); // 设置边框宽度和颜色

                // 将创建的Drawable作为背景设置到LinearLayout
                item1.Background = border;
            }

            linearLayout.AddView(item1);
        }

        return view;
    }
}