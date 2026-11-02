using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

public static class PaketciArtBuilder
{
    private static Texture2D texture;
    private static Color[] pixels;
    private static int width, height;
    private static Color C(string hex) { ColorUtility.TryParseHtmlString(hex, out var c); return c; }
    private static void Begin(int w, int h) { width=w; height=h; texture=new Texture2D(w,h,TextureFormat.RGBA32,false); pixels=new Color[w*h]; }
    private static void Rect(float x,float y,float w,float h,Color c,float radius=0)
    {
        for(int py=Mathf.Max(0,(int)y);py<Mathf.Min(height,y+h);py++)for(int px=Mathf.Max(0,(int)x);px<Mathf.Min(width,x+w);px++)
        {
            float dx=Mathf.Max(x+radius-px,px-(x+w-radius)),dy=Mathf.Max(y+radius-py,py-(y+h-radius));
            float d=Mathf.Sqrt(Mathf.Max(dx,0)*Mathf.Max(dx,0)+Mathf.Max(dy,0)*Mathf.Max(dy,0));
            float a=radius<=0?1:Mathf.Clamp01(radius-d); Color col=c;col.a*=a;
            int index=(height-1-py)*width+px;pixels[index]=Color.Lerp(pixels[index],col,col.a);pixels[index].a=Mathf.Max(pixels[index].a,col.a);
        }
    }
    private static void Circle(float x,float y,float r,Color c) { for(int py=Mathf.Max(0,(int)(y-r));py<Mathf.Min(height,y+r);py++)for(int px=Mathf.Max(0,(int)(x-r));px<Mathf.Min(width,x+r);px++){float a=Mathf.Clamp01(r-Vector2.Distance(new Vector2(px,py),new Vector2(x,y)));if(a<=0)continue;int i=(height-1-py)*width+px;Color col=c;col.a*=a;pixels[i]=Color.Lerp(pixels[i],col,col.a);pixels[i].a=Mathf.Max(pixels[i].a,col.a);} }
    private static void Line(Vector2 a,Vector2 b,float r,Color c) { int steps=Mathf.Max(1,Mathf.CeilToInt(Vector2.Distance(a,b)*2));for(int i=0;i<=steps;i++){Vector2 p=Vector2.Lerp(a,b,(float)i/steps);Circle(p.x,p.y,r,c);} }
    private static Sprite Finish(string path,bool sliced=false)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)); texture.SetPixels(pixels);texture.Apply();File.WriteAllBytes(path,texture.EncodeToPNG());Object.DestroyImmediate(texture);AssetDatabase.ImportAsset(path);
        var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.filterMode=FilterMode.Bilinear;importer.spritePixelsPerUnit=100;if(sliced)importer.spriteBorder=new Vector4(24,24,24,24);importer.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
    private static Sprite Icon(int kind)
    {
        Begin(96,96);Color white=Color.white;
        if(kind==0){Circle(34,39,15,white);Circle(62,39,15,white);for(int y=39;y<76;y++)Rect(19+(y-39)*0.78f,y,58-(y-39)*1.56f,1,white);}
        else if(kind==1||kind==4){int points=kind==1?10:4;Vector2[] p=new Vector2[points];for(int i=0;i<points;i++){float angle=-Mathf.PI/2+i*Mathf.PI*2/points;float r=kind==1?(i%2==0?34:15):32;p[i]=new Vector2(48+Mathf.Cos(angle)*r,48+Mathf.Sin(angle)*r);}for(int i=0;i<points;i++)Line(p[i],p[(i+1)%points],4,white);}
        else if(kind==2){Circle(48,48,28,white);Circle(48,48,15,Color.clear);Rect(42,31,12,34,C("#FFFFFF"),6);}
        else if(kind==3){Circle(48,48,10,white);for(int i=0;i<5;i++){float a=i*Mathf.PI*2/5;Circle(48+Mathf.Cos(a)*22,48+Mathf.Sin(a)*22,12,white);}}
        else if(kind==5){Line(new Vector2(54,16),new Vector2(32,49),5,white);Line(new Vector2(32,49),new Vector2(62,49),5,white);Line(new Vector2(62,49),new Vector2(42,81),5,white);}
        else if(kind==6){Rect(16,28,64,50,white,8);Rect(41,20,14,62,white);Line(new Vector2(48,28),new Vector2(30,13),4,white);Line(new Vector2(48,28),new Vector2(67,13),4,white);}
        else if(kind==7){Circle(48,48,22,white);for(int i=0;i<8;i++){float a=i*Mathf.PI/4;Line(new Vector2(48+Mathf.Cos(a)*20,48+Mathf.Sin(a)*20),new Vector2(48+Mathf.Cos(a)*31,48+Mathf.Sin(a)*31),5,white);}Circle(48,48,8,C("#24364D"));}
        else if(kind==8){Rect(27,22,12,52,white,4);Rect(57,22,12,52,white,4);}
        else if(kind==9){Circle(48,48,32,white);Circle(48,48,24,C("#D69032"));Rect(44,31,8,34,white,3);}
        else { Line(new Vector2(23,49),new Vector2(41,66),5,white);Line(new Vector2(41,66),new Vector2(75,28),5,white); }
        return Finish("Assets/Art/UI/Icon"+kind+".png");
    }
    private static Sprite Mascot()
    {
        Begin(256,256);Color dark=C("#192A40");Rect(41,65,174,152,dark,30);Rect(37,54,178,151,C("#DF803D"),26);Rect(44,49,166,137,C("#FFB15C"),24);Rect(111,49,30,137,C("#FFE5A5"));Rect(53,64,55,10,C("#FFD58B"),5);Circle(85,115,9,dark);Circle(171,115,9,dark);Circle(88,112,3,Color.white);Circle(174,112,3,Color.white);Line(new Vector2(108,145),new Vector2(127,156),4,dark);Line(new Vector2(127,156),new Vector2(147,145),4,dark);Circle(69,135,9,C("#F29175"));Circle(187,135,9,C("#F29175"));Rect(27,158,27,18,C("#FFBD74"),8);Rect(204,151,27,18,C("#FFBD74"),8);Rect(62,199,44,15,dark,6);Rect(151,199,44,15,dark,6);return Finish("Assets/Art/Characters/ParcelMascot.png");
    }
    private static Sprite Courier()
    {
        Begin(160,220);Color dark=C("#172738");Circle(44,186,20,dark);Circle(116,186,20,dark);Circle(44,186,10,C("#94AAB6"));Circle(116,186,10,C("#94AAB6"));Line(new Vector2(44,185),new Vector2(77,146),5,C("#FFAF4F"));Line(new Vector2(77,146),new Vector2(116,185),5,C("#FFAF4F"));Line(new Vector2(44,185),new Vector2(116,185),4,C("#FFAF4F"));Rect(55,117,21,47,dark,9);Rect(89,117,21,47,dark,9);Rect(37,77,87,67,C("#DE6B3D"),20);Rect(47,79,66,53,C("#FF9953"),18);Rect(12,88,31,52,C("#F7B77D"),13);Rect(116,88,31,52,C("#F7B77D"),13);Rect(43,92,73,52,dark,10);Rect(51,97,57,35,C("#C48B52"),6);Rect(72,97,14,35,C("#FFE0A3"));Circle(80,55,29,C("#E8B483"));Rect(47,25,66,33,C("#FFB74F"),20);Rect(54,51,60,13,dark,6);Rect(63,53,21,5,C("#95D6E2"),2);return Finish("Assets/Art/Characters/Courier.png");
    }
    private static Sprite Town()
    {
        Begin(780,440);Rect(0,0,780,440,C("#314C65"));Circle(615,93,53,C("#FFCB77"));Circle(108,67,30,C("#526F81"));Circle(145,73,40,C("#526F81"));Rect(78,71,110,30,C("#526F81"),12);
        for(int i=0;i<10;i++){float x=i*91-30;float y=160+(i%3)*18;Rect(x,y,76,220,C(i%2==0?"#496879":"#3C596F"),5);}
        string[] fills={"#D88968","#CFAB80","#8DB9A9","#CB9B8F","#9BAECC"};
        for(int i=0;i<5;i++){float x=i*158+7,y=173+(i%2)*32;Rect(x+5,y+8,148,195,C("#1B2D40"),10);Rect(x,y,144,192,C(fills[i]),8);Rect(x-4,y-7,152,14,C("#FFE0A5"),5);for(int r=0;r<2;r++)for(int c=0;c<3;c++){Rect(x+16+c*39,y+24+r*42,23,29,C("#263E53"),4);Rect(x+19+c*39,y+27+r*42,17,22,C("#FFD78D"),2);}Rect(x+12,y+117,119,40,C("#243C4F"),5);Rect(x+8,y+106,129,16,C(i%2==0?"#EB8355":"#4D9883"),4);for(int a=0;a<6;a++)Rect(x+8+a*22,y+106,11,16,C("#FFF0CB"));Rect(x+23,y+121,27,36,C("#FEDEA2"),2);Rect(x+59,y+121,56,28,C("#90C1CA"),3);}
        Rect(0,371,780,69,C("#203448"));Rect(0,368,780,6,C("#E6B87D"));for(int i=0;i<8;i++)Rect(i*114+12,407,61,5,C("#C4AD88"),2);for(int i=0;i<4;i++){float x=40+i*226;Rect(x,295,9,73,C("#435246"));Circle(x+5,284,25,C("#497E68"));Circle(x-8,270,17,C("#649F80"));}return Finish("Assets/Art/Environment/Neighbourhood.png");
    }
    public static PaketciTheme Build()
    {
        Directory.CreateDirectory("Assets/UI/Fonts");Directory.CreateDirectory("Assets/GameData");
        Begin(96,96);Rect(0,0,96,96,Color.white,24);Sprite panel=Finish("Assets/Art/UI/RoundedPanel.png",true);
        Sprite[] icons=new Sprite[11];for(int i=0;i<icons.Length;i++)icons[i]=Icon(i);
        var town=Town();var courier=Courier();var mascot=Mascot();
        string source="Packages/com.unity.dt.app-ui/PackageResources/Fonts/Inter-SemiBold.ttf";
        if(!File.Exists("Assets/UI/Fonts/Inter-SemiBold.ttf"))File.Copy(source,"Assets/UI/Fonts/Inter-SemiBold.ttf");
        var info=UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(TMPro.TMP_Text).Assembly);
        // Import canonical TMP resources without an interactive dialog.
        if (AssetDatabase.FindAssets("t:TMP_Settings", new[]{"Assets"}).Length == 0)
            TMPro.TMP_PackageResourceImporter.ImportResources(true, false, false);
        AssetDatabase.Refresh();
        var font=AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>("Assets/UI/Fonts/PaketciFont.asset");
        if(font==null){font=TMPro.TMP_FontAsset.CreateFontAsset(AssetDatabase.LoadAssetAtPath<Font>("Assets/UI/Fonts/Inter-SemiBold.ttf"),80,8,UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA,1024,1024,TMPro.AtlasPopulationMode.Dynamic);string chars=new string(Enumerable.Range(32,95).Select(i=>(char)i).ToArray())+"ÇĞİÖŞÜçğıöşüı…•−×›←→★☆";font.TryAddCharacters(chars,out string missing);Debug.Log("Paketci font missing: "+missing);font.atlasPopulationMode=TMPro.AtlasPopulationMode.Static;AssetDatabase.CreateAsset(font,"Assets/UI/Fonts/PaketciFont.asset");AssetDatabase.AddObjectToAsset(font.material,font);foreach(var atlas in font.atlasTextures)AssetDatabase.AddObjectToAsset(atlas,font);}
        var theme=AssetDatabase.LoadAssetAtPath<PaketciTheme>("Assets/UI/PaketciTheme.asset");if(theme==null){theme=ScriptableObject.CreateInstance<PaketciTheme>();AssetDatabase.CreateAsset(theme,"Assets/UI/PaketciTheme.asset");}var so=new SerializedObject(theme);so.FindProperty("panel").objectReferenceValue=panel;so.FindProperty("town").objectReferenceValue=town;so.FindProperty("courier").objectReferenceValue=courier;so.FindProperty("mascot").objectReferenceValue=mascot;so.FindProperty("font").objectReferenceValue=font;var ar=so.FindProperty("icons");ar.arraySize=icons.Length;for(int i=0;i<icons.Length;i++)ar.GetArrayElementAtIndex(i).objectReferenceValue=icons[i];so.ApplyModifiedPropertiesWithoutUndo();
        if(AssetDatabase.LoadAssetAtPath<PaketciGameConfig>("Assets/GameData/PaketciGameConfig.asset")==null)AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<PaketciGameConfig>(),"Assets/GameData/PaketciGameConfig.asset");AssetDatabase.SaveAssets();return theme;
    }
}
