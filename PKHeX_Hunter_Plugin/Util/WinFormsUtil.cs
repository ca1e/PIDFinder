using PKHeX.Core;
using System;
using System.Windows.Forms;

namespace PKHeX_Hunter_Plugin
{
    public static class WinFormsUtil
    {
        internal static DialogResult Alert(params string[] lines) => Alert(true, lines);

        internal static DialogResult Alert(bool sound, params string[] lines)
        {
            if (sound)
                System.Media.SystemSounds.Asterisk.Play();
            string msg = string.Join(Environment.NewLine + Environment.NewLine, lines);
            return MessageBox.Show(msg, "Alert", MessageBoxButtons.OK, sound ? MessageBoxIcon.Information : MessageBoxIcon.None);
        }

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