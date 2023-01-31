using System;
using UnityEngine;


public class TexturePainting : MonoBehaviour
{
    public Renderer mainTextureRenderer;
    public Renderer brushTextureRenderer;
    
    private Texture2D mainTexture;
    private Texture2D brushTexture;

    private int mainTextureColliderID;
    private int brushTextureColliderID;
    
    private Vector2Int brushOffset;
    
    private bool Erase;

    public int width = 400;
    public int height = 400;
    public int brushWidth = 10;
    public int brushHeight = 10;

    private void Start()
    {
        InitializeTexturesAndParams();
        
        //Set default offset to centre the brush
        brushOffset = new Vector2Int(-brushWidth / 2, -brushHeight / 2);
    }

    private void Update()
    {
        Erase = Input.GetMouseButton(1);
        if (Input.GetMouseButton(0) || Erase)
        {
            Draw();
        }

    }

    public void Draw()
    {
        int hitColliderID;
        Vector2 pointerUVCoordinates = GetPointerUVCoordinates(out hitColliderID);

        if (hitColliderID == brushTextureColliderID)
        {
            brushTexture.SetPixel((int)(brushWidth * pointerUVCoordinates.x), (int)(brushHeight * pointerUVCoordinates.y), Color.cyan);
            brushTexture.Apply();
        }
        else if (hitColliderID == mainTextureColliderID)
        {
            int textureCoordsX = (int)(width * pointerUVCoordinates.x);
            int textureCoordsY = (int)(height * pointerUVCoordinates.y);
            ApplyBrush(mainTexture, brushTexture, new Vector2Int(textureCoordsX, textureCoordsY), Erase, brushOffset);
            mainTexture.Apply();
        }
    }
    private void InitializeTexturesAndParams()
    {
        mainTextureColliderID = mainTextureRenderer.GetComponent<MeshCollider>().GetInstanceID();
        brushTextureColliderID = brushTextureRenderer.GetComponent<MeshCollider>().GetInstanceID();
        mainTexture = GetBlankTexture(width, height);
        brushTexture = GetBlankTexture(brushWidth, brushHeight);
        mainTextureRenderer.material.mainTexture = mainTexture;
        brushTextureRenderer.material.mainTexture = brushTexture;
    }
    private static void FillTexture(Texture2D texture, Color color)
    {
        Color[] colorArray = new Color[texture.width * texture.height];
        for (int i = 0; i < colorArray.Length; i++)
        {
            colorArray[i] = color;
        }
        texture.SetPixels(colorArray);
    }
    private static Texture2D GetBlankTexture(int _width, int _height)
    {
        Texture2D texture2D = new Texture2D(_width, _height);
        texture2D.wrapMode = TextureWrapMode.Clamp;
        texture2D.filterMode = FilterMode.Point;
        Color[] blankColors = new Color[_width*_height];
        for (int i = 0; i < blankColors.Length; i++)
        {
            blankColors[i] = Color.black;
        }
        texture2D.SetPixels(blankColors);
        texture2D.Apply();
        return texture2D;
    }
    /// <summary>
    /// <para> Adds or subtracts source texture from target texture at given coordinates without calling Apply()</para>
    /// </summary>
    /// <param name="target">Texture to edit</param>
    /// <param name="source">Must be smaller than target texture</param>
    /// <param name="inputCoords2D"></param>
    /// <param name="erase">Whether to add or subtract source texture from target texture</param>
    /// <param name="offset">Coordinate offset to add to inputCoords2D</param>
    private static void ApplyBrush(Texture2D target, Texture2D source, Vector2Int inputCoords2D, bool erase = false, Vector2Int offset = default)
    {
        if (source.width > target.width || source.height > target.height)
        {
            throw new ArgumentException("Source texture is bigger than target texture");
        }

        if (inputCoords2D.x > target.width || inputCoords2D.y > target.height)
        {
            throw new ArgumentException("Input coordinates are out of bounds of target texture");
        }

        Vector2Int coords2D = inputCoords2D + offset;
        Vector2Int appliedBrushWidthHeight = new Vector2Int(source.width, source.height);

        //Truncate brush's width and height to remain inbounds of target texture
        if (coords2D.x < 0)
        {
            appliedBrushWidthHeight.x -= 0 - coords2D.x;
        }
        
        if (coords2D.y < 0)
        {
            appliedBrushWidthHeight.y -= 0 - coords2D.y;
        }
        
        if (coords2D.x > target.width - source.width)
        {
            appliedBrushWidthHeight.x = target.width - coords2D.x;
        }

        if (coords2D.y > target.height - source.height)
        {
            appliedBrushWidthHeight.y = target.height - coords2D.y;
        }
        
        Color[] sourceTextureBlockColorArray = source.GetPixels((coords2D.x < target.width/2) ?source.width - appliedBrushWidthHeight.x : 0, 
                                                                (coords2D.y < target.height/2)? source.height - appliedBrushWidthHeight.y : 0, 
                                                                        appliedBrushWidthHeight.x, appliedBrushWidthHeight.y);

        coords2D.x = Math.Clamp(coords2D.x, 0, target.width);
        coords2D.y = Math.Clamp(coords2D.y, 0, target.height);
        
        Color[] targetTextureBlockColorArray = target.GetPixels(coords2D.x, coords2D.y, appliedBrushWidthHeight.x, appliedBrushWidthHeight.y);
        
        for (int i = 0; i < targetTextureBlockColorArray.Length; i++)
        {
            if (erase)
            {
                targetTextureBlockColorArray[i] -= sourceTextureBlockColorArray[i];
            }
            else
            {
                targetTextureBlockColorArray[i] += sourceTextureBlockColorArray[i];
            }
        }
        target.SetPixels(coords2D.x, coords2D.y, appliedBrushWidthHeight.x, appliedBrushWidthHeight.y, targetTextureBlockColorArray);
    }
    private static Vector2 GetPointerUVCoordinates(out int colliderID)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100))
        {
            colliderID = hit.colliderInstanceID;
            return hit.textureCoord;
        }
        
        colliderID = 0;
        return Vector2.zero;
    }
    
    public void ResetBrushTexture()
    {
        FillTexture(brushTexture, Color.black);
        brushTexture.Apply();
    }

    public void ResetMainTexture()
    {
        FillTexture(mainTexture, Color.black);
        mainTexture.Apply();
    }
}
