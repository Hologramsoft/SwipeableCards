using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Collections.Generic;
using Android.Util;
using SwipeableCards;

namespace SwipeableCardsExample
{
	[Activity (Label = "HoloSwipeableCards", MainLauncher = true, Icon = "@drawable/icon")]
	public class MainActivity : Activity, SwipeFlingAdapterView.IOnFlingListener
	{
		private List<string> al;
		private ArrayAdapter<string> arrayAdapter;
		private int i;
		SwipeFlingAdapterView flingContainer;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

			al = new List<string> ();
			al.Add ("Man U - Chelsea");
			al.Add ("Juventus - Inter");
			al.Add ("Barcelona - Malaga");
			al.Add ("Real Madrid - Sevilla");
			al.Add ("Man C - Arsenal");

			arrayAdapter = new ArrayAdapter<string> (this, Resource.Layout.item, Resource.Id.helloText, al);

			flingContainer = FindViewById<SwipeFlingAdapterView> (Resource.Id.frame);
			flingContainer.Adapter = arrayAdapter;
			flingContainer.setFlingListener (this);
		}

		#region IOnFlingListener implementation

		public void removeFirstObjectInAdapter ()
		{
			Console.WriteLine ("Removed object!");
			arrayAdapter.Remove (arrayAdapter.GetItem(0));
			arrayAdapter.NotifyDataSetChanged ();
			//flingContainer.RequestLayout ();
			/*flingContainer.Visibility = ViewStates.Gone;
			flingContainer.Visibility = ViewStates.Visible;*/
		}

		public void onLeftCardExit (object dataObject)
		{
			makeToast (this, "Left");
		}

		public void onRightCardExit (object dataObject)
		{
			makeToast (this, "Right");
		}

		public void onTopCardExit (object dataObject)
		{
			makeToast (this, "Top");
		}

		public void onBottomCardExit (object dataObject)
		{
			makeToast (this, "Bottom");
		}

		public void onAdapterAboutToEmpty (int itemsInAdapter)
		{
			arrayAdapter.Add ("Fenerbahce - Galatasaray " + i);
			arrayAdapter.NotifyDataSetChanged ();
			Console.WriteLine ("notified");
			i++;
		}

		public void onScroll (float scrollProgressPercentX, float scrollProgressPercentY)
		{
			View view = flingContainer.SelectedView;
			view.FindViewById(Resource.Id.item_swipe_right_indicator).Alpha = (scrollProgressPercentX < 0 ? -scrollProgressPercentX : 0);
			view.FindViewById(Resource.Id.item_swipe_left_indicator).Alpha = (scrollProgressPercentX > 0 ? scrollProgressPercentX : 0);
			view.FindViewById(Resource.Id.item_swipe_bottom_indicator).Alpha = (scrollProgressPercentY < 0 ? -scrollProgressPercentY : 0);
			view.FindViewById(Resource.Id.item_swipe_top_indicator).Alpha = (scrollProgressPercentY > 0 ? scrollProgressPercentY : 0);
		}

		#endregion

		void makeToast (Context ctx, string s) {
			Toast.MakeText (ctx, s, ToastLength.Short).Show();
		}
	}
}


