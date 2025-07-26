using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace CraftableCartography.Items.Compass
{
    internal class HudCompassNeedleRenderer : IRenderer
    {
        ICoreClientAPI api;

        private float _heading;

        public float heading
        {
            get
            {
                return _heading;
            }
            set
            {
                _heading = value;

                compassAngle = 360 - heading;
            }
        }

        private float compassAngle;

        private MeshData needleMeshData;
        private MeshRef needleMeshRef;

        public double RenderOrder
        {
            get
            {
                return 1;
            }
        }

        public int RenderRange 
        {
            get
            {
                return 10;
            }
        }

        public HudCompassNeedleRenderer(ICoreClientAPI api)
        {
            this.api = api;

            api.Event.RegisterRenderer(this, EnumRenderStage.Ortho);

            needleMeshData = new(3, 4, false, false, true, false);

            needleMeshData.AddVertexSkipTex(-0.2f, 0, 0);
            needleMeshData.AddVertexSkipTex(0.2f, 0, 0);
            needleMeshData.AddVertexSkipTex(0, -1.5f, 0);

            needleMeshData.AddIndices(new int[] { 0, 1, 2, 0 });

            if (needleMeshRef is null) needleMeshRef = api.Render.UploadMesh(needleMeshData);
            else api.Render.UpdateMesh(needleMeshRef, needleMeshData);
        }

        public void Dispose()
        {
            if (needleMeshRef is not null)
            {
                api.Render.DeleteMesh(needleMeshRef);
                needleMeshRef = null;
            }
        }

        public void OnRenderFrame(float deltaTime, EnumRenderStage stage)
        {
            if (needleMeshRef is null) return;
            
            IShaderProgram shader = api.Render.CurrentActiveShader;

            shader.Uniform("rgbaIn", new Vec4f(1,1,1,1));
            shader.Uniform("extraGlow", 0);
            shader.Uniform("applyColor", 0);
            shader.Uniform("tex2d", 0);
            shader.Uniform("noTexture", 1.0F);
            shader.UniformMatrix("projectionMatrix", api.Render.CurrentProjectionMatrix);

            api.Render.GlPushMatrix();
            api.Render.GlTranslate(api.Render.FrameWidth / 2, api.Render.FrameHeight / 2, 0);
            api.Render.GlScale(36, 36, 0);
            api.Render.GlRotate(compassAngle, 0, 0, 1);
            shader.UniformMatrix("modelViewMatrix", api.Render.CurrentModelviewMatrix);
            api.Render.GlPopMatrix();

            api.Render.RenderMesh(needleMeshRef);
        }
    }
}
