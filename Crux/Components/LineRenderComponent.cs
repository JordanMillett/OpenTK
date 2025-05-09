using OpenTK.Graphics.OpenGL4;
using Crux.Graphics;
using Crux.Utilities.IO;

namespace Crux.Components;

public class LineRenderComponent : RenderComponent
{
    public Shader shader { get; set; } = null!;

    public Color4 Color = Color4.Red;

    int vao;
    
    public LineRenderComponent(GameObject gameObject): base(gameObject)
    {
        shader = AssetHandler.LoadPresetShader(AssetHandler.ShaderPresets.Outline);

        vao = GraphicsCache.GetLineVAO("LineBounds", Shapes.LineBounds);

        //vao = GraphicsCache.GetLineVAO("LineAnchor", Shapes.LineAnchor);
    }
    
    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine($"{ this.GetType().Name }");

        return sb.ToString();
    }
    
    public override Component Clone(GameObject gameObject)
    {
        LineRenderComponent clone = new LineRenderComponent(gameObject);

        return clone;
    }

    public override void Render()
    {
        //Per-Object Uniforms and Shared Uniforms
        shader.SetUniform("model", GameObject.Transform.ModelMatrix);

        shader.TextureHue = Color;
        
        shader.Bind();
        
        GL.BindVertexArray(vao);

        //GL.DrawArrays(PrimitiveType.Lines, 0, Shapes.LineAnchor.Count);
        GL.DrawArrays(PrimitiveType.Lines, 0, Shapes.LineBounds.Count);
        GraphicsCache.DrawCallsThisFrame++;

        GL.BindVertexArray(0);

        shader.Unbind();
    }
}
