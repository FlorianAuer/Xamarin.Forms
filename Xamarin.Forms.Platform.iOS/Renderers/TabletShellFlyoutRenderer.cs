﻿using System;
using System.ComponentModel;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public class TabletShellFlyoutRenderer : UISplitViewController, IShellFlyoutRenderer, IFlyoutBehaviorObserver
	{
		#region IShellFlyoutRenderer

		UIViewController IShellFlyoutRenderer.ViewController => this;

		UIView IShellFlyoutRenderer.View => View;

		void IShellFlyoutRenderer.AttachFlyout(IShellContext context, UIViewController content)
		{
			_context = context;
			_content = content;
			
			FlyoutContent = _context.CreateShellFlyoutContentRenderer();
			FlyoutContent.WillAppear += OnFlyoutContentWillAppear;
			FlyoutContent.WillDisappear += OnFlyoutContentWillDisappear;

			((IShellController)_context.Shell).AddFlyoutBehaviorObserver(this);

			ViewControllers = new UIViewController[]
			{
				FlyoutContent.ViewController,
				content
			};
		}

		void IShellFlyoutRenderer.CloseFlyout()
		{
			if (_isPresented && _flyoutBehavior == FlyoutBehavior.Flyout)
				ToggleFlyout();
		}

		#endregion

		#region IFlyoutBehaviorObserver

		void IFlyoutBehaviorObserver.OnFlyoutBehaviorChanged(FlyoutBehavior behavior)
		{
			switch (behavior)
			{
				case FlyoutBehavior.Disabled:
					PreferredDisplayMode = UISplitViewControllerDisplayMode.PrimaryHidden;
					PresentsWithGesture = false;
					break;
				case FlyoutBehavior.Flyout:
					PreferredDisplayMode = UISplitViewControllerDisplayMode.PrimaryHidden;
					PresentsWithGesture = true;
					_isPresented = false;
					_context.Shell.SetValueFromRenderer(Shell.FlyoutIsPresentedProperty, false);
					break;
				case FlyoutBehavior.Locked:
					PreferredDisplayMode = UISplitViewControllerDisplayMode.AllVisible;
					break;
			}

			_flyoutBehavior = behavior;
		}

		#endregion

		private IShellContext _context;
		private UIViewController _content;
		private bool _isPresented;
		private FlyoutBehavior _flyoutBehavior;
		private bool _disposed;

		private IShellFlyoutContentRenderer FlyoutContent { get; set; }

		private void OnFlyoutContentWillAppear(object sender, EventArgs e)
		{
			_isPresented = true;
			_context.Shell.SetValueFromRenderer(Shell.FlyoutIsPresentedProperty, true);
		}

		private void OnFlyoutContentWillDisappear(object sender, EventArgs e)
		{
			_isPresented = false;
			_context.Shell.SetValueFromRenderer(Shell.FlyoutIsPresentedProperty, false);
		}

		private void OnShellPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Shell.FlyoutIsPresentedProperty.PropertyName)
			{
				OnFlyoutIsPresentedChanged();
			}
		}

		protected virtual void ToggleFlyout()
		{
			var barButtonItem = DisplayModeButtonItem;
			UIApplication.SharedApplication.SendAction(barButtonItem.Action, barButtonItem.Target, null, null);
		}

		protected virtual void OnFlyoutIsPresentedChanged()
		{
			var presented = _context.Shell.FlyoutIsPresented;

			if (_isPresented != presented)
				ToggleFlyout();
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			_context.Shell.PropertyChanged += OnShellPropertyChanged;
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing)
			{
				if (!_disposed)
				{
					_disposed = true;
					FlyoutContent.WillAppear -= OnFlyoutContentWillAppear;
					FlyoutContent.WillDisappear -= OnFlyoutContentWillDisappear;
					((IShellController)_context.Shell).RemoveFlyoutBehaviorObserver(this);
				}

				FlyoutContent = null;
				_content = null;
				_context = null;
			}
		}
	}
}