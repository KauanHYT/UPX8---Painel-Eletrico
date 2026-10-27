using UnityEngine;

namespace PainelAR
{
    public class InterfaceApp : MonoBehaviour
    {
        ProcedimentoController _app;
        GUIStyle _titulo, _tituloCentro, _texto, _pequeno, _destaque, _aviso, _botao, _painel, _painelAlerta;
        float _fps = 30f;

        float Margem => Screen.width / 30f;
        float AlturaBotao => Screen.width / 7f;

        void Start() => _app = FindAnyObjectByType<ProcedimentoController>();

        void Update() => _fps = Mathf.Lerp(_fps, 1f / Mathf.Max(Time.unscaledDeltaTime, 0.001f), 0.05f);

        void OnGUI()
        {
            if (_app == null) return;
            CriarEstilos();

            float m = Margem;
            float topo = Screen.height - Screen.safeArea.yMax + m;
            float largura = Screen.width - 2f * m;

            if (!string.IsNullOrEmpty(_app.ErroConfiguracao)) { TelaErro(m, largura); return; }

            // FPS no canto superior (RNF03)
            GUI.Label(new Rect(m, topo, largura, _pequeno.fontSize * 1.5f), _fps.ToString("F0") + " FPS", _pequeno);

            switch (_app.FaseAtual)
            {
                case Fase.Inicio: TelaInicio(m, largura); break;
                case Fase.Alerta: TelaAlerta(m, largura); break;
                case Fase.Identificacao: DesenharMira(); TelaEtapa(m, largura); break;
                case Fase.Concluido: TelaFim(m, largura); break;
            }
        }

        void TelaInicio(float m, float largura)
        {
            GUILayout.BeginArea(new Rect(m, Screen.height * 0.2f, largura, Screen.height * 0.65f));
            GUILayout.BeginVertical(_painel);
            GUILayout.Label(_app.Conteudo.titulo, _titulo);
            GUILayout.Space(m);
            GUILayout.Label("Participante:", _texto);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("-", _botao, GUILayout.Width(AlturaBotao), GUILayout.Height(AlturaBotao)) && _app.NumeroParticipante > 1)
                _app.NumeroParticipante--;
            GUILayout.Label(_app.ParticipanteId, _tituloCentro, GUILayout.Height(AlturaBotao));
            if (GUILayout.Button("+", _botao, GUILayout.Width(AlturaBotao), GUILayout.Height(AlturaBotao)))
                _app.NumeroParticipante++;
            GUILayout.EndHorizontal();
            GUILayout.Space(m);
            if (GUILayout.Button("INICIAR SESSÃO", _botao, GUILayout.Height(AlturaBotao)))
                _app.IniciarSessao();
            GUILayout.Space(m / 2f);
            GUILayout.Label(_app.TotalEtapas + " etapas. Os dados ficam salvos no aparelho.", _pequeno);
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        void TelaAlerta(float m, float largura)
        {
            var etapa = _app.EtapaAtual;
            GUILayout.BeginArea(new Rect(m, Screen.height * 0.2f, largura, Screen.height * 0.7f));
            GUILayout.BeginVertical(_painelAlerta);
            GUILayout.Label("ATENÇÃO — Etapa " + etapa.numero + " de " + _app.TotalEtapas, _titulo);
            GUILayout.Space(m / 2f);
            GUILayout.Label(etapa.alerta, _texto);
            GUILayout.Space(m);
            if (GUILayout.Button("LI E ENTENDI", _botao, GUILayout.Height(AlturaBotao)))
                _app.ConfirmarAlerta();
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        void TelaEtapa(float m, float largura)
        {
            var etapa = _app.EtapaAtual;
            var naMira = _app.ComponenteNaMira;

            GUILayout.BeginArea(new Rect(m, Screen.height * 0.45f, largura, Screen.height * 0.55f - m));
            GUILayout.FlexibleSpace(); // empurra o painel para baixo
            GUILayout.BeginVertical(_painel);
            GUILayout.Label("Etapa " + etapa.numero + " de " + _app.TotalEtapas, _pequeno);
            GUILayout.Label(etapa.titulo, _titulo);
            GUILayout.Label(etapa.instrucao, _texto);
            GUILayout.Space(m / 2f);

            if (naMira != null)
            {
                GUILayout.Label("Na mira: " + naMira.nome, _destaque);
                GUILayout.Label(naMira.funcao, _pequeno);
                if (!string.IsNullOrEmpty(naMira.alerta)) GUILayout.Label("Segurança: " + naMira.alerta, _aviso);
            }
            else
            {
                GUILayout.Label("Na mira: nenhum componente", _pequeno);
            }

            if (!string.IsNullOrEmpty(_app.Mensagem)) GUILayout.Label(_app.Mensagem, _aviso);

            GUILayout.Space(m / 2f);
            GUI.enabled = naMira != null;
            if (GUILayout.Button("CONFIRMAR COMPONENTE", _botao, GUILayout.Height(AlturaBotao)))
                _app.ConfirmarIdentificacao();
            GUI.enabled = true;
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        void TelaFim(float m, float largura)
        {
            GUILayout.BeginArea(new Rect(m, Screen.height * 0.15f, largura, Screen.height * 0.75f));
            GUILayout.BeginVertical(_painel);
            GUILayout.Label("Sessão concluída", _titulo);
            GUILayout.Label("Participante: " + _app.ParticipanteId, _texto);
            GUILayout.Label("Acertos: " + _app.Acertos + "   Erros: " + _app.Erros, _texto);
            GUILayout.Label("Taxa de erros: " + _app.TaxaErros.ToString("F1") + "%", _texto);
            GUILayout.Label("Tempo: " + _app.TempoTotal.ToString("F0") + " s", _texto);
            GUILayout.Space(m / 2f);
            GUILayout.Label("Arquivo salvo em:\n" + _app.CaminhoArquivo, _pequeno);
            GUILayout.Space(m);
            if (GUILayout.Button("PRÓXIMO PARTICIPANTE", _botao, GUILayout.Height(AlturaBotao)))
                _app.ProximoParticipante();
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        void TelaErro(float m, float largura)
        {
            GUILayout.BeginArea(new Rect(m, Screen.height * 0.2f, largura, Screen.height * 0.6f));
            GUILayout.BeginVertical(_painelAlerta);
            GUILayout.Label("Erro de configuração", _titulo);
            GUILayout.Label(_app.ErroConfiguracao, _texto);
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        void DesenharMira()
        {
            Vector2 p = RastreadorMarcadores.PosicaoMira;
            float x = p.x, y = Screen.height - p.y; // a tela do GUI conta o Y de cima para baixo
            float tamanho = Screen.width / 12f, espessura = Mathf.Max(3f, Screen.width / 200f);
            var corAnterior = GUI.color;
            GUI.color = _app.ComponenteNaMira != null ? Color.green : Color.white;
            GUI.DrawTexture(new Rect(x - tamanho, y - espessura / 2f, 2f * tamanho, espessura), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(x - espessura / 2f, y - tamanho, espessura, 2f * tamanho), Texture2D.whiteTexture);
            GUI.color = corAnterior;
        }

        void CriarEstilos()
        {
            if (_titulo != null) return;
            int b = Screen.width;
            _titulo = new GUIStyle(GUI.skin.label) { fontSize = b / 17, fontStyle = FontStyle.Bold, wordWrap = true };
            _titulo.normal.textColor = Color.white;
            _tituloCentro = new GUIStyle(_titulo) { alignment = TextAnchor.MiddleCenter, wordWrap = false };
            _texto = new GUIStyle(_titulo) { fontSize = b / 24, fontStyle = FontStyle.Normal };
            _pequeno = new GUIStyle(_texto) { fontSize = b / 32 };
            _destaque = new GUIStyle(_texto) { fontStyle = FontStyle.Bold };
            _destaque.normal.textColor = new Color(0.55f, 1f, 0.55f);
            _aviso = new GUIStyle(_pequeno) { fontStyle = FontStyle.Bold };
            _aviso.normal.textColor = new Color(1f, 0.75f, 0.3f);
            _botao = new GUIStyle(GUI.skin.button) { fontSize = b / 20, fontStyle = FontStyle.Bold };
            int p = b / 30;
            _painel = new GUIStyle { padding = new RectOffset(p, p, p, p) };
            _painel.normal.background = Fundo(new Color(0.05f, 0.08f, 0.12f, 0.92f));
            _painelAlerta = new GUIStyle(_painel);
            _painelAlerta.normal.background = Fundo(new Color(0.55f, 0.05f, 0.05f, 0.95f));
        }

        static Texture2D Fundo(Color cor)
        {
            var t = new Texture2D(1, 1);
            t.SetPixel(0, 0, cor);
            t.Apply();
            return t;
        }
    }
}
