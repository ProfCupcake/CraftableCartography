using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

        private MeshData[] meshDatas;
        private MeshRef[] meshRefs;

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

            PrepareMesh();
        }

        void PrepareMesh()
        {
            /*
            needleMeshData = new(3, 4, false, false, true, false);

            needleMeshData.AddVertexSkipTex(-0.1f, 0, 0);
            needleMeshData.AddVertexSkipTex(0.1f, 0, 0);
            needleMeshData.AddVertexSkipTex(0, -1.5f, 0);

            needleMeshData.AddIndices(new int[] { 0, 1, 2, 0 });
            */

            meshDatas = new MeshData[37];

            MeshData circleMeshData = new(720, 721*6, false, false, true, false);

            float circleWidth = 1.1f;

            for (int i = 0; i < 360; i++)
            {
                double angle = i * (Math.PI / 180);
                
                float x = (float)Math.Sin(angle) * 2;
                float y = (float)-Math.Cos(angle) * 2;

                circleMeshData.AddVertexSkipTex(x, y, 0);
                circleMeshData.AddVertexSkipTex(x * circleWidth, y * circleWidth, 0);

                if (i > 0)
                {
                    circleMeshData.AddIndices(new int[] { (i * 2) - 2, (i * 2) - 1, (i * 2) + 0 });
                    circleMeshData.AddIndices(new int[] { (i * 2) + 0, (i * 2) - 1, (i * 2) + 1 });
                }
            }

            circleMeshData.AddIndices(new int[] { 718, 719, 0 });
            circleMeshData.AddIndices(new int[] { 0, 719, 1 });

            meshDatas[0] = circleMeshData;

            for (float i = 0; i < 359; i += 22.5f)
            {
                float baseWidth;

                float baseRadius;

                if (i == 0)
                {
                    baseRadius = 0.25f;
                    baseWidth = 0.4f;
                } else if (i % 90 == 0)
                {
                    baseRadius = 0.5f;
                    baseWidth = 0.2f;
                } else if (i % 45 == 0)
                {
                    baseRadius = 1f;
                    baseWidth = 0.1f;
                }
                else
                {
                    baseRadius = 1.5f;
                    baseWidth = 0.1f;
                }

                float tipRadius = 2f;

                Vec3f p1 = new(baseWidth * -0.5f, 0, baseRadius);
                Vec3f p2 = new(baseWidth * 0.5f, 0, baseRadius);
                Vec3f p3 = new(0, 0, tipRadius);

                p1 = p1.RotatedCopy(i + 180);
                p2 = p2.RotatedCopy(i + 180);
                p3 = p3.RotatedCopy(i + 180);

                MeshData lineMesh = new(3, 3, false, false, true, false);

                lineMesh.AddVertexSkipTex(p1.X, p1.Z, 0);
                lineMesh.AddVertexSkipTex(p2.X, p2.Z, 0);
                lineMesh.AddVertexSkipTex(p3.X, p3.Z, 0);

                lineMesh.AddIndices(new int[] { 0, 1, 2 });

                meshDatas[(int)((i/22.5) + 1)] = lineMesh;
            }

            if (meshRefs is null) meshRefs = new MeshRef[meshDatas.Length];

            for (int i = 0; i < meshDatas.Length; i++)
            {
                if (meshDatas[i] is not null)
                {
                    if (meshRefs[i] is null) meshRefs[i] = api.Render.UploadMesh(meshDatas[i]);
                    else api.Render.UpdateMesh(meshRefs[i], meshDatas[i]);
                }
            }
        }

        public void Dispose()
        {
            if (meshRefs is not null)
            {
                foreach (MeshRef meshRef in meshRefs)
                {
                    api.Render.DeleteMesh(meshRef);
                }
                meshRefs = null;
            }
        }

        public void OnRenderFrame(float deltaTime, EnumRenderStage stage)
        {
            if (meshRefs is null) return;
            
            IShaderProgram shader = api.Render.CurrentActiveShader;

            shader.Uniform("rgbaIn", new Vec4f(1,1,1,1));
            shader.Uniform("extraGlow", 0);
            shader.Uniform("applyColor", 0);
            shader.Uniform("tex2d", 0);
            shader.Uniform("noTexture", 1.0F);
            shader.UniformMatrix("projectionMatrix", api.Render.CurrentProjectionMatrix);

            api.Render.GlPushMatrix();
            api.Render.GlTranslate(
                api.Render.FrameWidth / 2,
                api.Render.FrameHeight * 0.65f, 
                0);
            api.Render.GlScale(64, 64, 0);
            api.Render.GlRotate(compassAngle, 0, 0, 1);
            shader.UniformMatrix("modelViewMatrix", api.Render.CurrentModelviewMatrix);
            api.Render.GlPopMatrix();

            foreach (MeshRef meshRef in meshRefs)
            {
                if (meshRef is not null) api.Render.RenderMesh(meshRef);
            }
        }
    }
}
