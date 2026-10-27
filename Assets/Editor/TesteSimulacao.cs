#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Simulation;

namespace PainelAR.EditorTools
{
    // Teste automático no XR Simulation, para rodar em linha de comando (sem -quit):
    //   powershell -File tools\unity.ps1 TesteSimulacao.Executar -SemSair
    // 1. Espera os marcadores serem reconhecidos da posição inicial.
    // 2. Faz uma sessão completa como um participante: aponta a mira para o componente pedido,
    //    confirma os alertas e erra de propósito nas etapas de ErrosPropositais.
    // 3. Confere acertos, erros e o CSV, salva imagens em Logs/ e fecha o editor com código 0 (OK) ou 1.
    [InitializeOnLoad]
    public static class TesteSimulacao
    {
        const string Chave = "PainelAR.TesteSimulacao.Ativo";
        static readonly int[] ErrosPropositais = { 4, 12 };
        const double TempoMaximoReconhecimento = 20.0;
        const double EsperaAposMirar = 0.6;

        enum Passo { Reconhecendo, Mirando, Confirmando, Fim }
        static Passo _passo;
        static double _inicio, _desde;
        static string _alvo;
        static bool _imagemEtapaSalva;

        static TesteSimulacao()
        {
            if (!SessionState.GetBool(Chave, false)) return;
            EditorApplication.update += Acompanhar;
        }

        public static void Executar()
        {
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene(PainelARSetup.ScenePath);
            SessionState.SetBool(Chave, true);
            EditorApplication.EnterPlaymode();
        }

        static void Acompanhar()
        {
            if (!EditorApplication.isPlaying) return;
            double agora = EditorApplication.timeSinceStartup;
            if (_inicio == 0) { _inicio = agora; _desde = agora; }

            var gerenciador = UnityEngine.Object.FindAnyObjectByType<ARTrackedImageManager>();
            var app = UnityEngine.Object.FindAnyObjectByType<ProcedimentoController>();
            var rastreador = UnityEngine.Object.FindAnyObjectByType<RastreadorMarcadores>();
            if (gerenciador == null || app == null || rastreador == null) { Falhar("Cena sem gerenciador, App ou rastreador"); return; }
            if (!string.IsNullOrEmpty(app.ErroConfiguracao)) { Falhar("Erro de configuração: " + app.ErroConfiguracao); return; }

            switch (_passo)
            {
                case Passo.Reconhecendo:
                {
                    int esperados = gerenciador.referenceLibrary.count;
                    int visiveis = 0;
                    foreach (var i in gerenciador.trackables) if (i.trackingState == TrackingState.Tracking) visiveis++;
                    if (visiveis == esperados && agora - _inicio > 3.0)
                    {
                        Debug.Log("[PainelAR] TESTE: " + visiveis + " de " + esperados + " marcadores reconhecidos da posição inicial");
                        SalvarImagem("teste_simulacao.png");
                        app.IniciarSessao();
                        IrPara(Passo.Mirando, agora);
                    }
                    else if (agora - _inicio > TempoMaximoReconhecimento)
                        Falhar("só " + visiveis + " de " + esperados + " marcadores reconhecidos");
                    break;
                }
                case Passo.Mirando:
                {
                    if (app.FaseAtual == Fase.Concluido) { Concluir(app); return; }
                    if (app.FaseAtual == Fase.Alerta) { app.ConfirmarAlerta(); return; }
                    var etapa = app.EtapaAtual;
                    string componente = etapa.componenteId;
                    if (ErrosPropositais.Contains(etapa.numero))
                        componente = app.Conteudo.componentes.First(c => c.id != etapa.componenteId).id;
                    _alvo = app.Conteudo.BuscarPorId(componente).marcador;
                    if (!Mirar(gerenciador, _alvo)) { Falhar("marcador " + _alvo + " não está sendo rastreado"); return; }
                    IrPara(Passo.Confirmando, agora);
                    break;
                }
                case Passo.Confirmando:
                {
                    if (agora - _desde < EsperaAposMirar) return;
                    string naMira = rastreador.MarcadorNaMira();
                    if (naMira != _alvo) { Falhar("etapa " + app.EtapaAtual.numero + ": mira em " + (naMira ?? "nada") + ", esperado " + _alvo); return; }
                    if (!_imagemEtapaSalva) { SalvarImagem("teste_simulacao_etapa.png"); _imagemEtapaSalva = true; }
                    app.ConfirmarIdentificacao();
                    IrPara(Passo.Mirando, agora);
                    break;
                }
            }
        }

        static void IrPara(Passo passo, double agora) { _passo = passo; _desde = agora; }

        // Põe a câmera simulada a 40 cm do marcador, com ele exatamente na mira (62% da altura da tela)
        static bool Mirar(ARTrackedImageManager gerenciador, string marcador)
        {
            ARTrackedImage imagem = null;
            foreach (var i in gerenciador.trackables) if (i.referenceImage.name == marcador) imagem = i;
            if (imagem == null) return false;
            var cam = Camera.main;
            Vector3 alvo = imagem.transform.position;
            Vector3 posicao = alvo + imagem.transform.up * 0.40f; // +Y = frente da imagem
            float meiaFov = cam.fieldOfView * 0.5f * Mathf.Deg2Rad;
            float desvio = Mathf.Atan((RastreadorMarcadores.PosicaoMira.y / Screen.height * 2f - 1f) * Mathf.Tan(meiaFov)) * Mathf.Rad2Deg;
            var rotacao = Quaternion.LookRotation(alvo - posicao, Vector3.up) * Quaternion.Euler(desvio, 0f, 0f);

            var provedor = UnityEngine.Object.FindAnyObjectByType<SimulationCameraPoseProvider>();
            typeof(SimulationCameraPoseProvider).GetMethod("UpdatePose", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(provedor, new object[] { new Pose(posicao, rotacao) });
            return true;
        }

        static void Concluir(ProcedimentoController app)
        {
            int errosEsperados = ErrosPropositais.Length;
            var linhas = File.ReadAllLines(app.CaminhoArquivo);
            var identificacoes = linhas.Where(l => l.Split(';')[4] == "IDENTIFICACAO").ToArray();
            int zerosCsv = identificacoes.Count(l => l.Split(';')[8] == "0");
            var errosCsv = string.Join(",", identificacoes.Where(l => l.Split(';')[8] == "0").Select(l => l.Split(';')[5]));

            Debug.Log("[PainelAR] TESTE: acertos " + app.Acertos + ", erros " + app.Erros + ", taxa " + app.TaxaErros.ToString("F1") +
                "%, CSV com " + identificacoes.Length + " identificações (erros nas etapas " + errosCsv + ")");
            Directory.CreateDirectory("Logs");
            File.Copy(app.CaminhoArquivo, "Logs/teste_simulacao.csv", true);

            bool ok = app.Erros == errosEsperados && app.Acertos == app.TotalEtapas - errosEsperados
                && identificacoes.Length == app.TotalEtapas && zerosCsv == errosEsperados
                && errosCsv == string.Join(",", ErrosPropositais);
            Encerrar(ok, ok ? "sessão de " + app.TotalEtapas + " etapas registrada corretamente" : "resultado diferente do esperado");
        }

        static void Falhar(string motivo) => Encerrar(false, motivo);

        static void Encerrar(bool ok, string motivo)
        {
            EditorApplication.update -= Acompanhar;
            SessionState.EraseBool(Chave);
            Debug.Log("[PainelAR] TESTE SIMULACAO " + (ok ? "OK" : "FALHOU") + ": " + motivo);
            EditorApplication.Exit(ok ? 0 : 1);
        }

        static void SalvarImagem(string nome)
        {
            var cam = Camera.main;
            if (cam == null) return;
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
            File.WriteAllBytes("Logs/" + nome, imagem.EncodeToPNG());
        }
    }
}
#endif
