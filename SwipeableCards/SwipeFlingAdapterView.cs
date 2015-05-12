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
using Android.Content.Res;
using Android.Views;
using Android.Database;
using Android.Runtime;

namespace SwipeableCards
{
	public class SwipeFlingAdapterView : BaseFlingAdapterView, FlingCardListener.IFlingListener
	{

		private int MAX_VISIBLE = 4;
		private int MIN_ADAPTER_STACK = 6;
		private float ROTATION_DEGREES = 15f;

		private BaseAdapter mAdapter;
		private int LAST_OBJECT_IN_STACK = 0;
		private IOnFlingListener mFlingListener;
		private AdapterDataSetObserver mDataSetObserver;
		private bool mInLayout = false;
		private View mActiveCard = null;
		private IOnItemClickListener mOnItemClickListener;
		private FlingCardListener flingCardListener;

		public SwipeFlingAdapterView (Context context) : this(context, null) {
		}

		public SwipeFlingAdapterView (Context context, Android.Util.IAttributeSet attrs) : this(context, attrs, Resource.Attribute.SwipeFlingStyle) {
		}

		public SwipeFlingAdapterView (Context context, Android.Util.IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle) {
			TypedArray a = context.ObtainStyledAttributes (attrs, Resource.Styleable.SwipeFlingAdapterView, defStyle, 0);
			MAX_VISIBLE = a.GetInt (Resource.Styleable.SwipeFlingAdapterView_max_visible, MAX_VISIBLE);
			MIN_ADAPTER_STACK = a.GetInt (Resource.Styleable.SwipeFlingAdapterView_min_adapter_stack, MIN_ADAPTER_STACK);
			ROTATION_DEGREES = a.GetFloat (Resource.Styleable.SwipeFlingAdapterView_rotation_degrees, ROTATION_DEGREES);
			a.Recycle ();
		}

		public SwipeFlingAdapterView(IntPtr javaReference, JniHandleOwnership jni) : base (javaReference, jni)
		{
		}

		public void init(Context context, BaseAdapter mAdapter) {
			if (context is IOnFlingListener) {
				mFlingListener = (IOnFlingListener)context;
			} else {
				throw new Java.Lang.RuntimeException ("Activity does not implement SwipeFlingAdapterView.onFlingListener");
			}

			if (context is IOnItemClickListener) {
				mOnItemClickListener = (IOnItemClickListener)context;
			}


			RawAdapter = mAdapter.JavaCast<Java.Lang.Object>();
		}

		public override View SelectedView {
			get {
				return mActiveCard;
			}
		}

		public override void RequestLayout ()
		{
			if (!mInLayout) {
				base.RequestLayout ();
			}
		}

		protected override void OnLayout (bool changed, int left, int top, int right, int bottom)
		{
			base.OnLayout (changed, left, top, right, bottom);
			// if we don't have an adapter we don't need to do anything
			if (mAdapter == null) {
				return;
			}

			mInLayout = true;
			int adapterCount = mAdapter.Count;

			if (adapterCount == 0) {
				RemoveAllViewsInLayout ();
			} else {
				View topCard = GetChildAt (LAST_OBJECT_IN_STACK);
				if (mActiveCard != null && topCard != null && topCard == mActiveCard) {
					RemoveViewsInLayout (0, LAST_OBJECT_IN_STACK);
					layoutChildren (1, adapterCount);
				} else {
					// Reset the UI and set top view listener
					RemoveAllViewsInLayout();
					layoutChildren (0, adapterCount);
					setTopView ();
				}
			}

			mInLayout = false;

			if (adapterCount < MAX_VISIBLE)
				mFlingListener.onAdapterAboutToEmpty (adapterCount);
		}
			
		public override bool OnTouchEvent (MotionEvent e)
		{
			return base.OnTouchEvent (e);
		}

		private void layoutChildren(int startingIndex, int adapterCount) {
			while (startingIndex < Math.Min (adapterCount, MAX_VISIBLE)) {
				View newUnderChild = mAdapter.GetView (startingIndex, null, this);
				if (newUnderChild.Visibility != ViewStates.Gone) 
				{
					makeAndAddView (newUnderChild);
					LAST_OBJECT_IN_STACK = startingIndex;
				}
				startingIndex++;
			}
		}

		private void makeAndAddView(View child) {
			FrameLayout.LayoutParams lp = (FrameLayout.LayoutParams)child.LayoutParameters;
			AddViewInLayout (child, 0, lp, true);

			bool needToMeasure = child.IsLayoutRequested;
			if (needToMeasure) {
				int childWidthSpec = GetChildMeasureSpec (WidthMeasureSpec, PaddingLeft + PaddingRight + lp.LeftMargin + lp.RightMargin, lp.Width);
				int childHeightSpec = GetChildMeasureSpec (HeightMeasureSpec, PaddingTop + PaddingBottom + lp.TopMargin + lp.BottomMargin, lp.Height);
				child.Measure (childWidthSpec, childHeightSpec);
			} else {
				CleanupLayoutState (child);
			}

			int w = child.MeasuredWidth;
			int h = child.MeasuredHeight;

			GravityFlags gravity = lp.Gravity;
			if ((int) gravity == -1) {
				gravity = GravityFlags.Top | GravityFlags.Start;
			}

			int layoutDirection = (int) LayoutDirection;
			GravityFlags absoluteGravity = Gravity.GetAbsoluteGravity (gravity, (GravityFlags) layoutDirection);
			GravityFlags verticalGravity = gravity & GravityFlags.VerticalGravityMask;

			int childLeft;
			int childTop;
			switch (absoluteGravity & GravityFlags.HorizontalGravityMask) {
			case GravityFlags.CenterHorizontal:
				childLeft = (Width + PaddingLeft - PaddingRight - w) / 2 + lp.LeftMargin - lp.RightMargin;
				break;
			case GravityFlags.End:
				childLeft = Width + PaddingRight - w - lp.RightMargin;
				break;
			case GravityFlags.Start:
			default:
				childLeft = PaddingLeft + lp.LeftMargin;
				break;
			}
			switch (verticalGravity) {
			case GravityFlags.CenterVertical:
				childTop = (Height + PaddingTop - PaddingBottom - h) / 2 + lp.TopMargin - lp.BottomMargin;
				break;
			case GravityFlags.Bottom:
				childTop = Height - PaddingBottom - h - lp.BottomMargin;
				break;
			case GravityFlags.Top:
			default:
				childTop = PaddingTop + lp.TopMargin;
				break;
			}

			child.Layout (childLeft, childTop, childLeft + w, childTop + h);
		}

		// Set the top view and add the fling listener
		private void setTopView() {
			if (ChildCount > 0) {
				mActiveCard = GetChildAt (LAST_OBJECT_IN_STACK);
				if (mActiveCard != null) {
					flingCardListener = new FlingCardListener (mActiveCard, mAdapter.GetItem (0), this, ROTATION_DEGREES);
				}
				mActiveCard.SetOnTouchListener (flingCardListener);
			}
		}

		#region IFlingListener implementation

		public void onCardExited ()
		{
			mActiveCard = null;
			mFlingListener.removeFirstObjectInAdapter ();
		}

		public void leftExit (object dataObject)
		{
			mFlingListener.onLeftCardExit (dataObject);
		}

		public void rightExit (object dataObject)
		{
			mFlingListener.onRightCardExit (dataObject);
		}

		public void topExit (object dataObject)
		{
			mFlingListener.onTopCardExit (dataObject);
		}

		public void bottomExit (object dataObject)
		{
			mFlingListener.onBottomCardExit (dataObject);
		}

		public void onClick (object dataObject)
		{
			if (mOnItemClickListener != null)
				mOnItemClickListener.onItemClicked (0, dataObject);
		}

		public void onScroll (float scrollProgressPercentX, float scrollProgressPercentY)
		{
			mFlingListener.onScroll (scrollProgressPercentX, scrollProgressPercentY);
		}

		#endregion

		public FlingCardListener getTopCardListener() {
			if (flingCardListener == null)
				throw new NullReferenceException ();
			return flingCardListener;
		}

		public void setMaxVisible(int MAX_VISIBLE) {
			this.MAX_VISIBLE = MAX_VISIBLE;
		}

		public void setMinStackInAdapter(int MIN_ADAPTER_STACK){
			this.MIN_ADAPTER_STACK = MIN_ADAPTER_STACK;
		}

		public Java.Lang.Object Adapter {
			get { return RawAdapter;}
			set { RawAdapter = value;}
		}
			
		protected override Java.Lang.Object RawAdapter {
			get {
				return mAdapter;
			}
			set {
				if (mAdapter != null && mDataSetObserver != null) {
					mAdapter.UnregisterDataSetObserver (mDataSetObserver);
					mDataSetObserver = null;
				}


				mAdapter = value.JavaCast<global::Android.Widget.BaseAdapter>();

				if (mAdapter != null && mDataSetObserver == null) {
					mDataSetObserver = new AdapterDataSetObserver (this);
					mAdapter.RegisterDataSetObserver (mDataSetObserver);
				}
			}
		}

		public void setFlingListener(IOnFlingListener onFlingListener) {
			this.mFlingListener = onFlingListener;
		}

		public void setOnItemClickListener(IOnItemClickListener onItemClickListener) {
			this.mOnItemClickListener = onItemClickListener;
		}

		public override LayoutParams GenerateLayoutParams (Android.Util.IAttributeSet attrs)
		{
			return new FrameLayout.LayoutParams (Context, attrs);
		}

		private class AdapterDataSetObserver : DataSetObserver 
		{
			SwipeFlingAdapterView swipeFlingAdapterView;

			public AdapterDataSetObserver(SwipeFlingAdapterView swipeFlingAdapterView) 
			{
				this.swipeFlingAdapterView = swipeFlingAdapterView;
			}

			public override void OnChanged ()
			{
				if (swipeFlingAdapterView != null)
					swipeFlingAdapterView.RequestLayout ();
			}	

			public override void OnInvalidated ()
			{
				if (swipeFlingAdapterView != null)
					swipeFlingAdapterView.RequestLayout ();
			}
		}

		public interface IOnItemClickListener {
			void onItemClicked(int itemPosition, Object dataObject);
		}

		public interface IOnFlingListener {
			void removeFirstObjectInAdapter();
			void onLeftCardExit(Object dataObject);
			void onRightCardExit(Object dataObject);
			void onTopCardExit(Object dataObject);
			void onBottomCardExit(Object dataObject);
			void onAdapterAboutToEmpty(int itemsInAdapter);
			void onScroll(float scrollProgressPercentX, float scrollProgressPercentY);
		}
	}
}

