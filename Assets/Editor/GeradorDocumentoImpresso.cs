#if UNITY_EDITOR
using System.IO;
using System.Net;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PainelAR.EditorTools
{
    // Parte 10 do guia: gera a documentação impressa da condição de controle a partir do mesmo
    // conteudo_painel.json do app (RNF07). Também gera a foto do painel com os componentes indicados
    // e a ficha que o pesquisador preenche na condição papel (Parte 11.4).
    // Tudo vai para docs/impressao (o guia usava a raiz do projeto).
    public static class GeradorDocumentoImpresso
    {
        const string Pasta = "docs/impressao";
        const string Foto = "foto_painel.jpg";

        [MenuItem("Painel AR/Gerar documentação impressa")]
        public static void Gerar()
        {
            var c = ConteudoPainel.Carregar();
            var sb = new StringBuilder();
            sb.Append("<!doctype html><html lang='pt-BR'><head><meta charset='utf-8'><title>").Append(H(c.titulo)).Append("</title>");
            sb.Append("<style>body{font-family:Arial,sans-serif;margin:1.5cm;font-size:12pt}h1{font-size:18pt}");
            sb.Append("table{border-collapse:collapse;width:100%;margin-bottom:1cm}td,th{border:1px solid #444;padding:6px;text-align:left;vertical-align:top}");
            sb.Append(".alerta{color:#a00;font-weight:bold}.ok{width:1.2cm}img{max-width:100%;margin-bottom:1cm}");
            sb.Append("@page{size:A4;margin:1.2cm}body{margin:0}tr{page-break-inside:avoid}</style></head><body>");
            sb.Append("<h1>").Append(H(c.titulo)).Append("</h1><p>Versão do conteúdo: ").Append(H(c.versao)).Append("</p>");

            // Foto do painel com os componentes indicados (Painel AR → Gerar foto do painel)
            if (File.Exists(Path.Combine(Pasta, Foto))) sb.Append("<img src='").Append(Foto).Append("'>");

            sb.Append("<h2>Componentes</h2><table><tr><th>ID</th><th>Nome</th><th>Função</th><th>Alerta de segurança</th></tr>");
            foreach (var k in c.componentes)
                sb.Append("<tr><td>").Append(H(k.id)).Append("</td><td>").Append(H(k.nome)).Append("</td><td>").Append(H(k.funcao))
                  .Append("</td><td class='alerta'>").Append(H(k.alerta)).Append("</td></tr>");
            sb.Append("</table>");

            sb.Append("<h2>Procedimento</h2><table><tr><th>Etapa</th><th>Componente</th><th>Instrução</th><th>Alerta</th><th class='ok'>OK</th></tr>");
            foreach (var e in c.etapas)
            {
                var k = c.BuscarPorId(e.componenteId);
                sb.Append("<tr><td>").Append(e.numero).Append(". ").Append(H(e.titulo)).Append("</td><td>").Append(H(k.nome))
                  .Append("</td><td>").Append(H(e.instrucao)).Append("</td><td class='alerta'>").Append(H(e.alerta)).Append("</td><td></td></tr>");
            }
            sb.Append("</table></body></html>");

            Salvar("Documentacao_Impressa.html", sb.ToString());
        }

        // Ficha do pesquisador na condição papel: o participante indica, o pesquisador marca.
        // Não entregar ao participante (tem o gabarito).
        [MenuItem("Painel AR/Gerar ficha de observação (papel)")]
        public static void GerarFichaObservacao()
        {
            var c = ConteudoPainel.Carregar();
            var sb = new StringBuilder();
            sb.Append("<!doctype html><html lang='pt-BR'><head><meta charset='utf-8'><title>Ficha de observação</title>");
            sb.Append("<style>@page{size:A4;margin:1.2cm}body{font-family:Arial,sans-serif;font-size:10.5pt;margin:0}h1{font-size:15pt;margin:0 0 2mm}");
            sb.Append(".aviso{color:#a00;font-weight:bold;margin:0 0 3mm}.campos{display:grid;grid-template-columns:1fr 1fr;gap:2.5mm 8mm;margin-bottom:4mm}");
            sb.Append(".campo{border-bottom:1px solid #444;padding-top:4mm}table{border-collapse:collapse;width:100%}td,th{border:1px solid #444;padding:4px 5px;text-align:left}");
            sb.Append("th{background:#eee}.c{text-align:center;white-space:nowrap}.obs{width:28%}</style></head><body>");
            sb.Append("<h1>Ficha de observação — condição documentação impressa</h1>");
            sb.Append("<p class='aviso'>Uso do pesquisador. Não entregar ao participante: a coluna \"Solicitado\" é o gabarito.</p>");
            sb.Append("<div class='campos'>");
            foreach (var campo in new[] { "Participante (código, ex.: P01)", "Data", "Ordem: ( ) papel primeiro  ( ) RA primeiro",
                         "Familiaridade com painéis: ( ) nenhuma ( ) pouca ( ) média ( ) alta", "Hora de início", "Hora de término",
                         "Tempo total (segundos)", "Pesquisador" })
                sb.Append("<div class='campo'>").Append(H(campo)).Append(":</div>");
            sb.Append("</div>");
            sb.Append("<table><tr><th>Etapa</th><th>Solicitado</th><th class='c'>Componente indicado</th><th class='obs'>Observações</th></tr>");
            string opcoes = "";
            foreach (var k in c.componentes) opcoes += "( ) " + H(k.id) + "&nbsp;&nbsp;";
            opcoes += "( ) outro";
            foreach (var e in c.etapas)
                sb.Append("<tr><td>").Append(e.numero).Append(". ").Append(H(e.titulo)).Append("</td><td class='c'>").Append(H(e.componenteId))
                  .Append("</td><td class='c'>").Append(opcoes).Append("</td><td></td></tr>");
            sb.Append("</table><p>Conteúdo ").Append(H(c.versao)).Append(". Depois da sessão, passe as respostas para o modelo_papel.csv (docs/ANALISE.md).</p></body></html>");
            Salvar("Ficha_Observacao_Papel.html", sb.ToString());

            // Modelo de CSV com as mesmas colunas do app (Parte 11.4), já com as etapas e o gabarito
            var csv = new StringBuilder("participante;condicao;data_hora;tempo_s;evento;etapa;componente_solicitado;componente_indicado;correto;detalhe\n");
            csv.Append("Pxx;PAPEL;;0;SESSAO_INICIO;;;;;conteudo ").Append(c.versao).Append('\n');
            foreach (var e in c.etapas)
                csv.Append("Pxx;PAPEL;;;IDENTIFICACAO;").Append(e.numero).Append(';').Append(e.componenteId).Append(";;;\n");
            csv.Append("Pxx;PAPEL;;;SESSAO_FIM;;;;;\n");
            Directory.CreateDirectory("docs/analise");
            File.WriteAllText("docs/analise/modelo_papel.csv", csv.ToString(), new UTF8Encoding(true));
            Debug.Log("[PainelAR] Modelo de CSV da condição papel em docs/analise/modelo_papel.csv");
        }

        // Foto frontal do painel virtual com legendas apontando cada componente.
        // Provisória: quando o painel didático real estiver montado, troque por uma foto dele.
        [MenuItem("Painel AR/Gerar foto do painel (simulação)")]
        public static void GerarFotoPainel()
        {
            var c = ConteudoPainel.Carregar();
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Simulacao/AmbientePainel.prefab");
            if (prefab == null) throw new System.InvalidOperationException("[PainelAR] Rode antes Painel AR → Simulação → Configurar simulação no PC");

            var cena = EditorSceneManager.NewPreviewScene();
            try
            {
                var ambiente = (GameObject)PrefabUtility.InstantiatePrefab(prefab, cena);
                var centro = PainelARSimulacao.CentroPainel;
                var painel = ambiente.transform.Find("QuadroDeComando");
                painel.Find("Dobradica").gameObject.SetActive(false); // a porta aberta cobriria a legenda da esquerda
                ambiente.transform.Find("Bancada").gameObject.SetActive(false);

                // Legendas: componentes da metade esquerda à esquerda do painel, os outros à direita
                int esquerda = 0, direita = 0;
                foreach (var k in c.componentes)
                {
                    if (!PainelARSimulacao.PosicaoComponentes.TryGetValue(k.id, out var pos)) continue;
                    bool lado = pos.x < 0f;
                    float x = lado ? -0.47f : 0.47f;
                    float y = 0.2f - 0.16f * (lado ? esquerda++ : direita++);
                    // Na frente do gabinete (laterais vão até z = -0.23), para as linhas não sumirem atrás delas
                    Legenda(painel, k.nome, new Vector3(x, y, -0.30f), new Vector3(pos.x, pos.y, -0.07f));
                }

                // A cena de pré-visualização tem luz ambiente mais forte que a da simulação
                foreach (var luz in ambiente.GetComponentsInChildren<Light>()) luz.intensity *= 0.45f;

                var cam = new GameObject("CameraFoto").AddComponent<Camera>();
                SceneManager_Mover(cam.gameObject, cena);
                cam.scene = cena;
                cam.transform.position = centro + new Vector3(0f, 0f, -1.55f);
                cam.fieldOfView = 46f;
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.85f, 0.83f, 0.78f);

                var rt = new RenderTexture(1600, 1200, 24);
                cam.targetTexture = rt;
                cam.Render();
                RenderTexture.active = rt;
                var imagem = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
                imagem.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                imagem.Apply();
                RenderTexture.active = null;
                cam.targetTexture = null;
                Directory.CreateDirectory(Pasta);
                File.WriteAllBytes(Path.Combine(Pasta, Foto), imagem.EncodeToJPG(92));
                Debug.Log("[PainelAR] Foto do painel gerada em " + Path.GetFullPath(Path.Combine(Pasta, Foto)));
            }
            finally
            {
                EditorSceneManager.ClosePreviewScene(cena);
            }
        }

        static void SceneManager_Mover(GameObject objeto, UnityEngine.SceneManagement.Scene cena) =>
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(objeto, cena);

        // Caixa de texto branca com uma linha até o componente
        static void Legenda(Transform painel, string texto, Vector3 posTexto, Vector3 posComponente)
        {
            var fonte = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var raiz = new GameObject("Legenda").transform;
            raiz.SetParent(painel, false);

            var fundo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fundo.transform.SetParent(raiz, false);
            fundo.transform.localPosition = posTexto + new Vector3(0f, 0f, 0.002f);
            fundo.transform.localScale = new Vector3(0.30f, 0.055f, 0.002f);
            fundo.GetComponent<Renderer>().sharedMaterial = new Material(Shader.Find("Unlit/Color")) { color = Color.white };

            var t = new GameObject("Texto");
            t.transform.SetParent(raiz, false);
            t.transform.localPosition = posTexto;
            t.transform.localScale = Vector3.one * 0.0034f;
            var tm = t.AddComponent<TextMesh>();
            tm.text = texto;
            tm.font = fonte;
            tm.fontSize = 48;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.color = new Color(0.1f, 0.1f, 0.1f);
            t.GetComponent<MeshRenderer>().sharedMaterial = fonte.material;

            // Linha do canto da caixa mais próximo até o componente
            var inicio = posTexto + new Vector3(posTexto.x < 0f ? 0.15f : -0.15f, 0f, 0f);
            var linha = GameObject.CreatePrimitive(PrimitiveType.Cube);
            linha.transform.SetParent(raiz, false);
            linha.transform.localPosition = (inicio + posComponente) / 2f;
            linha.transform.localRotation = Quaternion.FromToRotation(Vector3.right, posComponente - inicio);
            linha.transform.localScale = new Vector3(Vector3.Distance(inicio, posComponente), 0.004f, 0.004f);
            linha.GetComponent<Renderer>().sharedMaterial = new Material(Shader.Find("Unlit/Color")) { color = new Color(0.8f, 0.05f, 0.05f) };
        }

        static void Salvar(string nome, string html)
        {
            Directory.CreateDirectory(Pasta);
            string caminho = Path.GetFullPath(Path.Combine(Pasta, nome));
            File.WriteAllText(caminho, html, new UTF8Encoding(false));
            Debug.Log("[PainelAR] Documento gerado em " + caminho);
            if (!Application.isBatchMode) Application.OpenURL("file:///" + caminho.Replace('\\', '/'));
        }

        static string H(string texto) => WebUtility.HtmlEncode(texto ?? "");
    }
}
#endif
