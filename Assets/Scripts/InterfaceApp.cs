using UnityEngine;

namespace PainelAR
{
    // Telas do app, desenhadas em IMGUI. Só apresenta: as regras do procedimento ficam no ProcedimentoController.
    public class InterfaceApp : MonoBehaviour
    {
        static readonly Color CorFundo = new Color(0.07f, 0.10f, 0.15f, 0.94f);
        static readonly Color CorSuperficie = new Color(0.15f, 0.20f, 0.27f, 1f);
        static readonly Color CorAzul = new Color(0.16f, 0.50f, 0.93f, 1f);
        static readonly Color CorVerde = new Color(0.13f, 0.66f, 0.42f, 1f);
        static readonly Color CorAmbar = new Color(0.98f, 0.71f, 0.20f, 1f);
        static readonly Color CorVermelho = new Color(0.86f, 0.25f, 0.25f, 1f);
        static readonly Color CorAlerta = new Color(0.36f, 0.08f, 0.09f, 0.97f);
        static readonly Color CorTextoSuave = new Color(0.69f, 0.75f, 0.83f, 1f);
        static readonly Color CorEscuro = new Color(0.10f, 0.08f, 0.04f, 1f);

        const float FpsMinimo = 15f; // RNF03

        ProcedimentoController _app;
        GUIStyle _titulo, _tituloCentro, _subtitulo, _texto, _pequeno, _pequenoCentro, _rotulo, _aviso, _valor, _valorErro, _valorOk;
        GUIStyle _cartao, _cartaoAlerta, _superficie, _botaoPrimario, _botaoConfirmar, _botaoInativo, _botaoSecundario, _botaoAlerta;
        GUIStyle _pilulaOk, _pilulaNeutra, _chipOk, _chipRuim, _trilha, _barra, _simbolo;
        Texture2D _escurecer;
        float _fps = 30f;

        float Margem => Screen.width / 26f;
        float Respiro => Screen.width / 24f;
        float AlturaBotao => Screen.width / 7.5f;
        float Largura => Screen.width - 2f * Margem;
        float Topo => Screen.height - Screen.safeArea.yMax + Margem;
        float Base => Screen.height - Screen.safeArea.yMin - Margem;

        void Start() => _app = FindAnyObjectByType<ProcedimentoController>();

        void Update() => _fps = Mathf.Lerp(_fps, 1f / Mathf.Max(Time.unscaledDeltaTime, 0.001f), 0.05f);

        void OnGUI()
        {
            if (_app == null) return;
            CriarEstilos();

            if (!string.IsNullOrEmpty(_app.ErroConfiguracao)) { TelaErro(); return; }

            switch (_app.FaseAtual)
            {
                case Fase.Inicio: TelaInicio(); break;
                case Fase.Alerta: TelaAlerta(); break;
                case Fase.Identificacao: DesenharMira(); TelaEtapa(); break;
                case Fase.Concluido: TelaFim(); break;
            }

            BarraSuperior();
        }

        // Progresso da sessão e FPS (RNF03), sempre no mesmo lugar
        void BarraSuperior()
        {
            float y = Topo;
            string fps = _fps.ToString("F0") + " FPS";
            var estilo = _fps >= FpsMinimo ? _chipOk : _chipRuim;
            float alturaChip = Screen.width / 13f;
            float larguraChip = estilo.CalcSize(new GUIContent(fps)).x + Respiro;

            bool emSessao = _app.FaseAtual == Fase.Alerta || _app.FaseAtual == Fase.Identificacao;
            if (emSessao)
            {
                float altura = alturaChip + 2f * Respiro * 0.6f + Screen.width / 60f;
                GUI.Box(new Rect(Margem, y, Largura, altura), GUIContent.none, _cartao);
                float x = Margem + Respiro;
                float largura = Largura - 2f * Respiro;
                float linha = y + Respiro * 0.5f;
                GUI.Label(new Rect(x, linha, largura - larguraChip, alturaChip),
                    "Etapa " + _app.EtapaAtual.numero + " de " + _app.TotalEtapas + "  ·  " + _app.ParticipanteId + "  ·  Conjunto " + _app.ConjuntoId, _rotulo);
                GUI.Label(new Rect(x + largura - larguraChip, linha, larguraChip, alturaChip), fps, estilo);

                float alturaBarra = Screen.width / 70f;
                var trilha = new Rect(x, linha + alturaChip + Respiro * 0.3f, largura, alturaBarra);
                GUI.Box(trilha, GUIContent.none, _trilha);
                float progresso = (float)_app.EtapaAtual.numero / _app.TotalEtapas;
                GUI.Box(new Rect(trilha.x, trilha.y, Mathf.Max(trilha.width * progresso, alturaBarra), alturaBarra), GUIContent.none, _barra);
            }
            else
            {
                GUI.Label(new Rect(Screen.width - Margem - larguraChip, y, larguraChip, alturaChip), fps, estilo);
            }
        }

        void TelaInicio()
        {
            CentroInicio();
            GUILayout.BeginVertical(_cartao);
            GUILayout.Label("PAINEL AR", _rotulo);
            GUILayout.Label(_app.Conteudo.titulo, _titulo);
            GUILayout.Space(Respiro);

            GUILayout.Label("Participante", _subtitulo);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("−", _botaoSecundario, GUILayout.Width(AlturaBotao), GUILayout.Height(AlturaBotao)) && _app.NumeroParticipante > 1)
                _app.NumeroParticipante--;
            GUILayout.Label(_app.ParticipanteId, _tituloCentro, GUILayout.Height(AlturaBotao));
            if (GUILayout.Button("+", _botaoSecundario, GUILayout.Width(AlturaBotao), GUILayout.Height(AlturaBotao)))
                _app.NumeroParticipante++;
            GUILayout.EndHorizontal();
            GUILayout.Space(Respiro);

            GUILayout.Label("Conjunto de etapas", _subtitulo);
            GUILayout.BeginHorizontal();
            BotaoConjunto("A");
            GUILayout.Space(Respiro / 2f);
            BotaoConjunto("B");
            GUILayout.EndHorizontal();
            GUILayout.Space(Respiro / 2f);
            int n = _app.NumeroParticipante;
            GUILayout.Label("Rodízio: " + _app.ParticipanteId + " usa o conjunto " + ProcedimentoController.ConjuntoSugerido(n) +
                " no app, " + (ProcedimentoController.RaPrimeiro(n) ? "RA primeiro." : "papel primeiro."), _pequeno);
            GUILayout.Space(Respiro);

            if (GUILayout.Button("INICIAR SESSÃO", _botaoPrimario, GUILayout.Height(AlturaBotao)))
                _app.IniciarSessao();
            GUILayout.Space(Respiro / 2f);
            GUILayout.Label(_app.TotalEtapas + " etapas. Os dados ficam salvos no aparelho.", _pequenoCentro);
            GUILayout.EndVertical();
            FimCentro();
        }

        void BotaoConjunto(string id)
        {
            var estilo = _app.ConjuntoId == id ? _botaoPrimario : _botaoSecundario;
            if (GUILayout.Button(id, estilo, GUILayout.Height(AlturaBotao))) _app.ConjuntoId = id;
        }

        void TelaAlerta()
        {
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), _escurecer);
            var etapa = _app.EtapaAtual;
            CentroInicio();
            GUILayout.BeginVertical(_cartaoAlerta);
            float tamanho = Screen.width / 6f;
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("!", _simbolo, GUILayout.Width(tamanho), GUILayout.Height(tamanho));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(Respiro / 2f);
            GUILayout.Label("ATENÇÃO", _tituloCentro);
            GUILayout.Label("Etapa " + etapa.numero + " de " + _app.TotalEtapas, _pequenoCentro);
            GUILayout.Space(Respiro);
            GUILayout.Label(etapa.alerta, _texto);
            GUILayout.Space(Respiro);
            if (GUILayout.Button("LI E ENTENDI", _botaoAlerta, GUILayout.Height(AlturaBotao)))
                _app.ConfirmarAlerta();
            GUILayout.EndVertical();
            FimCentro();
        }

        // Layout manual: o botão fica sempre no mesmo lugar e o cartão não muda de altura com a mira
        void TelaEtapa()
        {
            var etapa = _app.EtapaAtual;
            var naMira = _app.ComponenteNaMira;
            float p = Respiro, largura = Largura - 2f * p, espaco = p / 2f;

            float altTitulo = Altura(_titulo, etapa.titulo, largura);
            float altInstrucao = Altura(_texto, etapa.instrucao, largura);
            float altPilula = Screen.width / 9f;
            float altMensagem = string.IsNullOrEmpty(_app.Mensagem) ? 0f : Altura(_aviso, _app.Mensagem, largura) + espaco;
            float altDetalhe = AlturaDetalhes(largura, espaco);

            float fixo = 2f * p + altTitulo + espaco + altInstrucao + p + altPilula + altMensagem + p + AlturaBotao;
            // A mira fica a 38% da altura: o cartão não pode subir até ela
            bool semDetalhes = Base - fixo - altDetalhe < Screen.height * 0.47f;
            if (semDetalhes) altDetalhe = 0f;

            float alturaCartao = fixo + altDetalhe;
            float y = Base - alturaCartao;
            GUI.Box(new Rect(Margem, y, Largura, alturaCartao), GUIContent.none, _cartao);

            float x = Margem + p;
            y += p;
            GUI.Label(new Rect(x, y, largura, altTitulo), etapa.titulo, _titulo);
            y += altTitulo + espaco;
            GUI.Label(new Rect(x, y, largura, altInstrucao), etapa.instrucao, _texto);
            y += altInstrucao + p;

            GUI.Label(new Rect(x, y, largura, altPilula), naMira != null ? "Na mira: " + naMira.nome : "Na mira: nenhum componente",
                naMira != null ? _pilulaOk : _pilulaNeutra);
            y += altPilula;

            if (!semDetalhes && naMira != null)
            {
                float yDetalhe = y + espaco;
                float altFuncao = Altura(_pequeno, naMira.funcao, largura);
                GUI.Label(new Rect(x, yDetalhe, largura, altFuncao), naMira.funcao, _pequeno);
                yDetalhe += altFuncao;
                if (!string.IsNullOrEmpty(naMira.alerta))
                {
                    string aviso = "Segurança: " + naMira.alerta;
                    GUI.Label(new Rect(x, yDetalhe + espaco / 2f, largura, Altura(_aviso, aviso, largura)), aviso, _aviso);
                }
            }
            else if (!semDetalhes)
            {
                const string dica = "Aponte a mira para o marcador ao lado do componente.";
                GUI.Label(new Rect(x, y + espaco, largura, Altura(_pequenoCentro, dica, largura)), dica, _pequenoCentro);
            }
            y += altDetalhe;

            if (altMensagem > 0f)
            {
                GUI.Label(new Rect(x, y + espaco, largura, altMensagem - espaco), _app.Mensagem, _aviso);
                y += altMensagem;
            }

            var botao = new Rect(x, y + p, largura, AlturaBotao);
            if (naMira != null && _app.PodeConfirmar)
            {
                if (GUI.Button(botao, "CONFIRMAR COMPONENTE", _botaoConfirmar)) _app.ConfirmarIdentificacao();
            }
            else if (naMira != null)
            {
                // Botão enchendo de verde enquanto a mira fica parada; o toque antes disso é recusado e registrado
                if (GUI.Button(botao, "SEGURE A MIRA...", _botaoInativo)) _app.ConfirmarIdentificacao();
                GUI.BeginGroup(new Rect(botao.x, botao.y, botao.width * _app.ProgressoConfirmacao, botao.height));
                GUI.Label(new Rect(0f, 0f, botao.width, botao.height), "SEGURE A MIRA...", _botaoConfirmar);
                GUI.EndGroup();
            }
            else
            {
                GUI.Label(botao, "CONFIRMAR COMPONENTE", _botaoInativo);
            }
        }

        // Maior altura de função e alerta entre os componentes
        float AlturaDetalhes(float largura, float espaco)
        {
            float maior = 0f;
            foreach (var c in _app.Conteudo.componentes)
            {
                float altura = espaco + Altura(_pequeno, c.funcao, largura);
                if (!string.IsNullOrEmpty(c.alerta)) altura += espaco / 2f + Altura(_aviso, "Segurança: " + c.alerta, largura);
                maior = Mathf.Max(maior, altura);
            }
            return maior;
        }

        void TelaFim()
        {
            CentroInicio();
            GUILayout.BeginVertical(_cartao);
            GUILayout.Label("SESSÃO CONCLUÍDA", _rotulo);
            GUILayout.Label(_app.ParticipanteId + "  ·  Conjunto " + _app.ConjuntoId, _titulo);
            GUILayout.Space(Respiro);

            GUILayout.BeginHorizontal();
            Indicador(_app.Acertos.ToString(), "Acertos", _valorOk);
            GUILayout.Space(Respiro / 2f);
            Indicador(_app.Erros.ToString(), "Erros", _app.Erros > 0 ? _valorErro : _valor);
            GUILayout.EndHorizontal();
            GUILayout.Space(Respiro / 2f);
            GUILayout.BeginHorizontal();
            Indicador(_app.TaxaErros.ToString("F1") + "%", "Taxa de erros", _valor);
            GUILayout.Space(Respiro / 2f);
            Indicador(_app.TempoTotal.ToString("F0") + " s", "Tempo", _valor);
            GUILayout.EndHorizontal();

            GUILayout.Space(Respiro);
            GUILayout.Label("Arquivo salvo em:\n" + _app.CaminhoArquivo, _pequeno);
            GUILayout.Space(Respiro);
            if (GUILayout.Button("PRÓXIMO PARTICIPANTE", _botaoPrimario, GUILayout.Height(AlturaBotao)))
                _app.ProximoParticipante();
            GUILayout.EndVertical();
            FimCentro();
        }

        void Indicador(string valor, string nome, GUIStyle estilo)
        {
            float largura = (Largura - 2f * Respiro - Respiro / 2f) / 2f; // dois por linha, sempre do mesmo tamanho
            GUILayout.BeginVertical(_superficie, GUILayout.Width(largura));
            GUILayout.Label(valor, estilo);
            GUILayout.Label(nome, _pequenoCentro);
            GUILayout.EndVertical();
        }

        void TelaErro()
        {
            CentroInicio();
            GUILayout.BeginVertical(_cartaoAlerta);
            GUILayout.Label("Erro de configuração", _titulo);
            GUILayout.Space(Respiro / 2f);
            GUILayout.Label(_app.ErroConfiguracao, _texto);
            GUILayout.EndVertical();
            FimCentro();
        }

        void CentroInicio()
        {
            GUILayout.BeginArea(new Rect(Margem, Topo, Largura, Base - Topo));
            GUILayout.FlexibleSpace();
        }

        void FimCentro()
        {
            GUILayout.FlexibleSpace();
            GUILayout.EndArea();
        }

        // Cantos de um quadrado com um ponto no centro; verde quando há componente na mira
        void DesenharMira()
        {
            Vector2 p = RastreadorMarcadores.PosicaoMira;
            float x = p.x, y = Screen.height - p.y; // a tela do GUI conta o Y de cima para baixo
            float meio = Screen.width / 9f, braco = meio * 0.42f, espessura = Mathf.Max(4f, Screen.width / 150f);
            var corAnterior = GUI.color;
            Cantos(x + 2f, y + 2f, meio, braco, espessura, new Color(0f, 0f, 0f, 0.45f));
            Cantos(x, y, meio, braco, espessura, _app.ComponenteNaMira != null ? new Color(0.3f, 0.95f, 0.6f) : Color.white);
            GUI.color = corAnterior;
        }

        void Cantos(float x, float y, float meio, float braco, float e, Color cor)
        {
            GUI.color = cor;
            foreach (float sx in new[] { -1f, 1f })
            {
                foreach (float sy in new[] { -1f, 1f })
                {
                    float cx = x + sx * meio, cy = y + sy * meio;
                    GUI.DrawTexture(new Rect(sx < 0f ? cx : cx - braco, sy < 0f ? cy : cy - e, braco, e), Texture2D.whiteTexture);
                    GUI.DrawTexture(new Rect(sx < 0f ? cx : cx - e, sy < 0f ? cy : cy - braco, e, braco), Texture2D.whiteTexture);
                }
            }
            GUI.DrawTexture(new Rect(x - e, y - e, 2f * e, 2f * e), Texture2D.whiteTexture);
        }

        static float Altura(GUIStyle estilo, string texto, float largura) => estilo.CalcHeight(new GUIContent(texto), largura);

        void CriarEstilos()
        {
            if (_titulo != null) return;
            int b = Screen.width;
            int p = Mathf.RoundToInt(Respiro);
            int raio = b / 30, raioBotao = b / 40;

            _texto = new GUIStyle(GUI.skin.label) { fontSize = b / 24, wordWrap = true, padding = new RectOffset(), margin = new RectOffset() };
            _texto.normal.textColor = Color.white;
            _titulo = new GUIStyle(_texto) { fontSize = b / 17, fontStyle = FontStyle.Bold };
            _tituloCentro = new GUIStyle(_titulo) { alignment = TextAnchor.MiddleCenter, wordWrap = false };
            _subtitulo = new GUIStyle(_texto) { fontSize = b / 27, fontStyle = FontStyle.Bold, padding = new RectOffset(0, 0, 0, p / 3) };
            _subtitulo.normal.textColor = CorTextoSuave;
            _pequeno = new GUIStyle(_texto) { fontSize = b / 31 };
            _pequeno.normal.textColor = CorTextoSuave;
            _pequenoCentro = new GUIStyle(_pequeno) { alignment = TextAnchor.MiddleCenter };
            _rotulo = new GUIStyle(_pequeno) { fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft, wordWrap = false };
            _aviso = new GUIStyle(_pequeno) { fontStyle = FontStyle.Bold };
            _aviso.normal.textColor = CorAmbar;
            _valor = new GUIStyle(_titulo) { fontSize = b / 13, alignment = TextAnchor.MiddleCenter, wordWrap = false };
            _valorOk = new GUIStyle(_valor);
            _valorOk.normal.textColor = new Color(0.35f, 0.90f, 0.62f);
            _valorErro = new GUIStyle(_valor);
            _valorErro.normal.textColor = new Color(1f, 0.50f, 0.50f);

            _cartao = Caixa(CorFundo, raio, p);
            _cartaoAlerta = Caixa(CorAlerta, raio, p);
            _superficie = Caixa(CorSuperficie, raioBotao, p / 2);

            _botaoPrimario = Botao(CorAzul, Color.white, raioBotao);
            _botaoConfirmar = Botao(CorVerde, Color.white, raioBotao);
            _botaoSecundario = Botao(CorSuperficie, Color.white, raioBotao);
            _botaoAlerta = Botao(CorAmbar, CorEscuro, raioBotao);
            _botaoInativo = Botao(CorSuperficie, new Color(0.50f, 0.56f, 0.64f), raioBotao);

            _pilulaOk = Pilula(new Color(0.09f, 0.36f, 0.25f, 1f), new Color(0.55f, 1f, 0.75f), raioBotao);
            _pilulaNeutra = Pilula(CorSuperficie, CorTextoSuave, raioBotao);
            _chipOk = Chip(new Color(0.09f, 0.36f, 0.25f, 0.95f), new Color(0.55f, 1f, 0.75f));
            _chipRuim = Chip(new Color(0.45f, 0.10f, 0.10f, 0.95f), new Color(1f, 0.75f, 0.75f));

            int raioBarra = Mathf.Max(2, b / 150);
            _trilha = Caixa(CorSuperficie, raioBarra, 0);
            _barra = Caixa(CorAzul, raioBarra, 0);

            _simbolo = new GUIStyle(_titulo) { fontSize = b / 9, alignment = TextAnchor.MiddleCenter, wordWrap = false };
            _simbolo.normal.textColor = CorEscuro;
            _simbolo.normal.background = Arredondado(CorAmbar, b / 12);
            _simbolo.border = new RectOffset(b / 12, b / 12, b / 12, b / 12);

            _escurecer = new Texture2D(1, 1);
            _escurecer.SetPixel(0, 0, new Color(0f, 0f, 0f, 0.55f));
            _escurecer.Apply();
        }

        GUIStyle Caixa(Color cor, int raio, int padding)
        {
            var estilo = new GUIStyle { padding = new RectOffset(padding, padding, padding, padding), border = new RectOffset(raio + 2, raio + 2, raio + 2, raio + 2) };
            estilo.normal.background = Arredondado(cor, raio);
            return estilo;
        }

        GUIStyle Botao(Color cor, Color texto, int raio)
        {
            var estilo = Caixa(cor, raio, 0);
            estilo.alignment = TextAnchor.MiddleCenter;
            estilo.fontSize = Screen.width / 23;
            estilo.fontStyle = FontStyle.Bold;
            estilo.normal.textColor = texto;
            estilo.hover = estilo.normal;
            estilo.active.background = Arredondado(Color.Lerp(cor, Color.black, 0.25f), raio);
            estilo.active.textColor = texto;
            return estilo;
        }

        GUIStyle Pilula(Color cor, Color texto, int raio)
        {
            var estilo = Caixa(cor, raio, 0);
            estilo.alignment = TextAnchor.MiddleCenter;
            estilo.fontSize = Screen.width / 27;
            estilo.fontStyle = FontStyle.Bold;
            estilo.normal.textColor = texto;
            estilo.clipping = TextClipping.Clip;
            return estilo;
        }

        GUIStyle Chip(Color cor, Color texto)
        {
            var estilo = Caixa(cor, Screen.width / 40, 0);
            estilo.alignment = TextAnchor.MiddleCenter;
            estilo.fontSize = Screen.width / 33;
            estilo.fontStyle = FontStyle.Bold;
            estilo.normal.textColor = texto;
            return estilo;
        }

        // Retângulo de cantos arredondados com borda suave, para esticar como 9-slice
        static Texture2D Arredondado(Color cor, int raio)
        {
            int lado = raio * 2 + 4;
            var textura = new Texture2D(lado, lado, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            var pixels = new Color[lado * lado];
            float centro = lado / 2f, interno = centro - raio;
            for (int y = 0; y < lado; y++)
            {
                for (int x = 0; x < lado; x++)
                {
                    float dx = Mathf.Max(0f, Mathf.Abs(x + 0.5f - centro) - interno);
                    float dy = Mathf.Max(0f, Mathf.Abs(y + 0.5f - centro) - interno);
                    float alfa = Mathf.Clamp01(raio - Mathf.Sqrt(dx * dx + dy * dy) + 0.5f);
                    pixels[y * lado + x] = new Color(cor.r, cor.g, cor.b, cor.a * alfa);
                }
            }
            textura.SetPixels(pixels);
            textura.Apply();
            return textura;
        }
    }
}
