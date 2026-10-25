#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace PainelAR.EditorTools
{
    // Teste automático da simulação, para rodar em linha de comando (sem -quit):
    //   Unity.exe -batchmode -projectPath . -executeMethod PainelAR.EditorTools.TesteSimulacao.Executar
    // Entra em Play, espera os marcadores serem reconhecidos, salva Logs/teste_simulacao.png e fecha o editor
    // com código 0 (todos reconhecidos) ou 1.
    [InitializeOnLoad]
    public static class TesteSimulacao
    {
        const string Chave = "PainelAR.TesteSimulacao.Ativo";
        const double TempoMaximo = 20.0;
        static double _inicio;

        static TesteSimulacao()
        {
            if (!SessionState.GetBool(Chave, false)) return;
            EditorApplication.update += Acompanhar;
        }

        public static void Executar()
        {
            EditorSceneManagerAbrir();
            SessionState.SetBool(Chave, true);
            EditorApplication.EnterPlaymode();
        }

        static void EditorSceneManagerAbrir() => UnityEditor.SceneManagement.EditorSceneManager.OpenScene(PainelARSetup.ScenePath);

        static void Acompanhar()
        {
            if (!EditorApplication.isPlaying) return;
            if (_inicio == 0) _inicio = EditorApplication.timeSinceStartup;
            double decorrido = EditorApplication.timeSinceStartup - _inicio;

            var gerenciador = Object.FindFirstObjectByType<ARTrackedImageManager>();
            int esperados = gerenciador != null && gerenciador.referenceLibrary != null ? gerenciador.referenceLibrary.count : -1;
            int visiveis = 0;
            if (gerenciador != null)
                foreach (var imagem in gerenciador.trackables)
                    if (imagem.trackingState == TrackingState.Tracking) visiveis++;

            // Espera 3 s com tudo reconhecido para as placas de diagnóstico aparecerem na imagem
            bool ok = esperados > 0 && visiveis == esperados;
            if (!(ok && decorrido > 5.0) && decorrido < TempoMaximo) return;

            EditorApplication.update -= Acompanhar;
            SessionState.EraseBool(Chave);
            var loader = UnityEngine.XR.Management.XRGeneralSettings.Instance?.Manager?.activeLoader;
            var sub = gerenciador != null ? gerenciador.subsystem : null;
            Debug.Log("[PainelAR] Diagnóstico: loader=" + (loader != null ? loader.name : "nenhum")
                + " subsistema=" + (sub != null ? sub.subsystemDescriptor.id + " rodando=" + sub.running : "nenhum")
                + " camera=" + (Camera.main != null ? Camera.main.transform.position.ToString("F2") : "?")
                + " frames=" + Time.frameCount);
            SalvarImagem();
            Debug.Log("[PainelAR] TESTE SIMULACAO " + (ok ? "OK" : "FALHOU") + ": " + visiveis + " de " + esperados + " marcador(es) visíveis");
            EditorApplication.Exit(ok ? 0 : 1);
        }

        static void SalvarImagem()
        {
            var cam = Camera.main;
            if (cam == null) { Debug.LogError("[PainelAR] Sem Main Camera para a imagem do teste"); return; }
            // Mesma proporção da tela do Play, senão o fundo da câmera fica esticado em relação à RA
            var rt = new RenderTexture(Mathf.Max(cam.pixelWidth, 64), Mathf.Max(cam.pixelHeight, 64), 24);
            cam.targetTexture = rt;
            cam.Render();
            RenderTexture.active = rt;
            var imagem = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
            imagem.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            imagem.Apply();
            cam.targetTexture = null;
            RenderTexture.active = null;
            Directory.CreateDirectory("Logs");
            File.WriteAllBytes("Logs/teste_simulacao.png", imagem.EncodeToPNG());
            Debug.Log("[PainelAR] Imagem do teste salva em " + Path.GetFullPath("Logs/teste_simulacao.png"));
        }
    }
}
#endif
