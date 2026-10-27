#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.XR.ARSubsystems;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace PainelAR.EditorTools
{
    // Fase 2: gera as imagens dos marcadores e a Reference Image Library da cena.
    // As imagens são procedurais (semente fixa = sempre as mesmas): muitas formas de tamanhos,
    // cores e ângulos variados, sem simetria nem repetição, o que dá bastante cantos para o ARCore.
    public static class GeradorMarcadores
    {
        public const float LarguraImpressaMetros = 0.08f; // 8 cm, como sugere o guia (Parte 5.5)
        const int Resolucao = 1024;
        const string PastaMarcadores = "Assets/Markers";
        const string BibliotecaPath = "Assets/ReferenceImages/PainelReferenceImageLibrary.asset";

        // Nome na biblioteca = campo "marcador" do conteudo_painel.json (Fase 3)
        static readonly (string nome, int semente, Color[] paleta)[] Marcadores =
        {
            ("marcador_DJ1", 1101, new[] { Hex("1b3a6b"), Hex("e8b923"), Hex("d7263d"), Hex("f4f1de"), Hex("2e86ab"), Hex("111111") }),
            ("marcador_K1",  2207, new[] { Hex("2d6a4f"), Hex("ff7b00"), Hex("6a0572"), Hex("e9f5db"), Hex("40916c"), Hex("0b090a") }),
            ("marcador_FT1", 3313, new[] { Hex("7b2cbf"), Hex("06d6a0"), Hex("ef476f"), Hex("fff3b0"), Hex("118ab2"), Hex("1d1d1d") }),
        };

        [MenuItem("Painel AR/Marcadores/Gerar imagens")]
        public static void GerarImagens()
        {
            Directory.CreateDirectory(PastaMarcadores);
            foreach (var m in Marcadores)
            {
                var caminho = PastaMarcadores + "/" + m.nome + ".png";
                File.WriteAllBytes(caminho, Desenhar(m.semente, m.paleta).EncodeToPNG());
                Debug.Log("[PainelAR] Imagem gerada: " + caminho);
            }
            AssetDatabase.Refresh();
        }

        [MenuItem("Painel AR/Marcadores/Criar biblioteca e ligar à cena")]
        public static void CriarBiblioteca()
        {
            if (AssetDatabase.LoadAssetAtPath<XRReferenceImageLibrary>(BibliotecaPath) != null)
                AssetDatabase.DeleteAsset(BibliotecaPath);
            var biblioteca = ScriptableObject.CreateInstance<XRReferenceImageLibrary>();
            AssetDatabase.CreateAsset(biblioteca, BibliotecaPath);
            for (int i = 0; i < Marcadores.Length; i++)
            {
                var textura = AssetDatabase.LoadAssetAtPath<Texture2D>(PastaMarcadores + "/" + Marcadores[i].nome + ".png");
                if (textura == null) throw new InvalidOperationException("[PainelAR] Rode antes Painel AR → Marcadores → Gerar imagens");
                biblioteca.Add();
                biblioteca.SetName(i, Marcadores[i].nome);
                biblioteca.SetTexture(i, textura, false); // Keep Texture at Runtime desmarcado
                biblioteca.SetSpecifySize(i, true);
                biblioteca.SetSize(i, new Vector2(LarguraImpressaMetros, LarguraImpressaMetros));
            }
            EditorUtility.SetDirty(biblioteca);
            AssetDatabase.SaveAssets();

            var cena = EditorSceneManager.OpenScene(PainelARSetup.ScenePath);
            var gerenciador = UnityEngine.Object.FindAnyObjectByType<ARTrackedImageManager>();
            gerenciador.referenceLibrary = biblioteca;
            gerenciador.requestedMaxNumberOfMovingImages = Marcadores.Length;
            EditorUtility.SetDirty(gerenciador);
            EditorSceneManager.MarkSceneDirty(cena);
            EditorSceneManager.SaveScene(cena);

            // Material antigo do WL03, que não é mais usado
            AssetDatabase.DeleteAsset("Assets/ReferenceImages/WL03ReferenceImageLibrary.asset");
            AssetDatabase.DeleteAsset("Assets/Markers/wl03-equipment-target.png");
            AssetDatabase.SaveAssets();
            Debug.Log("[PainelAR] Biblioteca criada com " + biblioteca.count + " marcadores de " + LarguraImpressaMetros * 100f + " cm e ligada à cena");
        }

        // ---------- desenho ----------

        static Texture2D Desenhar(int semente, Color[] paleta)
        {
            var rnd = new System.Random(semente);
            var px = new Color[Resolucao * Resolucao];

            // Fundo com manchas suaves (evita áreas grandes de cor lisa)
            var fundo = paleta[3];
            for (int y = 0; y < Resolucao; y++)
                for (int x = 0; x < Resolucao; x++)
                {
                    float n = Mathf.PerlinNoise(x * 0.012f + semente, y * 0.012f - semente);
                    px[y * Resolucao + x] = Color.Lerp(fundo, paleta[4], n * 0.35f);
                }

            // Formas grandes e assimétricas, depois médias e pequenas (detalhe em várias escalas)
            Camada(px, rnd, paleta, 14, 120, 260);
            Camada(px, rnd, paleta, 60, 40, 120);
            Camada(px, rnd, paleta, 220, 10, 40);
            for (int i = 0; i < 70; i++)
                Linha(px, rnd.Next(Resolucao), rnd.Next(Resolucao), rnd.Next(Resolucao), rnd.Next(Resolucao),
                      2 + rnd.Next(7), paleta[rnd.Next(paleta.Length)]);

            // Moldura escura irregular só no canto inferior esquerdo: quebra a simetria de rotação
            Retangulo(px, 0, 0, Resolucao, 24, paleta[5]);
            Retangulo(px, 0, 0, 24, Resolucao * 2 / 3, paleta[5]);

            var tex = new Texture2D(Resolucao, Resolucao, TextureFormat.RGB24, false);
            tex.SetPixels(px);
            tex.Apply();
            return tex;
        }

        static void Camada(Color[] px, System.Random rnd, Color[] paleta, int quantidade, int min, int max)
        {
            for (int i = 0; i < quantidade; i++)
            {
                var cor = paleta[rnd.Next(paleta.Length)];
                int cx = rnd.Next(Resolucao), cy = rnd.Next(Resolucao), r = rnd.Next(min, max);
                switch (rnd.Next(4))
                {
                    case 0: Circulo(px, cx, cy, r, cor); break;
                    case 1: Anel(px, cx, cy, r, Mathf.Max(3, r / 4), cor); break;
                    case 2: Retangulo(px, cx, cy, rnd.Next(min, max), rnd.Next(min, max) / 2 + 4, cor); break;
                    default:
                        Triangulo(px, new Vector2(cx, cy),
                            new Vector2(cx + rnd.Next(-max, max), cy + rnd.Next(-max, max)),
                            new Vector2(cx + rnd.Next(-max, max), cy + rnd.Next(-max, max)), cor);
                        break;
                }
            }
        }

        static void Pintar(Color[] px, int x, int y, Color cor)
        {
            if (x < 0 || y < 0 || x >= Resolucao || y >= Resolucao) return;
            px[y * Resolucao + x] = cor;
        }

        static void Circulo(Color[] px, int cx, int cy, int r, Color cor)
        {
            for (int y = -r; y <= r; y++)
                for (int x = -r; x <= r; x++)
                    if (x * x + y * y <= r * r) Pintar(px, cx + x, cy + y, cor);
        }

        static void Anel(Color[] px, int cx, int cy, int r, int espessura, Color cor)
        {
            int ri = r - espessura;
            for (int y = -r; y <= r; y++)
                for (int x = -r; x <= r; x++)
                {
                    int d = x * x + y * y;
                    if (d <= r * r && d >= ri * ri) Pintar(px, cx + x, cy + y, cor);
                }
        }

        static void Retangulo(Color[] px, int x0, int y0, int w, int h, Color cor)
        {
            for (int y = y0; y < y0 + h; y++)
                for (int x = x0; x < x0 + w; x++)
                    Pintar(px, x, y, cor);
        }

        static void Triangulo(Color[] px, Vector2 a, Vector2 b, Vector2 c, Color cor)
        {
            int minX = (int)Mathf.Min(a.x, Mathf.Min(b.x, c.x)), maxX = (int)Mathf.Max(a.x, Mathf.Max(b.x, c.x));
            int minY = (int)Mathf.Min(a.y, Mathf.Min(b.y, c.y)), maxY = (int)Mathf.Max(a.y, Mathf.Max(b.y, c.y));
            float area = (b.x - a.x) * (c.y - a.y) - (c.x - a.x) * (b.y - a.y);
            if (Mathf.Abs(area) < 1f) return;
            for (int y = Mathf.Max(minY, 0); y <= Mathf.Min(maxY, Resolucao - 1); y++)
                for (int x = Mathf.Max(minX, 0); x <= Mathf.Min(maxX, Resolucao - 1); x++)
                {
                    float w0 = ((b.x - x) * (c.y - y) - (c.x - x) * (b.y - y)) / area;
                    float w1 = ((c.x - x) * (a.y - y) - (a.x - x) * (c.y - y)) / area;
                    if (w0 >= 0 && w1 >= 0 && w0 + w1 <= 1) Pintar(px, x, y, cor);
                }
        }

        static void Linha(Color[] px, int x0, int y0, int x1, int y1, int espessura, Color cor)
        {
            int passos = Mathf.Max(Mathf.Abs(x1 - x0), Mathf.Abs(y1 - y0));
            for (int i = 0; i <= passos; i++)
            {
                float t = passos == 0 ? 0 : (float)i / passos;
                int x = Mathf.RoundToInt(Mathf.Lerp(x0, x1, t)), y = Mathf.RoundToInt(Mathf.Lerp(y0, y1, t));
                Retangulo(px, x - espessura / 2, y - espessura / 2, espessura, espessura, cor);
            }
        }

        static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString("#" + hex, out var cor);
            return cor;
        }
    }
}
#endif
