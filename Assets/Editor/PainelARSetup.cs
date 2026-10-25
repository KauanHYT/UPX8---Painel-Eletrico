#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEditor.XR.Management;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.Management;
using Unity.XR.CoreUtils;

namespace PainelAR.EditorTools
{
    // Substitui o WL03ProjectSetup: configura o app, limpa a demonstração do WL03,
    // valida a cena e gera o APK. Tudo pode ser chamado pelo menu ou em linha de comando
    // (Unity.exe -batchmode -executeMethod PainelAR.EditorTools.PainelARSetup.<Metodo>).
    public static class PainelARSetup
    {
        public const string ScenePath = "Assets/Scenes/WL03ContextualAR.unity";
        const string ApkPath = "Builds/PainelAR.apk";

        [MenuItem("Painel AR/Configurar projeto")]
        public static void ConfigurarProjeto()
        {
            PlayerSettings.companyName = "Facens";
            PlayerSettings.productName = "Painel AR";
            PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Android, "br.edu.facens.painelar");
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };

            var scene = EditorSceneManager.OpenScene(ScenePath);
            int removidos = 0;
            foreach (var raiz in scene.GetRootGameObjects())
                foreach (var t in raiz.GetComponentsInChildren<Transform>(true))
                    removidos += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
            var overlay = GameObject.Find("WL03 Status Overlay");
            if (overlay != null) UnityEngine.Object.DestroyImmediate(overlay);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[PainelAR] Projeto configurado (" + removidos + " componente(s) do WL03 removido(s)).");
        }

        [MenuItem("Painel AR/Validar projeto")]
        public static void ValidarProjeto()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath);
            int erros = 0;
            erros += Exigir<ARSession>("AR Session");
            erros += Exigir<XROrigin>("XR Origin");
            erros += Exigir<ARTrackedImageManager>("AR Tracked Image Manager");
            erros += Exigir<ARCameraManager>("AR Camera Manager");
            erros += Exigir<ARCameraBackground>("AR Camera Background");

            foreach (var raiz in scene.GetRootGameObjects())
                foreach (var t in raiz.GetComponentsInChildren<Transform>(true))
                    if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject) > 0)
                    { Debug.LogError("[PainelAR] Script ausente em " + t.name); erros++; }
            if (GameObject.Find("WL03 Status Overlay") != null) { Debug.LogError("[PainelAR] Objeto do WL03 ainda na cena"); erros++; }

            if (!ARCoreAtivo()) { Debug.LogError("[PainelAR] ARCore Loader não habilitado para Android"); erros++; }
            if (PlayerSettings.GetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Android) != "br.edu.facens.painelar")
            { Debug.LogError("[PainelAR] Package Name incorreto"); erros++; }

            if (erros == 0) Debug.Log("[PainelAR] VALIDACAO OK");
            else throw new InvalidOperationException("[PainelAR] VALIDACAO FALHOU: " + erros + " problema(s)");
        }

        [MenuItem("Painel AR/Gerar APK")] public static void GerarApk() => Build(false);
        [MenuItem("Painel AR/Gerar APK e instalar no celular")] public static void GerarApkEInstalar() => Build(true);

        static void Build(bool instalar)
        {
            ValidarProjeto();
            Directory.CreateDirectory("Builds");
            var opcoes = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = ApkPath,
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
                options = instalar ? BuildOptions.AutoRunPlayer : BuildOptions.None
            };
            var relatorio = BuildPipeline.BuildPlayer(opcoes);
            if (relatorio.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("[PainelAR] Build falhou: " + relatorio.summary.result);
            Debug.Log("[PainelAR] APK gerado em " + Path.GetFullPath(ApkPath));
        }

        static int Exigir<T>(string nome) where T : UnityEngine.Object
        {
            if (UnityEngine.Object.FindFirstObjectByType<T>() != null) return 0;
            Debug.LogError("[PainelAR] Componente ausente: " + nome);
            return 1;
        }

        static bool ARCoreAtivo()
        {
            EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.k_SettingsKey, out XRGeneralSettingsPerBuildTarget porAlvo);
            var android = porAlvo != null ? porAlvo.SettingsForBuildTarget(BuildTargetGroup.Android) : null;
            if (android == null || android.AssignedSettings == null) return false;
            foreach (var loader in android.AssignedSettings.activeLoaders)
                if (loader != null && loader.GetType().FullName == "UnityEngine.XR.ARCore.ARCoreLoader") return true;
            return false;
        }
    }
}
#endif
