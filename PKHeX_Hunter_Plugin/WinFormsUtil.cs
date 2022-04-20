using PKHeX.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PKHeX_Hunter_Plugin
{
    public static class WinFormsUtil
    {
        internal static int GetIndex(ListControl cb)
        {
            return (int)(cb.SelectedValue ?? 0);
        }

        public static void InitializeBinding(this ListControl control)
        {
            control.DisplayMember = nameof(ComboItem.Text);
            control.ValueMember = nameof(ComboItem.Value);
        }
    }
}