using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace PainelAR
{
    [RequireComponent(typeof(ARTrackedImageManager))]
    public class RastreadorMarcadores : MonoBehaviour
    {
        [Tooltip("Distância máxima entre a mira e o centro do marcador, em fração da largura da tela.")]
        [SerializeField] float raioMira = 0.2f;

        [Tooltip("Tamanho do texto 3D acima dos marcadores. Aumente se ficar pequeno demais.")]
        [SerializeField] float escalaRotulo = 0.003f;

        // Posição da mira na tela: meio na horizontal, um pouco acima do centro.
        public static Vector2 PosicaoMira => new Vector2(Screen.width * 0.5f, Screen.height * 0.62f);

        public string MarcadorDestacado { get; set; }
        public Func<string, string> TextoDoMarcador;
        public Action<string> MarcadorFicouVisivel;

        class Visual
        {
            public ARTrackedImage imagem;
            public GameObject raiz;
            public Material material;
            public TextMesh texto;
            public bool estavaVisivel;
        }

        readonly Dictionary<TrackableId, Visual> _visuais = new Dictionary<TrackableId, Visual>();
        ARTrackedImageManager _gerenciador;
        Font _fonte;

        void Awake()
        {
            _gerenciador = GetComponent<ARTrackedImageManager>();
            _fonte = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        void OnEnable() => _gerenciador.trackablesChanged.AddListener(AoMudarMarcadores);
        void OnDisable() => _gerenciador.trackablesChanged.RemoveListener(AoMudarMarcadores);

        void AoMudarMarcadores(ARTrackablesChangedEventArgs<ARTrackedImage> args)
        {
            foreach (var imagem in args.added)
                _visuais[imagem.trackableId] = CriarVisual(imagem);

            foreach (var removido in args.removed)
            {
                if (_visuais.TryGetValue(removido.Key, out var visual))
                {
                    if (visual.raiz != null) Destroy(visual.raiz);
                    _visuais.Remove(removido.Key);
                }
            }
        }

        void Update()
        {
            var cam = Camera.main;
            foreach (var v in _visuais.Values)
            {
                if (v.imagem == null || v.raiz == null) continue;

                // "Tracking" = a câmera está vendo o marcador agora
                bool visivel = v.imagem.trackingState == TrackingState.Tracking;
                v.raiz.SetActive(visivel);
                if (visivel && !v.estavaVisivel) MarcadorFicouVisivel?.Invoke(v.imagem.referenceImage.name);
                v.estavaVisivel = visivel;
                if (!visivel) continue;

                string nome = v.imagem.referenceImage.name;
                bool destaque = nome == MarcadorDestacado;
                v.material.color = destaque ? new Color(1f, 0.55f, 0f, 0.55f) : new Color(0.3f, 0.6f, 1f, 0.25f);
                v.texto.text = TextoDoMarcador != null ? TextoDoMarcador(nome) : nome;
                v.texto.color = destaque ? new Color(1f, 0.8f, 0.2f) : Color.white;

                if (cam != null)
                {
                    // Texto acima do marcador e sempre virado para a câmera
                    float meiaAltura = Mathf.Max(v.imagem.size.y, 0.05f) * 0.5f;
                    v.texto.transform.position = v.imagem.transform.position + cam.transform.up * (meiaAltura + 0.01f);
                    v.texto.transform.rotation = Quaternion.LookRotation(v.texto.transform.position - cam.transform.position, cam.transform.up);
                }
            }
        }

        // Nome do marcador mais próximo da mira, ou null se nenhum estiver perto
        public string MarcadorNaMira()
        {
            var cam = Camera.main;
            if (cam == null) return null;

            string melhor = null;
            float menorDistancia = raioMira * Screen.width;
            foreach (var v in _visuais.Values)
            {
                if (v.imagem == null || v.imagem.trackingState != TrackingState.Tracking) continue;
                Vector3 p = cam.WorldToScreenPoint(v.imagem.transform.position);
                if (p.z <= 0f) continue;
                float d = Vector2.Distance(PosicaoMira, new Vector2(p.x, p.y));
                if (d < menorDistancia)
                {
                    menorDistancia = d;
                    melhor = v.imagem.referenceImage.name;
                }
            }
            return melhor;
        }

        Visual CriarVisual(ARTrackedImage imagem)
        {
            var v = new Visual { imagem = imagem };
            v.raiz = new GameObject("Visual_" + imagem.referenceImage.name);
            v.raiz.transform.SetParent(imagem.transform, false);

            // Placa colorida semitransparente deitada sobre o marcador
            var placa = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Destroy(placa.GetComponent<Collider>());
            placa.transform.SetParent(v.raiz.transform, false);
            placa.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            Vector2 tamanho = imagem.size.x > 0f ? imagem.size : new Vector2(0.08f, 0.08f);
            placa.transform.localScale = new Vector3(tamanho.x, tamanho.y, 1f);
            v.material = new Material(Shader.Find("Sprites/Default"));
            placa.GetComponent<Renderer>().material = v.material;

            // Texto 3D com o nome do componente
            var rotulo = new GameObject("Rotulo");
            rotulo.transform.SetParent(v.raiz.transform, false);
            rotulo.transform.localScale = Vector3.one * escalaRotulo;
            v.texto = rotulo.AddComponent<TextMesh>();
            v.texto.font = _fonte;
            v.texto.fontSize = 64;
            v.texto.characterSize = 1f;
            v.texto.anchor = TextAnchor.LowerCenter;
            v.texto.alignment = TextAlignment.Center;
            rotulo.GetComponent<MeshRenderer>().material = _fonte.material;

            return v;
        }

        void OnDestroy()
        {
            foreach (var v in _visuais.Values)
                if (v.material != null) Destroy(v.material);
        }
    }
}
