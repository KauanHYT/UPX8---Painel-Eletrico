using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace PainelAR
{
    // Diagnóstico temporário do reconhecimento: cobre cada marcador reconhecido com uma placa verde
    // e lista na tela os marcadores vistos. Será substituído pelo RastreadorMarcadores na Fase 3.
    [RequireComponent(typeof(ARTrackedImageManager))]
    public class DiagnosticoMarcadores : MonoBehaviour
    {
        ARTrackedImageManager _gerenciador;
        readonly Dictionary<TrackableId, GameObject> _placas = new Dictionary<TrackableId, GameObject>();
        GUIStyle _estilo;

        public int MarcadoresVisiveis { get; private set; }

        void Awake() => _gerenciador = GetComponent<ARTrackedImageManager>();
        void OnEnable() => _gerenciador.trackablesChanged.AddListener(AoMudar);
        void OnDisable() => _gerenciador.trackablesChanged.RemoveListener(AoMudar);

        void AoMudar(ARTrackablesChangedEventArgs<ARTrackedImage> args)
        {
            foreach (var imagem in args.added)
            {
                var placa = GameObject.CreatePrimitive(PrimitiveType.Quad);
                Destroy(placa.GetComponent<Collider>());
                placa.transform.SetParent(imagem.transform, false);
                placa.transform.localPosition = new Vector3(0f, 0.001f, 0f);
                placa.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                placa.transform.localScale = new Vector3(imagem.size.x, imagem.size.y, 1f);
                var material = new Material(Shader.Find("Sprites/Default")) { color = new Color(0.1f, 1f, 0.3f, 0.45f) };
                placa.GetComponent<Renderer>().material = material;
                _placas[imagem.trackableId] = placa;
                Debug.Log("[PainelAR] Marcador reconhecido: " + imagem.referenceImage.name);
            }
            foreach (var removido in args.removed)
                if (_placas.TryGetValue(removido.Key, out var placa)) { Destroy(placa); _placas.Remove(removido.Key); }
        }

        void Update()
        {
            int visiveis = 0;
            foreach (var imagem in _gerenciador.trackables)
            {
                bool visivel = imagem.trackingState == TrackingState.Tracking;
                if (_placas.TryGetValue(imagem.trackableId, out var placa)) placa.SetActive(visivel);
                if (visivel) visiveis++;
            }
            MarcadoresVisiveis = visiveis;
        }

        void OnGUI()
        {
            if (_estilo == null)
            {
                _estilo = new GUIStyle(GUI.skin.box) { fontSize = Mathf.Max(14, Screen.height / 40), alignment = TextAnchor.UpperLeft, wordWrap = true };
                _estilo.normal.textColor = Color.white;
            }
            var texto = "DIAGNÓSTICO DE MARCADORES\n";
            foreach (var imagem in _gerenciador.trackables)
                texto += imagem.referenceImage.name + ": " + (imagem.trackingState == TrackingState.Tracking ? "VISÍVEL" : "fora da vista") + "\n";
            if (_gerenciador.trackables.count == 0) texto += "Nenhum marcador reconhecido ainda\n";
#if UNITY_EDITOR
            texto += "\nSimulação: segure o botão direito do mouse e use W A S D (Q/E sobe e desce)";
#endif
            GUI.Box(new Rect(10, 10, Screen.width - 20, Screen.height * 0.25f), texto, _estilo);
        }
    }
}
