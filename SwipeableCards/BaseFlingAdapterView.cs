/*****************************************************************
 * This project has been converted to C# from SwipeCards project
 * of Diolor which can be found in this link: 
 * https://github.com/Diolor/Swipecards
 * Also I added another functionality so you can swipe the card
 * all 4 sides instead of just 2 sides.
 * Developer: Erkin YILDIZ - Hologramsoft http://www.hologramsoft.com
 * ****************************************************************/

using System;
using Android.Widget;
using Android.Content;
using Android.Runtime;

namespace SwipeableCards
{
	abstract public class BaseFlingAdapterView : AdapterView
	{
		public int HeightMeasureSpec {
			get;
			private set;
		}
		public int WidthMeasureSpec {
			get;
			private set;
		}

		public BaseFlingAdapterView(Context context) : base(context) {
			
		}

		public BaseFlingAdapterView(Context context, Android.Util.IAttributeSet attrs) : base(context, attrs) {
			
		}

		public BaseFlingAdapterView(Context context, Android.Util.IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle) {
			
		}

		public BaseFlingAdapterView(IntPtr javaReference, JniHandleOwnership jni) : base (javaReference, jni) 
		{
			
		}

		public override void SetSelection (int position)
		{
			throw new Java.Lang.UnsupportedOperationException ("Notsupported");
		}

		protected override void OnMeasure (int widthMeasureSpec, int heightMeasureSpec)
		{
			base.OnMeasure (widthMeasureSpec, heightMeasureSpec);
			this.HeightMeasureSpec = widthMeasureSpec;
			this.WidthMeasureSpec = heightMeasureSpec;
		}
	}
}

