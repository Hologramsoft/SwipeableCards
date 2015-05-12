/*****************************************************************
 * This project has been converted to C# from SwipeCards project
 * of Diolor which can be found in this link: 
 * https://github.com/Diolor/Swipecards
 * Also I added another functionality so you can swipe the card
 * all 4 sides instead of just 2 sides.
 * Developer: Erkin YILDIZ - Hologramsoft http://www.hologramsoft.com
 * ****************************************************************/

using System;
using Android.Views;
using Android.Views.Animations;

namespace SwipeableCards
{
	public enum ExitPlace 
	{
		Left,
		Right,
		Top,
		Bottom
	}

	public class FlingCardListener : Java.Lang.Object, View.IOnTouchListener, Android.Animation.Animator.IAnimatorListener
	{
		//If true cards can be swiped to 4 sides (bottom&top included)
		//else cards can be swiped to left and right only
		private const bool FOURSIDED = true; 

		private float objectX;
		private float objectY;
		private int objectH;
		private int objectW;
		private int parentWidth;
		private int parentHeight;
		private IFlingListener mFlingListener;
		private Object dataObject;
		private float halfWidth;
		private float halfHeight;
		private float BASE_ROTATION_DEGREES;

		private float aPosX;
		private float aPosY;
		private float aDownTouchX;
		private float aDownTouchY;
		private static int INVALID_POINTER_ID = -1;

		// The active pointer is the one currently moving our object.
		private int mActivePointerId = INVALID_POINTER_ID;
		private View frame = null;

		private const int TOUCH_ABOVE = 0;
		private const int TOUCH_BELOW = 1;
		private int touchPosition;
		private Object obj = new object();
		private bool isAnimationRunning = false;
		private float MAX_COS = (float) Math.Cos((Math.PI / 180) * 45);
		private float MAX_SIN = (float) Math.Sin((Math.PI / 180) * 45);

		public FlingCardListener(View frame, Object itemAtPosition, IFlingListener flingListener, float rotation_degrees = 15f) : base()
		{
			this.frame = frame;
			this.objectX = frame.GetX ();
			this.objectY = frame.GetY ();
			this.objectH = frame.Height;
			this.objectW = frame.Width;
			this.halfWidth = objectW / 2f;
			this.halfHeight = objectH / 2f;
			this.dataObject = itemAtPosition;
			this.parentWidth = ((ViewGroup) frame.Parent).Width;
			this.parentHeight = ((ViewGroup) frame.Parent).Height;
			this.BASE_ROTATION_DEGREES = rotation_degrees;
			this.mFlingListener = flingListener;
		}

		public bool OnTouch(View view, MotionEvent mEvent) 
		{
			switch (mEvent.Action & MotionEventActions.Mask) {
			case MotionEventActions.Down:
				{
					// Save the ID of this pointer
					mActivePointerId = mEvent.GetPointerId (0);
					float x = mEvent.GetX (mActivePointerId);
					float y = mEvent.GetY (mActivePointerId);

					// Remember where we started
					aDownTouchX = x;
					aDownTouchY = y;
					// to prevent an initial jump of the magnifier, aposX and aPosY must have the values
					// from the magnifier frame
					if (aPosX == 0) {
						aPosX = frame.GetX ();
					}
					if (aPosY == 0) {
						aPosY = frame.GetY ();
					}
					if (y < objectH / 2) {
						touchPosition = TOUCH_ABOVE;
					} else {
						touchPosition = TOUCH_BELOW;
					}
				}
				break;
			case MotionEventActions.Up:
				{
					mActivePointerId = INVALID_POINTER_ID;
					resetCardViewOnStack ();
				}
				break;
			case MotionEventActions.PointerDown:
				break;
			case MotionEventActions.PointerUp:
				{
					int pointerIndex = (int) (mEvent.Action & MotionEventActions.PointerIndexMask) >> (int) MotionEventActions.PointerIndexShift;
					int pointerID = mEvent.GetPointerId (pointerIndex);
					if (pointerID == mActivePointerId) {
						// This was our active pointer going up. Choose a new active pointer and adjust accordingly.
						int newPointerIndex = pointerIndex == 0 ? 1 : 0;
						mActivePointerId = mEvent.GetPointerId (newPointerIndex);
					}
				}
				break;
			case MotionEventActions.Move:
				{
					// Find the index of the active pointer and fetch its position
					int pointerIndexMove = mEvent.FindPointerIndex (mActivePointerId);
					float xMove = mEvent.GetX (pointerIndexMove);
					float yMove = mEvent.GetY (pointerIndexMove);

					//Calculate the distance moved
					float dx = xMove - aDownTouchX;
					float dy = yMove - aDownTouchY;

					// Move the frame
					aPosX += dx;
					aPosY += dy;

					// Calculate the rotation degrees
					float distobjectX = aPosX - objectX;
					float rotation = BASE_ROTATION_DEGREES * 2f * distobjectX / parentWidth;
					if (touchPosition == TOUCH_BELOW) {
						rotation = -rotation;
					}

					//in this area would be code for doing something with the view as the frame moves.
					frame.SetX (aPosX);
					frame.SetY (aPosY);
					frame.Rotation = rotation;
					mFlingListener.onScroll (getScrollProgressPercentX (), getScrollProgressPercentY ());
				}
				break;
			}

			return true;
		}

		private float getScrollProgressPercentX() {
			if (movedBeyondLeftBorder ()) {
				return -1f;
			} else if (movedBeyondRightBorder ()) {
				return 1f;
			} else {
				float zeroToOneValue = (aPosX + halfWidth - leftBorder ()) / (rightBorder () - leftBorder ());
				return zeroToOneValue * 2f -1f;
			}
		}

		private float getScrollProgressPercentY() {
			if (movedBeyondTopBorder ()) {
				return -1f;
			} else if (movedBeyondBottomBorder ()) {
				return 1f;
			} else {
				float zeroToOneValue = (aPosY + halfHeight - topBorder ()) / (bottomBorder () - topBorder ());
				return zeroToOneValue * 2f -1f;
			}
		}

		private bool resetCardViewOnStack() 
		{
			if (movedBeyondLeftBorder ()) {
				//Left Swipe
				if (FOURSIDED) 
				{
					// if it goes all 4 sides, and if it is both beyond left border and one of the vertical borders it needs to go back to original
					// position, since we don't know what the user intended exactly
					if (movedBeyondBottomBorder() || movedBeyondTopBorder () ) 
					{
						returnToOriginalPosition ();
						return false;
					}
				}
				var exitX = -objectW - getRotationWidthOffset ();
				onSelected (ExitPlace.Left, exitX, getExitPoint (-objectW), 100);
				mFlingListener.onScroll (-1.0f, 0);
			} else if (movedBeyondRightBorder ()) {
				//Right Swipe
				if (FOURSIDED) 
				{
					// if it goes all 4 sides, and if it is both beyond left border and one of the vertical borders it needs to go back to original
					// position, since we don't know what the user intended exactly
					if (movedBeyondBottomBorder() || movedBeyondTopBorder () ) 
					{
						returnToOriginalPosition ();
						return false;
					}
				}
				var exitX = parentWidth + getRotationWidthOffset ();
				onSelected (ExitPlace.Right, exitX, getExitPoint (parentWidth), 100);
				mFlingListener.onScroll (1.0f, 0);
			} else if (movedBeyondTopBorder ()) {
				if (!FOURSIDED) 
				{
					// Can't go there, its just 2 sided
					returnToOriginalPosition();
					return false;
				}
				var exitY = -objectH - getRotationHeightOffset ();
				onSelected (ExitPlace.Top, objectX, exitY, 100);
				mFlingListener.onScroll (0, -1.0f);
			} else if (movedBeyondBottomBorder ()) {
				if (!FOURSIDED) 
				{
					// Can't go there, its just 2 sided
					returnToOriginalPosition();
					return false;
				}
				var exitY = parentHeight + getRotationHeightOffset ();
				onSelected (ExitPlace.Bottom, objectX, exitY, 100);
				mFlingListener.onScroll (0, 1.0f);
			} else {
				returnToOriginalPosition ();
			}
			return false;
		}

		private void returnToOriginalPosition() {
			float abslMoveDistance = Math.Abs (aPosX - objectX);
			aPosX = 0;
			aPosY = 0;
			aDownTouchX = 0;
			aDownTouchY = 0;
			frame.Animate ().SetDuration (200).SetInterpolator (new OvershootInterpolator (1.5f)).X (objectX).Y (objectY).Rotation (0);
			mFlingListener.onScroll (0.0f, 0.0f);
			if (abslMoveDistance < 4.0) {
				mFlingListener.onClick (dataObject);
			}
		}

		private bool movedBeyondLeftBorder() {
			return aPosX + halfWidth < leftBorder ();
		}

		private bool movedBeyondRightBorder() {
			return aPosX + halfWidth > rightBorder ();
		}

		private bool movedBeyondTopBorder() {
			return aPosY + halfHeight < topBorder ();
		}

		private bool movedBeyondBottomBorder() {
			return aPosY + halfHeight > bottomBorder ();
		}

		public float leftBorder() {
			return parentWidth / 4f;
		}

		public float rightBorder() {
			return 3 * parentWidth / 4f;
		}

		public float topBorder() {
			return parentHeight / 4f;
		}

		public float bottomBorder() {
			return 3 * parentHeight / 4f;
		}

		ExitPlace exitPlace;
		public void onSelected(ExitPlace exitPlace, float exitX, float exitY, long duration) {
			this.exitPlace = exitPlace;
			isAnimationRunning = true;

			this.frame.Animate ().SetDuration (duration).SetInterpolator (new AccelerateInterpolator ()).X (exitX).Y (exitY).SetListener (this).Rotation (getExitRotation (true));
		}

		#region IAnimatorListener implementation
		public void OnAnimationCancel (Android.Animation.Animator animation)
		{
			
		}

		public void OnAnimationEnd (Android.Animation.Animator animation)
		{
			if (exitPlace == ExitPlace.Left) {
				mFlingListener.onCardExited ();
				mFlingListener.leftExit (dataObject);
			} else if (exitPlace == ExitPlace.Right) {
				mFlingListener.onCardExited ();
				mFlingListener.rightExit (dataObject);
			} else if (exitPlace == ExitPlace.Top) {
				mFlingListener.onCardExited ();
				mFlingListener.topExit (dataObject);
			} else if (exitPlace == ExitPlace.Bottom) {
				mFlingListener.onCardExited ();
				mFlingListener.bottomExit (dataObject);
			}
		}

		public void OnAnimationRepeat (Android.Animation.Animator animation)
		{
			
		}

		public void OnAnimationStart (Android.Animation.Animator animation)
		{
			
		}

		#endregion

		// Starts a default left exit animation.
		public void selectLeft() {
			if (!isAnimationRunning)
				onSelected (ExitPlace.Left, -objectW - getRotationWidthOffset (), objectY, 200);
		}

		// Starts a default right exit animation.
		public void selectRight() {
			if (!isAnimationRunning)
				onSelected (ExitPlace.Right, parentWidth + getRotationWidthOffset (), objectY, 200);
		}

		// Starts a default top exit animation.
		public void selectTop() {
			if (!isAnimationRunning)
				onSelected (ExitPlace.Top, objectX, -objectY - getRotationHeightOffset(), 200);
		}

		// Starts a default bottom exit animation.
		public void selectBottom() {
			if (!isAnimationRunning)
				onSelected (ExitPlace.Left, objectX, parentHeight + getRotationHeightOffset(), 200);
		}

		private float getExitPoint(int exitXPoint) {
			float[] x = new float[2];
			x [0] = objectX;
			x [1] = aPosX;

			float[] y = new float[2];
			y [0] = objectY;
			y [1] = aPosY;

			LinearRegression regression = new LinearRegression (x, y);

			return (float)regression.slope () * exitXPoint + (float)regression.intercept ();
		}

		private float getExitRotation(bool isLeft) {
			float rotation = BASE_ROTATION_DEGREES * 2f * (parentWidth - objectX) / parentWidth;
			if (touchPosition == TOUCH_BELOW) {
				rotation = -rotation;
			}
			return rotation;
		}

		private float getRotationWidthOffset() {
			return objectW/MAX_COS - objectW;
		}

		private float getRotationHeightOffset() {
			return objectH/MAX_SIN - objectH;
		}

		public void setRotationDegrees(float degrees) {
			this.BASE_ROTATION_DEGREES = degrees;
		}

		public interface IFlingListener {
			void onCardExited();
			void leftExit(Object dataObject);
			void rightExit(Object dataObject);
			void topExit(Object dataObject);
			void bottomExit(Object dataObject);
			void onClick(Object dataObject);
			void onScroll(float scrollProgressPercentX, float scrollProgressPercentY);
		}
	}


}

