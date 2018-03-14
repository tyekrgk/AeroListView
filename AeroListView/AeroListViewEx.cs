using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace AeroListView
{
	public class AeroListViewEx : ListView
	{
		internal static class NativeMethods
		{
			[DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
			public static extern int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string pszSubIdList);
		}

		public class FastContextMenuItem : ToolStripMenuItem
		{
			private readonly Action _clickAction;

			private bool _enableClickAction = true;

			public Action ClickAction
			{
				get
				{
					return this._clickAction;
				}
			}

			public bool EnableClickAction
			{
				get
				{
					return this._enableClickAction;
				}
				set
				{
					this._enableClickAction = value;
				}
			}

			internal FastContextMenuItem(string text, Action action)
			{
				base.Text = text;
				this._clickAction = action;
			}

			protected override void OnClick(EventArgs e)
			{
				base.OnClick(e);
				if (this._enableClickAction && this._clickAction != null)
				{
					this._clickAction();
				}
			}
		}

		private readonly ContextMenuStrip _fastContextMenu;

		private bool _enableFastContextMenu = true;

		private readonly Dictionary<string, List<ListViewItem>> _keyItems;

		[Browsable(true)]
		[DefaultValue(true)]
		public bool EnableFastContextMenu
		{
			get
			{
				return this._enableFastContextMenu;
			}
			set
			{
				this._enableFastContextMenu = value;
			}
		}

		public AeroListViewEx()
		{
			base.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
			this._fastContextMenu = new ContextMenuStrip();
			this._keyItems = new Dictionary<string, List<ListViewItem>>();
			base.BorderStyle = BorderStyle.None;
			base.FullRowSelect = true;
			base.ShowItemToolTips = true;
			base.View = View.Details;
		}

		public FastContextMenuItem AddFastContextMenuItem(string text, Action action)
		{
			FastContextMenuItem fastContextMenuItem = null;
			if (!string.IsNullOrWhiteSpace(text) && action != null)
			{
				fastContextMenuItem = new FastContextMenuItem(text, action);
				this._fastContextMenu.Items.Add(fastContextMenuItem);
			}
			return fastContextMenuItem;
		}

		public void RemoveFastContextMenuItem(FastContextMenuItem fastContextMenuItem)
		{
			if (this._fastContextMenu.Items.Contains(fastContextMenuItem))
			{
				this._fastContextMenu.Items.Remove(fastContextMenuItem);
			}
		}

		public void RemoveAllFastContextMenuItems()
		{
			if (this._fastContextMenu.Items.Count > 0)
			{
				foreach (FastContextMenuItem item in this._fastContextMenu.Items)
				{
					this.RemoveFastContextMenuItem(item);
				}
			}
		}

		public void AddKeyItem(string key, ListViewItem item)
		{
			if (this._keyItems.ContainsKey(key))
			{
				List<ListViewItem> list = this._keyItems[key];
				list.Add(item);
				this._keyItems[key] = list;
			}
			else
			{
				List<ListViewItem> list2 = new List<ListViewItem>();
				list2.Add(item);
				this._keyItems.Add(key, list2);
			}
			base.Items.Add(item);
		}

		public void AddKeyItems(string key, IEnumerable<ListViewItem> items)
		{
			foreach (ListViewItem item in items)
			{
				this.AddKeyItem(key, item);
			}
		}

		public IEnumerable<ListViewItem> GetKeyItems(string key)
		{
			List<ListViewItem> list = new List<ListViewItem>();
			if (this._keyItems.ContainsKey(key))
			{
				List<ListViewItem> collection = this._keyItems[key];
				list.AddRange(collection);
			}
			return list;
		}

		public bool RemoveKeyItems(string key, bool removeFromListView = true)
		{
			bool flag = this._keyItems.ContainsKey(key);
			if (flag)
			{
				if (removeFromListView)
				{
					foreach (KeyValuePair<string, List<ListViewItem>> keyItem in this._keyItems)
					{
						if (keyItem.Key == key)
						{
							foreach (ListViewItem item in keyItem.Value)
							{
								base.Items.Remove(item);
							}
						}
					}
				}
				this._keyItems.Remove(key);
			}
			return flag;
		}

		public void RemoveAllKeyItems(bool removeFromListView = true)
		{
			if (removeFromListView)
			{
				foreach (KeyValuePair<string, List<ListViewItem>> keyItem in this._keyItems)
				{
					foreach (ListViewItem item in keyItem.Value)
					{
						base.Items.Remove(item);
					}
				}
			}
			this._keyItems.Clear();
		}

		public IEnumerable<ListViewItem> Filter(string text, int column = 0, RegexOptions regexOptions = RegexOptions.IgnoreCase)
		{
			List<ListViewItem> result = null;
			if (!string.IsNullOrEmpty(text))
			{
				result = new List<ListViewItem>();
				{
					foreach (ListViewItem item in base.Items)
					{
						if (Regex.IsMatch(item.SubItems[column].Text, text, regexOptions))
						{
							result.Add(item);
						}
					}
					return result;
				}
			}
			return result;
		}

		public IEnumerable<ListViewItem> GetDuplicates(int column)
		{
			List<string> list = new List<string>();
			List<ListViewItem> list2 = new List<ListViewItem>();
			foreach (ListViewItem item in base.Items)
			{
				string text = item.SubItems[column].Text;
				if (!list.Contains(text))
				{
					list.Add(text);
				}
				else
				{
					list2.Add(item);
				}
			}
			return list2;
		}

		public void ReplaceItems(IEnumerable<ListViewItem> items)
		{
			base.Items.Clear();
			base.Items.AddRange(items.ToArray());
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			if (e.Button == MouseButtons.Right && this._enableFastContextMenu && this.ContextMenu == null)
			{
				this._fastContextMenu.Show(this, new Point(e.X, e.Y));
			}
		}

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);
			NativeMethods.SetWindowTheme(base.Handle, "explorer", null);
		}
	}
}
