using System.Linq;
using UnityEngine;

namespace KitchenModdedCosmeticsIntegration.Extensions
{
    internal static class Texture2DExtensions
    {
        public static Texture2D OverlayWith(this Texture2D baseTexture, Texture2D overlay)
        {
            RenderTexture renderTexture = new RenderTexture(baseTexture.width, baseTexture.height, 0, RenderTextureFormat.ARGB32);
            Texture2D tex = new Texture2D(baseTexture.width, baseTexture.height, TextureFormat.ARGB32, false);
            var old_rt = RenderTexture.active;
            RenderTexture.active = renderTexture;

            Graphics.Blit(baseTexture, renderTexture);
            Graphics.Blit(overlay, renderTexture);

            tex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            tex.Apply();

            RenderTexture.DestroyImmediate(renderTexture);

            RenderTexture.active = old_rt;
            return tex;
        }

        public static Texture2D Colorize(this Texture2D baseTexture, Color color)
        {
            Texture2D readableTexture = baseTexture.CopyToReadable();

            Color[] colors = readableTexture.GetPixels().Select(x =>
            {
                Color newColor = x * color;
                Main.LogInfo($"{newColor.r}, {newColor.g}, {newColor.b}, {newColor.a}");
                return x * color;
            }).ToArray();
            readableTexture.SetPixels(colors);
            readableTexture.Apply();
            return readableTexture;
        }

        public static Texture2D CopyToReadable(this Texture2D sourceTexture)
        {
            RenderTexture renderTexture = new RenderTexture(sourceTexture.width, sourceTexture.height, 32, RenderTextureFormat.ARGB32);
            RenderTexture oldActive = RenderTexture.active;

            RenderTexture.active = renderTexture;
            Graphics.Blit(sourceTexture, renderTexture);

            Texture2D newTexture = new Texture2D(renderTexture.width, renderTexture.height, sourceTexture.format, false);
            newTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);

            RenderTexture.Destroy(renderTexture);

            return newTexture;
        }
    }
}
