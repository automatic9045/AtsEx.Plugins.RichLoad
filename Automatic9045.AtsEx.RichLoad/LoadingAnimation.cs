﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using SlimDX;
using SlimDX.Direct3D9;

using BveTypes.ClassWrappers;

using Automatic9045.AtsEx.DXRendering;

namespace Automatic9045.AtsEx.RichLoad
{
    internal class LoadingAnimation : IDisposable
    {
        private readonly Model ImageModel;
        private readonly SizeF ImageSize;

        private readonly ShapeDrawer ShapeDrawer;

        public float Progress { get; set; } = 0;

        private LoadingAnimation(Model imageModel, SizeF imageSize)
        {
            ImageModel = imageModel;
            ImageSize = imageSize;

            ShapeDrawer = new ShapeDrawer(Direct3DProvider.Instance.Device);
        }

        public static LoadingAnimation Create(string texturePath, SizeF size)
        {
            RectangleF rectangle = new RectangleF(-size.Width / 2, size.Height / 2, size.Width, -size.Height);
            Model imageModel = Model.CreateRectangleWithTexture(rectangle, 0, 0, texturePath);
            return new LoadingAnimation(imageModel, size);
        }

        public void Dispose()
        {
            ImageModel.Dispose();
        }

        public void Render()
        {
            Direct3DProvider direct3DProvider = Direct3DProvider.Instance;
            Device device = direct3DProvider.Device;

            direct3DProvider.InitializeStates();

            device.BeginScene();
            {
                device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1, 0);

                device.SetSamplerState(0, SamplerState.AddressU, TextureAddress.Clamp);
                device.SetSamplerState(0, SamplerState.AddressV, TextureAddress.Clamp);

                device.SetRenderState(RenderState.Ambient, Color.White.ToArgb());
                device.EnableLight(0, false);

                float width = direct3DProvider.PresentParameters.BackBufferWidth;
                float height = direct3DProvider.PresentParameters.BackBufferHeight;
                float zoom = Math.Min(width / ImageSize.Width, height / ImageSize.Height);

                device.SetTransform(TransformState.World, Matrix.Scaling(zoom, zoom, 1));
                device.SetTransform(TransformState.View, Matrix.Identity);
                device.SetTransform(TransformState.Projection, Matrix.OrthoOffCenterLH(-width / 2, width / 2, -height / 2, height / 2, 0, 1));

                ImageModel.Draw(direct3DProvider, false);
                ShapeDrawer.FillRectangle(Color.White, 0, height - 10, width * Progress / 101, 10);

            }
            device.EndScene();

            try
            {
                device.Present();
            }
            catch (Direct3D9Exception ex)
            {
                if (ex.ResultCode == ResultCode.DeviceLost)
                {
                    direct3DProvider.HasDeviceLost = true;
                }
            }
        }
    }
}
