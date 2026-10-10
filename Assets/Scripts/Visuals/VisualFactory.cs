using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// Karakterleri ve bossları basit şekillerden (küp, küre, silindir...) kod ile kurar.
// Tüm parçalar DamageFlash shader'ını kullanır; böylece hasar yanıp sönmesi yeni modellerde de çalışır.
// İleride gerçek 3D modeller geldiğinde sadece *Visual sınıflarının Build metotlarını değiştirmek yeterli.
public static class VisualFactory {

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int FlashColorId = Shader.PropertyToID("_FlashColor");
    private static readonly int FlashAmountId = Shader.PropertyToID("_FlashAmount");
    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

    private static Material template;
    private static readonly Dictionary<PrimitiveType, Mesh> meshes = new Dictionary<PrimitiveType, Mesh>();
    private static readonly Dictionary<Color, Material> solidMaterials = new Dictionary<Color, Material>();
    private static readonly Dictionary<Color, Material> glowMaterials = new Dictionary<Color, Material>();

    public static bool IsInitialized => template != null;

    public static void Initialize(Material flashTemplate) {
        if (template != null && template == flashTemplate) return;

        if (flashTemplate != null && flashTemplate.HasProperty(FlashAmountId)) {
            template = flashTemplate;
        }
        else {
            Debug.LogWarning("VisualFactory: DamageFlash materyali bulunamadı, URP/Lit kullanılıyor.");
            template = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        }

        // Önbellekteki eski materyaller önceki sahneye ait olabilir
        solidMaterials.Clear();
        glowMaterials.Clear();
    }

    public static Mesh GetMesh(PrimitiveType type) {
        if (!meshes.TryGetValue(type, out Mesh mesh) || mesh == null) {
            GameObject temp = GameObject.CreatePrimitive(type);
            mesh = temp.GetComponent<MeshFilter>().sharedMesh;
            Object.DestroyImmediate(temp);
            meshes[type] = mesh;
        }
        return mesh;
    }

    public static Material Solid(Color color) {
        if (!solidMaterials.TryGetValue(color, out Material material) || material == null) {
            material = new Material(template) { name = "Solid_" + ColorUtility.ToHtmlStringRGB(color) };
            material.SetColor(BaseColorId, color);
            if (material.HasProperty(FlashAmountId)) {
                material.SetFloat(FlashAmountId, 0f);
            }
            solidMaterials[color] = material;
        }
        return material;
    }

    // Kendi ışığını yayan (bloom ile parlayan) parçalar için
    public static Material Glow(Color color, float intensity = 3f) {
        Color key = new Color(color.r, color.g, color.b, intensity);
        if (!glowMaterials.TryGetValue(key, out Material material) || material == null) {
            material = new Material(template) { name = "Glow_" + ColorUtility.ToHtmlStringRGB(color) };
            Color hdr = color * intensity;
            hdr.a = 1f;

            material.SetColor(BaseColorId, color);
            if (material.HasProperty(FlashAmountId)) {
                material.SetColor(FlashColorId, hdr);
                material.SetFloat(FlashAmountId, 1f);
            }
            else if (material.HasProperty(EmissionColorId)) {
                material.EnableKeyword("_EMISSION");
                material.SetColor(EmissionColorId, hdr);
            }
            glowMaterials[key] = material;
        }
        return material;
    }

    public static Transform CreateRoot(Transform parent, string name) {
        GameObject root = new GameObject(name);
        root.layer = parent.gameObject.layer;
        root.transform.SetParent(parent, false);
        return root.transform;
    }

    public static Transform Part(PrimitiveType type, Transform parent, Vector3 localPosition, Vector3 localScale, Material material, Vector3 localEuler = default) {
        GameObject part = new GameObject(type.ToString());
        part.layer = parent.gameObject.layer;

        Transform t = part.transform;
        t.SetParent(parent, false);
        t.localPosition = localPosition;
        t.localRotation = Quaternion.Euler(localEuler);
        t.localScale = localScale;

        part.AddComponent<MeshFilter>().sharedMesh = GetMesh(type);
        MeshRenderer meshRenderer = part.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = material;
        meshRenderer.shadowCastingMode = ShadowCastingMode.On;

        return t;
    }

    // Parlayan parça: hasar yanıp sönmesinden etkilenmez ve gölge düşürmez
    public static Transform GlowPart(PrimitiveType type, Transform parent, Vector3 localPosition, Vector3 localScale, Color color, float intensity = 3f, Vector3 localEuler = default) {
        Transform t = Part(type, parent, localPosition, localScale, Glow(color, intensity), localEuler);
        t.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
        t.gameObject.AddComponent<NoDamageFlash>();
        return t;
    }

    public static Color Shade(Color color, float factor) {
        return new Color(color.r * factor, color.g * factor, color.b * factor, color.a);
    }

    public static Color GetBaseColor(Renderer renderer, Color fallback) {
        if (renderer != null && renderer.sharedMaterial != null && renderer.sharedMaterial.HasProperty(BaseColorId)) {
            return renderer.sharedMaterial.GetColor(BaseColorId);
        }
        return fallback;
    }

    // Orijinal tek parça görseli gizle (collider ve diğer bileşenler yerinde kalır)
    public static void HideRenderer(Transform target) {
        if (target != null && target.TryGetComponent(out MeshRenderer meshRenderer)) {
            meshRenderer.enabled = false;
        }
    }
}
