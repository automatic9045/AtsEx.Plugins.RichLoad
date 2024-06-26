using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using SlimDX.Direct3D9;

using BveTypes.ClassWrappers;
using FastMember;
using ObjectiveHarmonyPatch;
using TypeWrapping;

using AtsEx.PluginHost;
using AtsEx.PluginHost.Plugins;
using AtsEx.PluginHost.Plugins.Extensions;

using Automatic9045.AtsEx.RichLoad.Data;

namespace Automatic9045.AtsEx.RichLoad
{
    [Plugin(PluginType.Extension, "1.0.40401.1")]
    public class ExtensionMain : AssemblyPluginBase, IExtension
    {
        private readonly HarmonyPatch RenderPatch;
        private readonly HarmonyPatch SetProgressPatch;
        private readonly Stopwatch FadeStopwatch = new Stopwatch();

        private RichLoadConfig Config = null;
        private LoadingAnimation LoadingAnimation = null;

        private double FadeProgress;

        public ExtensionMain(PluginBuilder builder) : base(builder)
        {
            if (App.Instance.LaunchMode == LaunchMode.Ats)
            {
                throw new BveFileLoadException($"{nameof(RichLoad)} は {App.Instance.ProductShortName} {LaunchMode.Ats.GetTypeString()} では動作しません。", Name);
            }

            ClassMemberSet direct3DProviderMembers = BveHacker.BveTypes.GetClassInfoOf<Direct3DProvider>();
            FastMethod renderMethod = direct3DProviderMembers.GetSourceMethodOf(nameof(Direct3DProvider.Render));
            RenderPatch = HarmonyPatch.Patch(nameof(RichLoad), renderMethod.Source, PatchType.Prefix);
            RenderPatch.Invoked += (sender, e) =>
            {
                if (LoadingAnimation is null) return PatchInvokationResult.DoNothing(e);

                Direct3DProvider direct3DProvider = Direct3DProvider.FromSource(e.Instance);
                Device device = direct3DProvider.Device;
                MainForm mainForm = MainForm.FromSource(e.Args[0]);

                if (device is null) return PatchInvokationResult.DoNothing(e);
                if (direct3DProvider.HasDeviceLost) return PatchInvokationResult.DoNothing(e);

                if (mainForm.CurrentScenario is null)
                {
                    FadeStopwatch.Reset();
                    FadeProgress = 1;
                    LoadingAnimation.Render(255);
                }
                else
                {
                    if (FadeProgress == 0)
                    {
                        FadeStopwatch.Reset();
                        return PatchInvokationResult.DoNothing(e);
                    }

                    FadeStopwatch.Start();
                    FadeProgress = Math.Max(0, 1 - 0.8 * FadeStopwatch.Elapsed.TotalSeconds);

                    LoadingAnimation.Render((int)(255 * FadeProgress), () =>
                    {
                        mainForm.CurrentScenario.Draw(direct3DProvider);
                        mainForm.AssistantDrawer.Draw();
                    });
                }

                return new PatchInvokationResult(SkipModes.SkipOriginal);
            };

            ClassMemberSet loadingProgressFormMembers = BveHacker.BveTypes.GetClassInfoOf<LoadingProgressForm>();
            FastMethod setProgressMethod = loadingProgressFormMembers.GetSourceMethodOf(nameof(LoadingProgressForm.SetProgress));
            SetProgressPatch = HarmonyPatch.Patch(nameof(RichLoad), setProgressMethod.Source, PatchType.Postfix);
            SetProgressPatch.Invoked += (sender, e) =>
            {
                if (LoadingAnimation is null) return PatchInvokationResult.DoNothing(e);

                double progress = BveHacker.LoadingProgressForm.ProgressBar.Value;
                LoadingAnimation.Progress = (float)progress;

                BveHacker.MainFormSource.Invalidate();

                return PatchInvokationResult.DoNothing(e);
            };

            BveHacker.ScenarioOpened += OnScenarioOpened;
            BveHacker.ScenarioClosed += OnScenarioClosed;
            BveHacker.LoadingProgressFormSource.VisibleChanged += OnLoadingProgressFormVisibleChanged;
        }

        public override void Dispose()
        {
            LoadingAnimation?.Dispose();
            LoadingAnimation = null;

            RenderPatch.Dispose();
            SetProgressPatch.Dispose();

            BveHacker.ScenarioOpened -= OnScenarioOpened;
            BveHacker.ScenarioClosed -= OnScenarioClosed;
            //BveHacker.LoadingProgressFormSource.VisibleChanged -= OnLoadingProgressFormShown;
        }

        private void OnScenarioOpened(ScenarioOpenedEventArgs e)
        {
            LoadingAnimation?.Dispose();
            LoadingAnimation = null;

            string configDirectory = Path.Combine(e.ScenarioInfo.DirectoryName, $"AtsEx.{nameof(RichLoad)}");
            string configPath = Path.Combine(configDirectory, Path.GetFileNameWithoutExtension(e.ScenarioInfo.FileName) + ".xml");
            if (!File.Exists(configPath)) return;

            Config = RichLoadConfig.Deserialize(configPath, true);

            string texturePath = Path.Combine(configDirectory, Config.StaticImage.Path);
            LoadingAnimation = LoadingAnimation.Create(texturePath, new SizeF(Config.StaticImage.Width, Config.StaticImage.Height));

            BveHacker.MainFormSource.Invalidate();
        }

        private void OnScenarioClosed(EventArgs e)
        {
            LoadingAnimation?.Dispose();
            LoadingAnimation = null;
        }

        private void OnLoadingProgressFormVisibleChanged(object sender, EventArgs e)
        {
            if (LoadingAnimation is null) return;
            if (!BveHacker.LoadingProgressFormSource.Visible) return;

            BveHacker.LoadingProgressFormSource.DialogResult = DialogResult.Ignore;
            BveHacker.LoadingProgressFormSource.Hide();
        }

        public override TickResult Tick(TimeSpan elapsed)
        {
            return new ExtensionTickResult();
        }
    }
}
