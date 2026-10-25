#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Management;
using UnityEngine.XR.Simulation;

namespace PainelAR.EditorTools
{
    // Permite testar o app no PC, sem celular: liga o XR Simulation no Play do editor
    // e gera um ambiente virtual com um painel e todos os marcadores da biblioteca da cena.
    // Rode de novo sempre que a Reference Image Library mudar.
    public static class PainelARSimulacao
    {
        const string AmbientePath = "Assets/Simulacao/AmbientePainel.prefab";
        const string PreferenciasPath = "Assets/XR/UserSimulationSettings/Resources/XRSimulationPreferences.asset";
        const float AlturaPainel = 1.4f;     // centro do painel, em metros
        const float DistanciaCamera = 0.7f;  // distância inicial entre a câmera e o painel

        [MenuItem("Painel AR/Simulação/Configurar simulação no PC")]
        public static void ConfigurarSimulacao()
        {
            AtivarLoaderNoPC();
            AtivarInputSystem();
            var ambiente = GerarAmbiente();
            var prefs = AssetDatabase.LoadMainAssetAtPath(PreferenciasPath);
            if (prefs == null) throw new InvalidOperationException("[PainelAR] Não encontrei " + PreferenciasPath);
            var so = new SerializedObject(prefs);
            so.FindProperty("m_EnvironmentPrefab").objectReferenceValue = ambiente;
            so.ApplyModifiedPropertiesWithoutUndo();
            AdicionarDiagnostico();
            AssetDatabase.SaveAssets();
            Debug.Log("[PainelAR] Simulação configurada. Aperte Play no editor para testar.");
        }

        // Diagnóstico temporário no XR Origin, até o RastreadorMarcadores da Fase 3
        static void AdicionarDiagnostico()
        {
            var scene = EditorSceneManager.OpenScene(PainelARSetup.ScenePath);
            var gerenciador = UnityEngine.Object.FindFirstObjectByType<ARTrackedImageManager>();
            if (gerenciador.GetComponent<DiagnosticoMarcadores>() == null)
                gerenciador.gameObject.AddComponent<DiagnosticoMarcadores>();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        // O Play do editor usa as configurações XR da plataforma Windows (Standalone)
        static void AtivarLoaderNoPC()
        {
            EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.k_SettingsKey, out XRGeneralSettingsPerBuildTarget porAlvo);
            if (porAlvo == null) throw new InvalidOperationException("[PainelAR] Configurações XR não encontradas");
            if (!porAlvo.HasSettingsForBuildTarget(BuildTargetGroup.Standalone)) porAlvo.CreateDefaultSettingsForBuildTarget(BuildTargetGroup.Standalone);
            if (!porAlvo.HasManagerSettingsForBuildTarget(BuildTargetGroup.Standalone)) porAlvo.CreateDefaultManagerSettingsForBuildTarget(BuildTargetGroup.Standalone);
            var pc = porAlvo.SettingsForBuildTarget(BuildTargetGroup.Standalone);
            pc.InitManagerOnStart = true;
            XRPackageMetadataStore.AssignLoader(pc.AssignedSettings, "UnityEngine.XR.Simulation.SimulationLoader", BuildTargetGroup.Standalone);
            EditorUtility.SetDirty(pc);
            EditorUtility.SetDirty(porAlvo);
        }

        // A navegação da simulação usa o Input System. "Both" mantém o OnGUI funcionando.
        // Vale a partir da próxima abertura do editor.
        static void AtivarInputSystem()
        {
            var player = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0];
            var so = new SerializedObject(player);
            var campo = so.FindProperty("activeInputHandler");
            if (campo.intValue == 2) return;
            campo.intValue = 2;
            so.ApplyModifiedPropertiesWithoutUndo();
            Debug.Log("[PainelAR] Active Input Handling = Both (reabra o editor se ele estiver aberto).");
        }

        static GameObject GerarAmbiente()
        {
            EditorSceneManager.OpenScene(PainelARSetup.ScenePath);
            var gerenciador = UnityEngine.Object.FindFirstObjectByType<ARTrackedImageManager>();
            var biblioteca = gerenciador != null ? gerenciador.referenceLibrary as XRReferenceImageLibrary : null;
            if (biblioteca == null || biblioteca.count == 0)
                throw new InvalidOperationException("[PainelAR] A cena não tem Reference Image Library com imagens");

            Directory.CreateDirectory(Path.GetDirectoryName(AmbientePath));
            var raiz = new GameObject("AmbientePainel");
            try
            {
                // SimulationEnvironment é interno no AR Foundation; adiciona pelo nome do tipo
                var tipoAmbiente = typeof(SimulatedTrackedImage).Assembly.GetType("UnityEngine.XR.Simulation.SimulationEnvironment", true);
                var ambiente = raiz.AddComponent(tipoAmbiente);
                var so = new SerializedObject(ambiente);
                var cameraPos = new Vector3(0f, AlturaPainel, -DistanciaCamera);
                so.FindProperty("m_CameraStartingPose.position").vector3Value = cameraPos;
                so.FindProperty("m_CameraStartingPose.rotation").quaternionValue = Quaternion.identity;
                so.FindProperty("m_CameraMovementBounds").boundsValue = new Bounds(new Vector3(0f, 1.5f, -1f), new Vector3(4f, 3f, 4f));
                so.FindProperty("m_DefaultViewPose.position").vector3Value = cameraPos + new Vector3(0.6f, 0.3f, -0.6f);
                so.FindProperty("m_DefaultViewPose.rotation").quaternionValue = Quaternion.Euler(15f, -35f, 0f);
                so.FindProperty("m_DefaultViewPivot").vector3Value = new Vector3(0f, AlturaPainel, 0f);
                so.FindProperty("m_DefaultViewSize").floatValue = 1.5f;
                so.ApplyModifiedPropertiesWithoutUndo();

                // Sala simples: chão, parede e o painel (caixa cinza) encostado nela
                float largura = 0f;
                for (int i = 0; i < biblioteca.count; i++) largura += Mathf.Max(biblioteca[i].width, 0.05f) + 0.08f;
                largura = Mathf.Max(largura + 0.1f, 0.5f);
                Bloco(raiz, "Chao", new Vector3(0f, -0.005f, -1f), new Vector3(4f, 0.01f, 4f), new Color(0.35f, 0.35f, 0.38f));
                Bloco(raiz, "Parede", new Vector3(0f, 1.5f, 0.2f), new Vector3(4f, 3f, 0.1f), new Color(0.85f, 0.83f, 0.78f));
                Bloco(raiz, "Painel", new Vector3(0f, AlturaPainel, 0.07f), new Vector3(largura, 0.6f, 0.14f), new Color(0.6f, 0.62f, 0.65f));

                var luz = new GameObject("Luz").AddComponent<Light>();
                luz.type = LightType.Directional;
                luz.transform.SetParent(raiz.transform, false);
                luz.transform.rotation = Quaternion.Euler(35f, 20f, 0f);

                // Um marcador por imagem da biblioteca, lado a lado na frente do painel
                float x = -largura / 2f + 0.09f;
                for (int i = 0; i < biblioteca.count; i++)
                {
                    var referencia = biblioteca[i];
                    var textura = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(referencia.textureGuid.ToString("N")));
                    if (textura == null) throw new InvalidOperationException("[PainelAR] Textura do marcador " + referencia.name + " não encontrada");
                    Vector2 tamanho = referencia.specifySize ? referencia.size : new Vector2(0.08f, 0.08f);

                    var marcador = new GameObject("Marcador_" + referencia.name);
                    marcador.transform.SetParent(raiz.transform, false);
                    marcador.transform.localPosition = new Vector3(x + tamanho.x / 2f, AlturaPainel, -0.001f);
                    // A imagem fica no plano XZ local com a frente em +Y: vira +Y para a câmera e o topo para cima
                    marcador.transform.localRotation = Quaternion.LookRotation(Vector3.up, Vector3.back);
                    marcador.AddComponent<MeshFilter>();
                    marcador.AddComponent<MeshRenderer>();
                    var simulada = marcador.AddComponent<SimulatedTrackedImage>();
                    var sm = new SerializedObject(simulada);
                    sm.FindProperty("m_Image").objectReferenceValue = textura;
                    sm.FindProperty("m_ImagePhysicalSizeMeters").vector2Value = tamanho;
                    sm.ApplyModifiedPropertiesWithoutUndo();
                    x += tamanho.x + 0.08f;
                }

                var prefab = PrefabUtility.SaveAsPrefabAsset(raiz, AmbientePath);
                Debug.Log("[PainelAR] Ambiente de simulação gerado com " + biblioteca.count + " marcador(es): " + AmbientePath);
                return prefab;
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(raiz);
            }
        }

        static void Bloco(GameObject pai, string nome, Vector3 posicao, Vector3 escala, Color cor)
        {
            var bloco = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bloco.name = nome;
            bloco.transform.SetParent(pai.transform, false);
            bloco.transform.localPosition = posicao;
            bloco.transform.localScale = escala;
            var material = new Material(Shader.Find("Standard")) { color = cor };
            Directory.CreateDirectory("Assets/Simulacao/Materiais");
            AssetDatabase.CreateAsset(material, "Assets/Simulacao/Materiais/" + nome + ".mat");
            bloco.GetComponent<Renderer>().sharedMaterial = material;
        }
    }
}
#endif
