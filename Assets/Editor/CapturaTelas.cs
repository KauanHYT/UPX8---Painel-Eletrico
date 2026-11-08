#if UNITY_EDITOR
using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace PainelAR.EditorTools
{
    // Captura as telas do app no XR Simulation, em formato de celular, para docs/images.
    // Precisa do editor aberto (não roda com -batchmode), porque as telas são desenhadas na Game view.
    [InitializeOnLoad]
    public static class CapturaTelas
    {
        const string Chave = "PainelAR.CapturaTelas.Ativo";
        const string Pasta = "docs/images";
        const int Largura = 1080, Altura = 2400;
        const double Espera = 1.2;

        enum Passo { Reconhecendo, Inicio, Alerta, Etapa, SemMira, Concluindo, Fim, Sair }
        static Passo _passo;
        static double _desde;

        static CapturaTelas()
        {
            if (SessionState.GetBool(Chave, false)) EditorApplication.update += Acompanhar;
        }

        [MenuItem("Painel AR/Capturar telas do app")]
        public static void Executar()
        {
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene(PainelARSetup.ScenePath);
            AjustarGameView();
            SessionState.SetBool(Chave, true);
            EditorApplication.EnterPlaymode();
        }

        static void Acompanhar()
        {
            if (!EditorApplication.isPlaying) return;
            double agora = EditorApplication.timeSinceStartup;
            if (_desde == 0) _desde = agora;
            if (agora - _desde < Espera) return;

            var gerenciador = UnityEngine.Object.FindAnyObjectByType<ARTrackedImageManager>();
            var app = UnityEngine.Object.FindAnyObjectByType<ProcedimentoController>();
            if (gerenciador == null || app == null) return;

            switch (_passo)
            {
                case Passo.Reconhecendo:
                    int visiveis = 0;
                    foreach (var i in gerenciador.trackables) if (i.trackingState == TrackingState.Tracking) visiveis++;
                    if (visiveis == gerenciador.referenceLibrary.count) Proximo(Passo.Inicio, agora);
                    break;
                case Passo.Inicio:
                    Capturar("tela-inicial.png");
                    Proximo(Passo.Alerta, agora);
                    break;
                case Passo.Alerta:
                    if (app.FaseAtual == Fase.Inicio) { app.IniciarSessao(); _desde = agora; return; }
                    Capturar("alerta-seguranca.png");
                    Proximo(Passo.Etapa, agora);
                    break;
                case Passo.Etapa:
                    if (app.FaseAtual == Fase.Alerta) { app.ConfirmarAlerta(); TesteSimulacao.Mirar(gerenciador, "marcador_DJ1"); _desde = agora; return; }
                    Capturar("etapa-componente-na-mira.png");
                    Proximo(Passo.SemMira, agora);
                    break;
                case Passo.SemMira:
                    if (app.ComponenteNaMira != null) { MirarLonge(); _desde = agora; return; }
                    Capturar("etapa-sem-componente.png");
                    Proximo(Passo.Concluindo, agora);
                    break;
                case Passo.Concluindo:
                    if (app.FaseAtual == Fase.Concluido) { Proximo(Passo.Fim, agora); return; }
                    if (app.FaseAtual == Fase.Alerta) { app.ConfirmarAlerta(); _desde = agora - Espera; return; }
                    // Só confirma quando a mira já chegou ao componente pedido, para a sessão terminar sem erros
                    if (app.ComponenteNaMira != null && app.ComponenteNaMira.id == app.EtapaAtual.componenteId) { if (app.PodeConfirmar) app.ConfirmarIdentificacao(); }
                    else TesteSimulacao.Mirar(gerenciador, app.Conteudo.BuscarPorId(app.EtapaAtual.componenteId).marcador);
                    _desde = agora - Espera + 0.5;
                    break;
                case Passo.Fim:
                    Capturar("resumo-sessao.png");
                    Proximo(Passo.Sair, agora);
                    break;
                case Passo.Sair:
                    EditorApplication.update -= Acompanhar;
                    SessionState.EraseBool(Chave);
                    Debug.Log("[PainelAR] Telas capturadas em " + Path.GetFullPath(Pasta));
                    if (Environment.CommandLine.Contains("-executeMethod")) EditorApplication.Exit(0);
                    else EditorApplication.ExitPlaymode();
                    break;
            }
        }

        static void Proximo(Passo passo, double agora) { _passo = passo; _desde = agora; }

        static void Capturar(string nome)
        {
            Directory.CreateDirectory(Pasta);
            ScreenCapture.CaptureScreenshot(Path.Combine(Pasta, nome));
        }

        // Câmera voltada para fora do painel, sem nenhum marcador na mira
        static void MirarLonge()
        {
            var provedor = UnityEngine.Object.FindAnyObjectByType<UnityEngine.XR.Simulation.SimulationCameraPoseProvider>();
            var cam = Camera.main.transform;
            typeof(UnityEngine.XR.Simulation.SimulationCameraPoseProvider).GetMethod("UpdatePose", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(provedor, new object[] { new Pose(cam.position, cam.rotation * Quaternion.Euler(-12f, 0f, 0f)) });
        }

        // A API de tamanhos da Game view é interna: adiciona um tamanho de celular por reflexão
        static void AjustarGameView()
        {
            var assembly = typeof(EditorWindow).Assembly;
            var tipoGameView = assembly.GetType("UnityEditor.GameView");
            var janela = EditorWindow.GetWindow(tipoGameView);
            var tipoSizes = assembly.GetType("UnityEditor.GameViewSizes");
            var singleton = typeof(ScriptableSingleton<>).MakeGenericType(tipoSizes);
            var sizes = singleton.GetProperty("instance").GetValue(null);
            var grupo = tipoSizes.GetProperty("currentGroup").GetValue(sizes);
            var tipoSize = assembly.GetType("UnityEditor.GameViewSize");
            var tipoEnum = assembly.GetType("UnityEditor.GameViewSizeType");
            var tamanho = Activator.CreateInstance(tipoSize, Enum.Parse(tipoEnum, "FixedResolution"), Largura, Altura, "Celular Painel AR");
            grupo.GetType().GetMethod("AddCustomSize").Invoke(grupo, new[] { tamanho });
            int total = (int)grupo.GetType().GetMethod("GetTotalCount").Invoke(grupo, null);
            tipoGameView.GetProperty("selectedSizeIndex", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .SetValue(janela, total - 1);
            janela.Focus();
        }
    }
}
#endif
