using System;
using UnityEngine;

namespace PainelAR
{
    public enum Fase { Inicio, Alerta, Identificacao, Concluido }

    public class ProcedimentoController : MonoBehaviour
    {
        [Tooltip("Se marcado, o componente pedido na etapa fica destacado em laranja na RA.")]
        [SerializeField] bool destacarComponenteDaEtapa = true;

        [Tooltip("Tempo, em segundos, que um componente precisa ficar na mira para gerar INFO_EXIBIDA no CSV.")]
        [SerializeField] float tempoMinimoNaMira = 0.3f;

        public ConteudoPainel Conteudo { get; private set; }
        public string ErroConfiguracao { get; private set; }
        public Fase FaseAtual { get; private set; } = Fase.Inicio;
        public int NumeroParticipante
        {
            get => _numeroParticipante;
            set { _numeroParticipante = value; ConjuntoId = ConjuntoSugerido(value); }
        }
        public string ConjuntoId { get; set; } = "A";
        public string ParticipanteId => "P" + NumeroParticipante.ToString("00");
        public int IndiceEtapa { get; private set; }
        public Etapa[] Etapas => Conteudo.BuscarConjunto(ConjuntoId).etapas;
        public Etapa EtapaAtual => Etapas[IndiceEtapa];
        public int TotalEtapas => Etapas.Length;
        public int Acertos { get; private set; }
        public int Erros { get; private set; }
        public float TaxaErros => TotalEtapas == 0 ? 0f : 100f * Erros / TotalEtapas;
        public float TempoTotal { get; private set; }
        public string Mensagem { get; private set; } = "";
        public string CaminhoArquivo => _registro != null ? _registro.CaminhoArquivo : "";
        public Componente ComponenteNaMira { get; private set; }

        // Rodízio de 4 participantes: alterna a ordem das condições e o conjunto usado em cada uma.
        // P01 RA(A) e depois papel(B), P02 papel(B) e depois RA(A), P03 RA(B) e depois papel(A), P04 papel(A) e depois RA(B).
        public static string ConjuntoSugerido(int participante) => (participante - 1) / 2 % 2 == 0 ? "A" : "B";
        public static bool RaPrimeiro(int participante) => participante % 2 == 1;

        int _numeroParticipante = 1;
        RastreadorMarcadores _rastreador;
        RegistroExperimento _registro;
        string _ultimoMarcadorNaMira;
        string _candidatoInfo;   // marcador esperando completar o tempo mínimo na mira
        float _candidatoDesde;
        string _ultimaInfoRegistrada;

        void Awake()
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep; // tela não apaga durante a sessão

            try { Conteudo = ConteudoPainel.Carregar(); }
            catch (Exception ex) { ErroConfiguracao = ex.Message; Debug.LogError("[PainelAR] " + ex.Message); return; }

            _rastreador = FindAnyObjectByType<RastreadorMarcadores>();
            if (_rastreador == null)
            {
                ErroConfiguracao = "Falta o componente Rastreador Marcadores no XR Origin.";
                Debug.LogError("[PainelAR] " + ErroConfiguracao);
                return;
            }

            _rastreador.TextoDoMarcador = NomeDoComponente;
            _rastreador.MarcadorFicouVisivel = nome =>
            {
                if (_registro != null && FaseAtual != Fase.Concluido)
                    _registro.Registrar("MARCADOR_VISIVEL", EtapaAtual.numero, detalhe: nome);
            };
        }

        string NomeDoComponente(string marcador)
        {
            var c = Conteudo.BuscarPorMarcador(marcador);
            return c != null ? c.nome : "Sem cadastro: " + marcador;
        }

        public void IniciarSessao()
        {
            _registro = new RegistroExperimento(ParticipanteId, ConjuntoId);
            _registro.Registrar("SESSAO_INICIO", detalhe: "conteudo " + Conteudo.versao);
            IndiceEtapa = 0;
            Acertos = 0;
            Erros = 0;
            TempoTotal = 0f;
            ComecarEtapa();
        }

        void ComecarEtapa()
        {
            var etapa = EtapaAtual;
            Mensagem = "";
            _candidatoInfo = null;
            _ultimaInfoRegistrada = null;
            _rastreador.MarcadorDestacado = destacarComponenteDaEtapa ? Conteudo.BuscarPorId(etapa.componenteId).marcador : null;
            _registro.Registrar("ETAPA_INICIO", etapa.numero, etapa.componenteId);

            if (!string.IsNullOrEmpty(etapa.alerta))
            {
                FaseAtual = Fase.Alerta;
                _registro.Registrar("ALERTA_EXIBIDO", etapa.numero, etapa.componenteId, detalhe: etapa.alerta);
            }
            else
            {
                FaseAtual = Fase.Identificacao;
            }
        }

        public void ConfirmarAlerta()
        {
            _registro.Registrar("ALERTA_CONFIRMADO", EtapaAtual.numero, EtapaAtual.componenteId);
            FaseAtual = Fase.Identificacao;
        }

        public void ConfirmarIdentificacao()
        {
            var etapa = EtapaAtual;
            string marcador = _rastreador.MarcadorNaMira();
            if (marcador == null)
            {
                Mensagem = "Nenhum marcador na mira. Aponte a cruz para o componente.";
                _registro.Registrar("CONFIRMACAO_SEM_MARCADOR", etapa.numero, etapa.componenteId);
                return; // não conta como identificação
            }

            var indicado = Conteudo.BuscarPorMarcador(marcador);
            string idIndicado = indicado != null ? indicado.id : marcador;
            bool correto = idIndicado == etapa.componenteId;
            if (correto) Acertos++; else Erros++;
            _registro.Registrar("IDENTIFICACAO", etapa.numero, etapa.componenteId, idIndicado, correto ? "1" : "0");

            ProximaEtapa();
        }

        void ProximaEtapa()
        {
            if (IndiceEtapa < TotalEtapas - 1)
            {
                IndiceEtapa++;
                ComecarEtapa();
                return;
            }

            FaseAtual = Fase.Concluido;
            TempoTotal = _registro.TempoDecorrido;
            _rastreador.MarcadorDestacado = null;
            _registro.Registrar("SESSAO_FIM", detalhe: "acertos " + Acertos + ", erros " + Erros + ", taxa de erros " + TaxaErros.ToString("F1") + "%");
        }

        public void ProximoParticipante()
        {
            _registro = null;
            NumeroParticipante++;
            FaseAtual = Fase.Inicio;
        }

        void Update()
        {
            if (ErroConfiguracao != null) return;
            if (FaseAtual != Fase.Identificacao)
            {
                ComponenteNaMira = null;
                _ultimoMarcadorNaMira = null;
                return;
            }

            // A tela acompanha a mira na hora (RF04)
            string marcador = _rastreador.MarcadorNaMira();
            if (marcador != _ultimoMarcadorNaMira)
            {
                _ultimoMarcadorNaMira = marcador;
                ComponenteNaMira = marcador != null ? Conteudo.BuscarPorMarcador(marcador) : null;
            }
            if (marcador == null) return; // falha breve de rastreamento não reinicia a contagem

            // O CSV só registra uma consulta real: o componente mudou e ficou na mira pelo tempo
            // mínimo. Sem isso, cada tremida da câmera vira uma linha nova.
            if (marcador != _candidatoInfo)
            {
                _candidatoInfo = marcador;
                _candidatoDesde = Time.time;
            }
            if (marcador != _ultimaInfoRegistrada && Time.time - _candidatoDesde >= tempoMinimoNaMira)
            {
                _ultimaInfoRegistrada = marcador;
                _registro.Registrar("INFO_EXIBIDA", EtapaAtual.numero, EtapaAtual.componenteId, ComponenteNaMira != null ? ComponenteNaMira.id : marcador);
            }
        }
    }
}
