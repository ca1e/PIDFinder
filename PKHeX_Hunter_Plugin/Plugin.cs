using PKHeX.Core;
using System;
using System.Windows.Forms;
using System.Diagnostics;

namespace PKHeX_Hunter_Plugin
{
    public class Plugin : IPlugin
    {
        private const string ParentMenuParent = "Menu_Tools";

        public string Name => "猎游者";

        public int Priority => 1;

        public ISaveFileProvider SaveFileEditor { get; private set; } = null!;
        protected IPKMView PKMEditor { get; private set; } = null!;

        private ToolStripMenuItem Hunter;
        private Searcher searcher;

        public void Initialize(params object[] args)
        {
            Debug.WriteLine($"[{Name}] Loading...");
            SaveFileEditor = (ISaveFileProvider)Array.Find(args, z => z is ISaveFileProvider);
            PKMEditor = (IPKMView)Array.Find(args, z => z is IPKMView);
            var menu = (ToolStrip)Array.Find(args, z => z is ToolStrip);
            LoadMenuStrip(menu);

            NotifySaveLoaded();
        }

        private void LoadMenuStrip(ToolStrip menuStrip)
        {
            var items = menuStrip.Items;
            if (items.Find(ParentMenuParent, false)[0] is not ToolStripDropDownItem tools)
                return;
            // only visible for specified game version
            Hunter = new ToolStripMenuItem(Name) { Visible = true };
            Hunter.Click += (s, e) => Open();
            tools.DropDownItems.Add(Hunter);
        }

        private void Open()
        {
            if (searcher != null && !searcher.IsDisposed)
            {
                searcher.Activate();
                return;
            }
            searcher = new Searcher(SaveFileEditor, PKMEditor);
            searcher.Show();
        }

        public void NotifySaveLoaded()
        {
            Console.WriteLine($"{Name} was notified that a Save File was just loaded.");
            if (Hunter == null)
                return;
            var sav = SaveFileEditor.SAV;
            //Hunter.Visible = sav switch
            //{
            //     SAV8BS or SAV7 => true,
            //    _ => false,
            //};
        }

        public bool TryLoadFile(string filePath)
        {
            return false;
        }
    }
}

namespace System.Runtime.CompilerServices
{
    internal class IsExternalInit { }
}