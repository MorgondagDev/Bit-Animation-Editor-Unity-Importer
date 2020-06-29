using UnityEngine;

using UnityEditor;
using UnityEditor.Animations;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;

using System.Linq;

namespace Bit {
    [Serializable]
    public class BitProject{
        public int width;
        public int height;
        public string name;
        public string id;
        public bool isTransparent;
        public bool isAnimated;
        public int scale;
        public string version;
        public float[] background = new float[]{0,0,0};
        public List<BitAssetData> assets = new List<BitAssetData>();
        public List<BitEntityData> scene = new List<BitEntityData>();

        public BitAssetData getAssetById(string id){
            foreach(BitAssetData b in assets){
                if(b.id == id){
                    return b;
                }
            }
            return null;
        }
        public BitEntityData getEntityId(string id){
            foreach(BitEntityData a in scene){
                if(a.id == id){
                    return a;
                }
            }
            return null;
        }
    }

    [Serializable]
    public class BitAssetData{
       public string id;
       public int defaultTile;
       public string name;
       public string sprite;
       public Vector2 spriteSize;
       public Vector2 tileSize;
       public List<BitAssetAnimation> animations = new List<BitAssetAnimation>();

       public string getAnimationName(int index){
        if(animations.Count == 0 && index == 0){
             return "idle";
        }
        if(animations.Count < 0 || index < 0 || index > animations.Count){
            return "idle";
        }
        return animations[index].name;
       }
    }

    [Serializable]
    public class BitAssetAnimation{
       public int frameskip;
       public string id;
       public string name;
       public List<int> tiles = new List<int>();
    }

    [Serializable]
    public class BitEntityData{
       public string assetId;
       public string id;
       public string name;
       public BitEntityDataOptions data;
    }

    [Serializable]
    public class BitEntityDataOptions{
        public string assetId;
        public string id;
        public string name;
        public bool isVisible;
        public float[] tint = new float[]{0,0,0};
        public bool flipX;
        public bool flipY;
        public int rotation;
        public int rotateRepeat;
        public int rotateRepeatSpace;
        public Vector2 position;
        public int defaultTile;
        public Vector2 mirror;
        public bool mirrorX;
        public bool mirrorY;
        public int repeatXCount;
        public int repeatXOffset;
        public int repeatYCount;
        public int repeatYOffset;
        public int animationIndex;
        public int alpha;
        public float rotationSpeed;
        public bool isActive;
        public bool animationsEnabled;
        public float initialAnimationDelay;
        public List<BitEntityAnimationData> animations = new List<BitEntityAnimationData>();
    }

    public enum Easing {
        Linear,
        QuadIn,
        QuadOut,
        QuadInOut,
        CubeIn,
        CubeOut,
        CubeInOut,
        QuartIn,
        QuartOut,
        QuartInOut,
        QuintIn,
        QuintOut,
        QuintInOut,
        SineIn,
        SineOut,
        SineInOut,
        ExpoIn,
        ExpoOut,
        ExpoInOut,
        CircIn,
        CircOut,
        CircInOut,
    }

    [Serializable]
    public class BitEntityAnimationData{
        public float delay;
        public float duration;
        public Easing easing;
        public BitEntityAnimationOptions data;
    }

    [Serializable]
    public class BitEntityAnimationOptions{
        public bool hasTransformX;
        public float transformX;
        public bool hasTransformY;
        public float transformY;
        public bool hasAlpha;
        public float alpha;
        public bool hasRotation;
        public float rotation;
        public bool hasRotateSpeed;
        public float rotateSpeed;
        public bool hasSpriteFlipX;
        public int spriteFlipX;
        public bool hasSpriteFlipY;
        public int spriteFlipY;
        public bool hasMirrorDistanceX;
        public float mirrorDistanceX;
        public bool hasMirrorDistanceY;
        public float mirrorDistanceY;
    }

    public class BitAnimationEditor : EditorWindow {
        static int renderScale = 100;
        static float renderScaleMultiplier = 0.01f;

        static GameObject CloneInstansiatedPrefab(
            UnityEngine.Object assetPrefab,
            GameObject clone,
            Vector2 position,
            Quaternion rotation,
            Transform parent
        ){
            GameObject newAsset = PrefabUtility.InstantiatePrefab(assetPrefab) as GameObject;
            SpriteRenderer cloneRenderer = clone.GetComponent<SpriteRenderer>();
            SpriteRenderer assetRenderer = newAsset.GetComponent<SpriteRenderer>();
            assetRenderer.sortingOrder = cloneRenderer.sortingOrder;
            assetRenderer.flipX = cloneRenderer.flipX;
            assetRenderer.flipY = cloneRenderer.flipY;
            assetRenderer.color = cloneRenderer.color;
            newAsset.transform.parent = parent;
            newAsset.transform.rotation = rotation;
            newAsset.transform.position = position;
            return newAsset;
        }

        [MenuItem("Window/Bit Animation Editor/Bundle Importer")]
        static void ImportBundle(){
            string path = EditorUtility.OpenFilePanel("Bit exported bundle", "", "zip");
            if(path == ""){
                return;
            }

            try {
                string tempDirectory = Application.dataPath+"/Resources/Bit/Import";
                FileUtil.DeleteFileOrDirectory(tempDirectory);
                ZipFile.ExtractToDirectory(path, tempDirectory);

                BitProject currentProject = JsonUtility.FromJson<BitProject>(System.IO.File.ReadAllText(tempDirectory+"/bit.json"));

                FileUtil.DeleteFileOrDirectory(Application.dataPath+"/Resources/Bit/"+currentProject.id);
                string projectDirectory = "Assets/Resources/Bit/"+currentProject.id;
                AssetDatabase.CreateFolder("Assets/Resources/Bit", currentProject.id);
                AssetDatabase.CreateFolder("Assets/Resources/Bit/"+currentProject.id, "assets");
                AssetDatabase.CreateFolder("Assets/Resources/Bit/"+currentProject.id, "prefabs");
                FileUtil.ReplaceDirectory(tempDirectory, Application.dataPath+"/Resources/Bit/"+currentProject.id+"/assets");
                FileUtil.DeleteFileOrDirectory(tempDirectory);
                AssetDatabase.Refresh();

                Material defaultMaterial = new Material(Shader.Find("Sprites/Diffuse"));
                AssetDatabase.CreateAsset(defaultMaterial, "Assets/Resources/Bit/" + currentProject.id + "/sprite.mat");

                foreach(BitAssetData asset in currentProject.assets){
                    string filePath = "Bit/" + currentProject.id + "/assets/" + asset.sprite.Replace(".png", "");

                    Texture2D texture = Resources.Load<Texture2D> (filePath);
                    string texturePath = AssetDatabase.GetAssetPath(texture);

                    TextureImporter ti = AssetImporter.GetAtPath(texturePath) as TextureImporter;
                    ti.isReadable = true;
                    ti.filterMode = FilterMode.Point;
                    ti.spriteImportMode = SpriteImportMode.Multiple;
                    ti.compressionQuality = 100;
                    ti.textureCompression = TextureImporterCompression.Uncompressed;

                    List<SpriteMetaData> newData = new List<SpriteMetaData>();
                    int SliceWidth = (int)asset.tileSize.x;
                    int SliceHeight = (int)asset.tileSize.y;
                    for (int i = 0; i < texture.width; i += SliceWidth){
                        for(int j = texture.height; j > 0;  j -= SliceHeight) {
                            SpriteMetaData smd = new SpriteMetaData();
                            smd.pivot = new Vector2(0.5f, 0.5f);
                            smd.alignment = 9;
                            smd.name = (texture.height - j)/SliceHeight + ", " + i/SliceWidth;
                            smd.rect = new Rect(i, j-SliceHeight, SliceWidth, SliceHeight);
                            newData.Add(smd);
                        }
                    }
                    ti.spritesheet = newData.ToArray();
                    AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceUpdate);
                    AssetDatabase.Refresh();

                    List<AnimationClip> animationClips = new List<AnimationClip>();
                    Sprite[] sprites = Resources.LoadAll<Sprite>(filePath);

                    AnimationClip idleClip = new AnimationClip();
                    idleClip.frameRate = 60;
                    idleClip.name = "idle";
                    idleClip.wrapMode = WrapMode.Loop;
                    AnimationClipSettings idleSettings = AnimationUtility.GetAnimationClipSettings(idleClip);
                    idleSettings.loopTime = true;
                    AnimationUtility.SetAnimationClipSettings(idleClip, idleSettings);
                    EditorCurveBinding idleSpriteBinding = new EditorCurveBinding();
                    idleSpriteBinding.type = typeof(SpriteRenderer);
                    idleSpriteBinding.path = "";
                    idleSpriteBinding.propertyName = "m_Sprite";
                    ObjectReferenceKeyframe[] idleSpriteFrames = new ObjectReferenceKeyframe[1];
                    idleSpriteFrames[0] = new ObjectReferenceKeyframe();
                    idleSpriteFrames[0].time = 1;
                    idleSpriteFrames[0].value = sprites[asset.defaultTile];
                    AnimationUtility.SetObjectReferenceCurve(idleClip, idleSpriteBinding, idleSpriteFrames);
                    animationClips.Add(idleClip);

                    foreach(BitAssetAnimation anim in asset.animations){
                        AnimationClip clip = new AnimationClip();
                        clip.frameRate = 60;
                        clip.name = anim.name;
                        clip.wrapMode = WrapMode.Loop;
                        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
                        settings.loopTime = true;
                        AnimationUtility.SetAnimationClipSettings(clip, settings);
                        EditorCurveBinding spriteBinding = new EditorCurveBinding();
                        spriteBinding.type = typeof(SpriteRenderer);
                        spriteBinding.path = "";
                        spriteBinding.propertyName = "m_Sprite";

                        ObjectReferenceKeyframe[] spriteKeyFrames = new ObjectReferenceKeyframe[anim.tiles.Count];

                        for (int i = 0; i < (anim.tiles.Count); i++) {
                            spriteKeyFrames[i] = new ObjectReferenceKeyframe();
                            spriteKeyFrames[i].time = ((float) (i*(anim.frameskip)) /60f);
                            spriteKeyFrames[i].value = sprites[anim.tiles[i]];
                        }
                        AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, spriteKeyFrames);
                        animationClips.Add(clip);
                    }

                    var animatorController = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath(
                        "Assets/Resources/Bit/" + currentProject.id + "/assets/"+asset.id + "_animationController.controller"
                    );

                    string controllerPath = AssetDatabase.GetAssetPath(animatorController);

                    foreach(AnimationClip a in animationClips){
                        AssetDatabase.AddObjectToAsset(a, animatorController);
                        AnimatorState state = animatorController.AddMotion(a,0);
                        state.name = a.name;
                    }

                    AssetDatabase.ImportAsset(controllerPath);

                    GameObject assetGameObject = new GameObject(asset.id);
                    SpriteRenderer assetRenderer = assetGameObject.AddComponent<SpriteRenderer>();
                    BitAsset assetScript = assetGameObject.AddComponent<BitAsset>();

                    assetRenderer.material = defaultMaterial;
                    assetRenderer.sprite = sprites[0];
                    assetRenderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
                    Animator animator = assetGameObject.AddComponent<Animator>();
                    animator.runtimeAnimatorController = animatorController;
                    assetScript.animator = animator;
                    assetScript.renderer = assetRenderer;

                    PrefabUtility.SaveAsPrefabAssetAndConnect(
                        assetGameObject,
                        "Assets/Resources/Bit/" +  currentProject.id + "/prefabs/" +asset.id + ".prefab",
                        InteractionMode.AutomatedAction
                    );
                    DestroyImmediate(assetGameObject);
                }

                GameObject rootGameObject = new GameObject(currentProject.name);
                GameObject canvasGameObject = new GameObject("canvas");
                canvasGameObject.transform.parent = rootGameObject.transform;
                SpriteMask spriteMask = canvasGameObject.AddComponent<SpriteMask>();
                SpriteRenderer canvasRenderer = canvasGameObject.AddComponent<SpriteRenderer>();

                Sprite backgroundSprite = (Sprite)AssetDatabase.LoadAssetAtPath("Assets/Resources/Bit/" + currentProject.id + "/assets/background.png", typeof(Sprite));;
                spriteMask.sprite = backgroundSprite;
                canvasRenderer.sprite = backgroundSprite;
                canvasRenderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
                canvasRenderer.material = defaultMaterial;

                Vector3 scaleVector = new Vector3(currentProject.scale, currentProject.scale, currentProject.scale);

                for (int x = currentProject.scene.Count-1; x >= 0; x--){
                    List<GameObject> children = new List<GameObject>();

                    BitEntityData entity = currentProject.scene[x];
                    BitAssetData assetData = currentProject.getAssetById(entity.assetId);
                    GameObject entityGameObject = new GameObject( currentProject.scene.Count-x + "." + entity.name);
                    entityGameObject.transform.parent = canvasGameObject.transform;

                    BitEntity entityScript = entityGameObject.AddComponent<BitEntity>();
                    entityScript.animation = assetData.getAnimationName(entity.data.animationIndex);
                    entity.data.assetId = entity.assetId;
                    entity.data.id = entity.id;
                    entity.data.name = entity.name;
                    entityScript.data = entity.data;
                    entityScript.assetData = assetData;
                    entityScript.assets = new List<BitAsset>();

                    UnityEngine.Object assetPrefab = (UnityEngine.Object)AssetDatabase.LoadAssetAtPath("Assets/Resources/Bit/" + currentProject.id + "/prefabs/" + entity.assetId + ".prefab", typeof(UnityEngine.Object));
                    GameObject currentAsset = PrefabUtility.InstantiatePrefab(assetPrefab) as GameObject;

                    SpriteRenderer assetRenderer = currentAsset.GetComponent<SpriteRenderer>();

                    string filePath = "Bit/" + currentProject.id + "/assets/" + assetData.sprite.Replace(".png", "");
                    Sprite[] sprites = Resources.LoadAll<Sprite>(filePath);
                    assetRenderer.sprite = sprites[ Mathf.Max(entity.data.defaultTile,0)];

                    Color tintColor = new Color(
                        entity.data.tint[0],
                        entity.data.tint[1],
                        entity.data.tint[2]
                    );
                    tintColor.a = (float)entity.data.alpha / 255f;

                    if(
                        entity.data.tint[0] == 0 &&
                        entity.data.tint[1] == 0 &&
                        entity.data.tint[2] == 0 &&
                        entity.data.alpha == 0
                    ){
                        tintColor.r = 255;
                        tintColor.g = 255;
                        tintColor.b = 255;
                        tintColor.a = 1f;
                    }

                    assetRenderer.flipX = entity.data.flipX;
                    assetRenderer.flipY = entity.data.flipY;
                    assetRenderer.color = tintColor;

                    currentAsset.transform.parent = entityGameObject.transform;
                    currentAsset.transform.eulerAngles = new Vector3( 0, 0, -entity.data.rotation );

                    currentAsset.transform.localScale = scaleVector;

                    if(entity.data.mirrorX && entity.data.mirrorY){
                        float xPos = ( entity.data.mirror.x / 2) * renderScaleMultiplier;
                        float yPos = ( entity.data.mirror.y / 2 ) * renderScaleMultiplier;

                        Vector2 pos1 = new Vector2( -xPos, yPos ); // <-- top left corner
                        Vector2 pos2 = new Vector2( xPos, yPos ); // <-- top right corner
                        Vector2 pos3 = new Vector2( -xPos, -yPos ); // <-- bottom left corner
                        Vector2 pos4 = new Vector2( xPos, -yPos ); // <-- bottom right corner

                        currentAsset.transform.localPosition = pos1;

                        List<GameObject> p1Container = new List<GameObject>();
                        List<GameObject> p2Container = new List<GameObject>();
                        List<GameObject> p3Container = new List<GameObject>();
                        List<GameObject> p4Container = new List<GameObject>();

                        p1Container.Add(currentAsset);

                        GameObject x1 = CloneInstansiatedPrefab(
                            assetPrefab,
                            currentAsset,
                            pos2,
                            Quaternion.Euler(0, 0, entity.data.rotation),
                            entityGameObject.transform
                        );
                        x1.transform.localScale = scaleVector;

                        SpriteRenderer x1R = x1.GetComponent<SpriteRenderer>();
                        x1R.flipX = !entity.data.flipX;
                        p2Container.Add(x1);
                        x1.GetComponent<BitAsset>().isMirroredX = true;

                        GameObject y1 = CloneInstansiatedPrefab(
                            assetPrefab,
                            currentAsset,
                            pos3,
                            Quaternion.Euler(0, 0, entity.data.rotation),
                            entityGameObject.transform
                        );
                        y1.transform.localScale = scaleVector;

                        SpriteRenderer x2R = y1.GetComponent<SpriteRenderer>();
                        x2R.flipY = !entity.data.flipY;
                        p3Container.Add(y1);
                        y1.GetComponent<BitAsset>().isMirroredX = true;

                        GameObject xy1 = CloneInstansiatedPrefab(
                            assetPrefab,
                            currentAsset,
                            pos4,
                            Quaternion.Euler(0, 0, -entity.data.rotation),
                            entityGameObject.transform
                        );
                        xy1.transform.localScale = scaleVector;

                        SpriteRenderer x3R = xy1.GetComponent<SpriteRenderer>();
                        x3R.flipY = !entity.data.flipY;
                        x3R.flipX = !entity.data.flipX;
                        p4Container.Add(xy1);


                        for(int r = 0; r < entity.data.rotateRepeat; r++){

                            GameObject repeated1 = CloneInstansiatedPrefab(
                                assetPrefab,
                                currentAsset,
                                pos1,
                                Quaternion.Euler(0, 0, - (entity.data.rotation + ( (r + 1) *entity.data.rotateRepeatSpace)) ),
                                entityGameObject.transform
                            );
                            repeated1.transform.localScale = scaleVector;
                            p1Container.Add(repeated1);

                            GameObject repeated2 = CloneInstansiatedPrefab(
                                assetPrefab,
                                x1,
                                pos2,
                                Quaternion.Euler(0, 0,  (entity.data.rotation + ( (r + 1) *entity.data.rotateRepeatSpace)) ),
                                entityGameObject.transform
                            );
                            repeated2.transform.localScale = scaleVector;
                            p2Container.Add(repeated2);

                            GameObject repeated3 = CloneInstansiatedPrefab(
                                assetPrefab,
                                y1,
                                pos3,
                                Quaternion.Euler(0, 0,  (entity.data.rotation + ( (r + 1) *entity.data.rotateRepeatSpace)) ),
                                entityGameObject.transform
                            );
                            repeated3.transform.localScale = scaleVector;
                            p3Container.Add(repeated3);

                            GameObject repeated4 = CloneInstansiatedPrefab(
                                assetPrefab,
                                xy1,
                                pos4,
                                Quaternion.Euler(0, 0,  - (entity.data.rotation + ( (r + 1) *entity.data.rotateRepeatSpace)) ),
                                entityGameObject.transform
                            );
                            repeated4.transform.localScale = scaleVector;
                            p4Container.Add(repeated4);
                        }

                        GameObject p1G = new GameObject("mirrorXTop");
                        p1G.transform.parent = entityGameObject.transform;
                        foreach (GameObject child in p1Container){
                            child.transform.parent = p1G.transform;
                        }
                        children.Add(p1G);

                        GameObject p2G = new GameObject("mirrorYTop");
                        p2G.transform.parent = entityGameObject.transform;
                        foreach (GameObject child in p2Container){
                            child.transform.parent = p2G.transform;
                        }
                        children.Add(p2G);

                        GameObject p3G = new GameObject("mirrorXBottom");
                        p3G.transform.parent = entityGameObject.transform;
                        foreach (GameObject child in p3Container){
                            child.transform.parent = p3G.transform;
                        }
                         children.Add(p3G);

                        GameObject p4G = new GameObject("mirrorYBottom");
                        p4G.transform.parent = entityGameObject.transform;
                        foreach (GameObject child in p4Container){
                            child.transform.parent = p4G.transform;
                        }
                         children.Add(p4G);

                    } else if( entity.data.mirrorX ) {

                        float xPos = ( entity.data.mirror.x/2) * renderScaleMultiplier;
                        Vector2 pos1 = new Vector2( -xPos, 0 ); // <-- left side
                        Vector2 pos2 = new Vector2(  xPos, 0 ); // <-- right side

                        currentAsset.transform.localPosition = pos1;

                        List<GameObject> p1Container = new List<GameObject>();
                        List<GameObject> p2Container = new List<GameObject>();

                        p1Container.Add(currentAsset);

                        GameObject x1 = CloneInstansiatedPrefab(
                                assetPrefab,
                            currentAsset,
                            pos2,
                            Quaternion.Euler(0, 0, entity.data.rotation),
                            entityGameObject.transform
                        );
                        x1.transform.localScale = scaleVector;
                        x1.GetComponent<SpriteRenderer>().flipX = !entity.data.flipX;
                        p2Container.Add(x1);
                        x1.GetComponent<BitAsset>().isMirroredX = true;


                        for(int r = 0; r < entity.data.rotateRepeat; r++){

                            GameObject repeated1 = CloneInstansiatedPrefab(
                                assetPrefab,
                                currentAsset,
                                pos1,
                                Quaternion.Euler(0, 0, - (entity.data.rotation + ( (r + 1) *entity.data.rotateRepeatSpace)) ),
                                entityGameObject.transform
                            );
                            repeated1.transform.localScale = scaleVector;
                            p1Container.Add(repeated1);

                            GameObject repeated2 = CloneInstansiatedPrefab(
                                assetPrefab,
                                x1,
                                pos2,
                                Quaternion.Euler(0, 0, (entity.data.rotation + ( (r + 1) *entity.data.rotateRepeatSpace)) ),
                                entityGameObject.transform
                            );
                            repeated2.transform.localScale = scaleVector;
                            p2Container.Add(repeated2);
                        }

                        GameObject p1G = new GameObject("mirrorXLeft");
                        p1G.transform.parent = entityGameObject.transform;
                        foreach (GameObject child in p1Container){
                            child.transform.parent = p1G.transform;
                        }
                        children.Add(p1G);

                        GameObject p2G = new GameObject("mirrorXRight");
                        p2G.transform.parent = entityGameObject.transform;
                        foreach (GameObject child in p2Container){
                            child.transform.parent = p2G.transform;
                        }
                        children.Add(p2G);


                    } else if( entity.data.mirrorY ) {
                        float yPos = ( entity.data.mirror.y /2 ) * renderScaleMultiplier;
                        Vector2 pos1 = new Vector2( 0, yPos ); // <-- top side
                        Vector2 pos2 = new Vector2( 0, -yPos ); // <-- bottom side

                        currentAsset.transform.localPosition = pos1;

                        List<GameObject> p1Container = new List<GameObject>();
                        List<GameObject> p2Container = new List<GameObject>();

                        p1Container.Add(currentAsset);

                        GameObject y1 = CloneInstansiatedPrefab(
                                assetPrefab,
                            currentAsset,
                            pos2,
                            Quaternion.Euler(0, 0, entity.data.rotation),
                            entityGameObject.transform
                        );
                        y1.transform.localScale = scaleVector;
                        y1.GetComponent<SpriteRenderer>().flipY = !entity.data.flipY;
                        p2Container.Add(y1);

                        for(int r = 0; r < entity.data.rotateRepeat; r++){

                            GameObject repeated1 = CloneInstansiatedPrefab(
                                assetPrefab,
                                currentAsset,
                                pos1,
                                Quaternion.Euler(0, 0, - (entity.data.rotation + ( (r + 1) *entity.data.rotateRepeatSpace)) ),
                                entityGameObject.transform
                            );
                            repeated1.transform.localScale = scaleVector;
                            p1Container.Add(repeated1);

                            GameObject repeated2 = CloneInstansiatedPrefab(
                                assetPrefab,
                                y1,
                                pos2,
                                Quaternion.Euler(0, 0, (entity.data.rotation + ( (r + 1) *entity.data.rotateRepeatSpace)) ),
                                entityGameObject.transform
                            );
                            repeated2.transform.localScale = scaleVector;
                            p2Container.Add(repeated2);
                        }

                        GameObject p1G = new GameObject("mirrorYTop");
                        p1G.transform.parent = entityGameObject.transform;
                        foreach (GameObject child in p1Container){
                            child.transform.parent = p1G.transform;
                        }
                        children.Add(p1G);

                        GameObject p2G = new GameObject("mirrorYBottom");
                        p2G.transform.parent = entityGameObject.transform;
                        foreach (GameObject child in p2Container){
                            child.transform.parent = p2G.transform;
                        }
                        children.Add(p2G);
                    } else {
                        children.Add(currentAsset);
                        for(int r = 0; r < entity.data.rotateRepeat; r++){
                            GameObject repeated = CloneInstansiatedPrefab(
                                assetPrefab,
                                currentAsset,
                                currentAsset.transform.position,
                                Quaternion.Euler(0, 0, - (entity.data.rotation + ( (r + 1) *entity.data.rotateRepeatSpace)) ),
                                entityGameObject.transform
                            );
                            repeated.transform.localScale = scaleVector;
                            children.Add(repeated);
                        }
                    }

                    GameObject entityContainer = new GameObject("container");
                    entityContainer.transform.parent = entityGameObject.transform;
                    foreach (GameObject child in children){
                        child.transform.parent = entityContainer.transform;
                    }

                    int countXLeft = 0;
                    int countXRight = 0;
                    for (int xc = entity.data.repeatXCount; xc > 0; --xc){
                        float xp = 0;
                        if ( xc % 2 == 0){
                            countXLeft ++;
                            xp = - ( countXLeft * entity.data.repeatXOffset) * renderScaleMultiplier;
                        } else {
                            countXRight ++;
                            xp = ( countXRight * entity.data.repeatXOffset) * renderScaleMultiplier;
                        }

                        GameObject containerX = Instantiate (entityContainer);
                        containerX.transform.localPosition = new Vector2(xp, 0);
                        containerX.transform.parent = entityGameObject.transform;

                        int countYLeftX = 0;
                        int countYRightX = 0;
                        for (int yc = entity.data.repeatYCount; yc > 0; --yc){
                            float yp = 0;
                            if ( yc % 2 == 0){
                                countYLeftX ++;
                                yp =  ( countYLeftX * entity.data.repeatYOffset) * renderScaleMultiplier;
                            } else {
                                countYRightX ++;
                                yp = - ( countYRightX * entity.data.repeatYOffset) * renderScaleMultiplier;
                            }
                            GameObject  container1 = Instantiate (entityContainer);
                            container1.transform.localPosition = new Vector2(xp, yp);
                            container1.transform.parent = entityGameObject.transform;
                        }
                    }

                    int countYLeft = 0;
                    int countYRight = 0;
                    for (int yc = entity.data.repeatYCount; yc > 0; --yc){
                        float yp = 0;
                        if ( yc % 2 == 0){
                            countYLeft ++;
                            yp = ( countYLeft * entity.data.repeatYOffset) * renderScaleMultiplier;
                        } else {
                            countYRight ++;
                            yp = - ( countYRight * entity.data.repeatYOffset) * renderScaleMultiplier;
                        }
                        GameObject  container1 = Instantiate (entityContainer);
                        container1.transform.localPosition = new Vector2(0, yp);
                        container1.transform.parent = entityGameObject.transform;
                    }
                    entityGameObject.transform.localPosition = new Vector2(
                        entity.data.position.x*renderScaleMultiplier,
                        -entity.data.position.y*renderScaleMultiplier
                    );
                }

                SpriteRenderer[] renderers = rootGameObject.GetComponentsInChildren<SpriteRenderer>();
                for(int i = 0; i < renderers.Length; i++){
                    renderers[i].sortingOrder = 20 + (renderers.Length - i);
                }
                canvasRenderer.sortingOrder = 20;
                BitEntity[] entities = rootGameObject.GetComponentsInChildren<BitEntity>();
                for(int i = 0; i < entities.Length; i++){
                   BitAsset[] assets = entities[i].gameObject.GetComponentsInChildren<BitAsset>();
                   entities[i].assets = new List<BitAsset>(assets);
                }
                PrefabUtility.SaveAsPrefabAssetAndConnect(
                    rootGameObject,
                    "Assets/Resources/Bit/" +  currentProject.id + "/" + currentProject.name + ".prefab",
                    InteractionMode.AutomatedAction
                );
                AssetDatabase.SaveAssets();
            } finally {
                AssetDatabase.Refresh();
            }
        }
    }

    static public class Tween{
        static public float EaseLinear(float e, float t, float n, float r) {
            return (n * e / r + t);
        }
        static public float EaseQuadIn(float e, float t, float n, float r) {
            e  /= r;
            return (n * e * e + t);
        }
        static public float EaseQuadOut(float e, float t, float n, float r) {
            e /= r;
            return (-n * e * (e - 2) + t);
        }
        static public float EaseQuadInOut(float e, float t, float n, float r) {
            e /= r / 2;
            if(e < 1){
                return (n / 2 * e * e + t);
            }
            e--;
            return (-n / 2 * (e * (e - 2) - 1) + t);
        }
        static public float EaseCubeIn(float e, float t, float n, float r) {
            e /= r;
            return (n * e * e * e + t);
        }
        static public float EaseCubeOut(float e, float t, float n, float r) {
            e /= r;
            e--;
            return (n * (e * e * e + 1) + t);
        }
        static public float EaseCubeInOut(float e, float t, float n, float r) {
            e /= r / 2;
            if(e < 1){
                return (n / 2 * e * e * e + t);
            }
            e -= 2;
            return (n / 2 * (e * e * e + 2) + t);
        }
        static public float EaseQuartIn(float e, float t, float n, float r) {
            e /= r;
            return (n * e * e * e * e + t);
        }
        static public float EaseQuartOut(float e, float t, float n, float r) {
            e /= r;
            e--;
            return (-n * (e * e * e * e - 1) + t);
        }
        static public float EaseQuartInOut(float e, float t, float n, float r) {
            e /= r / 2;
            if(e < 1){
                return (n / 2 * e * e * e * e + t);
            }
            e -= 2;
            return (-n / 2 * (e * e * e * e - 2) + t);
        }
        static public float EaseQuintIn(float e, float t, float n, float r) {
            e /= r;
            return (n * e * e * e * e * e + t);
        }
        static public float EaseQuintOut(float e, float t, float n, float r) {
            e /= r;
            e--;
            return (n * (e * e * e * e * e + 1) + t);
        }
        static public float EaseQuintInOut(float e, float t, float n, float r) {
            e /= r / 2;
            if(e < 1){
                return (n / 2 * e * e * e * e * e + t);
            }
            e -= 2;
            return (n / 2 * (e * e * e * e * e + 2) + t);
        }
        static public float EaseSineIn(float e, float t, float n, float r) {
            return (-n * Mathf.Cos(e / r * (Mathf.PI / 2)) + n + t);
        }
        static public float EaseSineOut(float e, float t, float n, float r) {
            return (n * Mathf.Sin(e / r * (Mathf.PI / 2)) + t);
        }
        static public float EaseSineInOut(float e, float t, float n, float r) {
            return (-n / 2 * (Mathf.Cos(Mathf.PI * e / r) - 1) + t);
        }
        static public float EaseExpoIn(float e, float t, float n, float r) {
            return (n * Mathf.Pow(2, 10 * (e / r - 1)) + t);
        }
        static public float EaseExpoOut(float e, float t, float n, float r) {
            return (n * (-Mathf.Pow(2, -10 * e / r) + 1) + t);
        }
        static public float EaseExpoInOut(float e, float t, float n, float r) {
            e /= r / 2f;
            if(e < 1){
                return (n / 2f * Mathf.Pow(2f, 10f * (e - 1f)) + t);
            }
            e--;
            return (n / 2f * (-Mathf.Pow(2f, -10f * e) + 2f) + t);
        }
        static public float EaseCircIn(float e, float t, float n, float r) {
             e /= r;
            return -n * (Mathf.Sqrt(1 - e * e) - 1) + t;
        }
        static public float EaseCircOut(float e, float t, float n, float r) {
            e /= r;
            e--;
            return (n * Mathf.Sqrt(1f - e * e) + t);
        }
        static public float EaseCircInOut(float e, float t, float n, float r) {
            e /= r / 2;
            if(e < 1){
                return (-n / 2 * (Mathf.Sqrt(1 - e * e) - 1) + t);
            }
            e -= 2;
            return (n / 2 * (Mathf.Sqrt(1 - e * e) + 1) + t);
        }

        static public float animate(float e, float t, float n, float r, Easing easing) {
          switch (easing) {
            case Easing.Linear:
              return EaseLinear(e, t, n, r);
            case Easing.QuadIn:
              return EaseQuadIn(e, t, n, r);
            case Easing.QuadOut:
              return EaseQuadOut(e, t, n, r);
            case Easing.QuadInOut:
              return EaseQuadInOut(e, t, n, r);
            case Easing.CubeIn:
              return EaseCubeIn(e, t, n, r);
            case Easing.CubeOut:
              return EaseCubeOut(e, t, n, r);
            case Easing.CubeInOut:
              return EaseCubeInOut(e, t, n, r);
            case Easing.QuartIn:
              return EaseQuartIn(e, t, n, r);
            case Easing.QuartOut:
              return EaseQuartOut(e, t, n, r);
            case Easing.QuartInOut:
              return EaseQuartInOut(e, t, n, r);
            case Easing.QuintIn:
              return EaseQuintIn(e, t, n, r);
            case Easing.QuintOut:
              return EaseQuintOut(e, t, n, r);
            case Easing.QuintInOut:
              return EaseQuintInOut(e, t, n, r);
            case Easing.SineIn:
              return EaseSineIn(e, t, n, r);
            case Easing.SineOut:
              return EaseSineOut(e, t, n, r);
            case Easing.SineInOut:
              return EaseSineInOut(e, t, n, r);
            case Easing.ExpoIn:
              return EaseExpoIn(e, t, n, r);
            case Easing.ExpoOut:
              return EaseExpoOut(e, t, n, r);
            case Easing.ExpoInOut:
              return EaseExpoInOut(e, t, n, r);
            case Easing.CircIn:
              return EaseCircIn(e, t, n, r);
            case Easing.CircOut:
              return EaseCircOut(e, t, n, r);
            case Easing.CircInOut:
              return EaseCircInOut(e, t, n, r);
            default:
              return EaseLinear(e, t, n, r);
          }
        }
    }
}
