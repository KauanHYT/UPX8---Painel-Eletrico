#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Management;
using UnityEngine.XR.Simulation;

namespace PainelAR.EditorTools
{
    // Permite testar o app no PC, sem celular: liga o XR Simulation no Play do editor
    // e gera um ambiente virtual com um quadro de comando (DJ1, K1, FT1) e os marcadores da biblioteca
    // colados ao lado de cada componente. Rode de novo sempre que a Reference Image Library mudar.
    public static class PainelARSimulacao
    {
        const string AmbientePath = "Assets/Simulacao/AmbientePainel.prefab";
        const string MateriaisPath = "Assets/Simulacao/Materiais";
        const string PreferenciasPath = "Assets/XR/UserSimulationSettings/Resources/XRSimulationPreferences.asset";

        // Centro da placa de montagem, em metros. A placa fica no plano XY, de frente para -Z.
        public static readonly Vector3 CentroPainel = new Vector3(0f, 1.3f, 0f);
        public const float DistanciaCamera = 0.75f; // distância inicial entre a câmera e a placa

        // Centro de cada componente na placa (x, y relativos ao centro), pelo id do conteudo_painel.json
        public static readonly Dictionary<string, Vector2> PosicaoComponentes = new Dictionary<string, Vector2>
        {
            { "DJ1", new Vector2(-0.17f, 0.13f) },
            { "K1",  new Vector2(0.10f, 0.005f) },
            { "FT1", new Vector2(0.10f, -0.083f) },
        };

        // Posição de cada marcador na placa (x, y relativos ao centro), ao lado do seu componente
        public static readonly Dictionary<string, Vector2> PosicaoMarcadores = new Dictionary<string, Vector2>
        {
            { "marcador_DJ1", new Vector2(-0.17f, 0.005f) },
            { "marcador_K1",  new Vector2(0.000f, 0.005f) },
            { "marcador_FT1", new Vector2(0.185f, -0.085f) },
        };

        static readonly Dictionary<string, Material> s_Materiais = new Dictionary<string, Material>();
        static Font s_Fonte;

        [MenuItem("Painel AR/Simulação/Configurar simulação no PC")]
        public static void ConfigurarSimulacao()
        {
            AtivarLoaderNoPC();
            AtivarInputSystem();
            var ambiente = GerarAmbiente();
            var prefs = AssetDatabase.LoadMainAssetAtPath(PreferenciasPath);
            if (prefs == null) throw new InvalidOperationException("[PainelAR] Não encontrei " + PreferenciasPath);
            var so = new SerializedObject(prefs);
            so.FindProperty("m_EnvironmentPrefab").objectReferenceValue = ambiente;
            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
            Debug.Log("[PainelAR] Simulação configurada. Aperte Play no editor para testar.");
        }

        // O Play do editor usa as configurações XR da plataforma Windows (Standalone)
        static void AtivarLoaderNoPC()
        {
            EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.k_SettingsKey, out XRGeneralSettingsPerBuildTarget porAlvo);
            if (porAlvo == null) throw new InvalidOperationException("[PainelAR] Configurações XR não encontradas");
            if (!porAlvo.HasSettingsForBuildTarget(BuildTargetGroup.Standalone)) porAlvo.CreateDefaultSettingsForBuildTarget(BuildTargetGroup.Standalone);
            if (!porAlvo.HasManagerSettingsForBuildTarget(BuildTargetGroup.Standalone)) porAlvo.CreateDefaultManagerSettingsForBuildTarget(BuildTargetGroup.Standalone);
            var pc = porAlvo.SettingsForBuildTarget(BuildTargetGroup.Standalone);
            pc.InitManagerOnStart = true;
            XRPackageMetadataStore.AssignLoader(pc.AssignedSettings, "UnityEngine.XR.Simulation.SimulationLoader", BuildTargetGroup.Standalone);
            EditorUtility.SetDirty(pc);
            EditorUtility.SetDirty(porAlvo);
        }

        // A navegação da simulação usa o Input System. "Both" mantém o OnGUI funcionando.
        // Vale a partir da próxima abertura do editor.
        static void AtivarInputSystem()
        {
            var player = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0];
            var so = new SerializedObject(player);
            var campo = so.FindProperty("activeInputHandler");
            if (campo.intValue == 2) return;
            campo.intValue = 2;
            so.ApplyModifiedPropertiesWithoutUndo();
            Debug.Log("[PainelAR] Active Input Handling = Both (reabra o editor se ele estiver aberto).");
        }

        static GameObject GerarAmbiente()
        {
            EditorSceneManager.OpenScene(PainelARSetup.ScenePath);
            var gerenciador = UnityEngine.Object.FindAnyObjectByType<ARTrackedImageManager>();
            var biblioteca = gerenciador != null ? gerenciador.referenceLibrary as XRReferenceImageLibrary : null;
            if (biblioteca == null || biblioteca.count == 0)
                throw new InvalidOperationException("[PainelAR] A cena não tem Reference Image Library com imagens");

            AssetDatabase.DeleteAsset(MateriaisPath); // só o ambiente usa; é recriado abaixo
            Directory.CreateDirectory(MateriaisPath);
            AssetDatabase.Refresh();
            s_Materiais.Clear();
            s_Fonte = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            var raiz = new GameObject("AmbientePainel");
            try
            {
                ConfigurarAmbiente(raiz);
                Sala(raiz.transform);

                var painel = new GameObject("QuadroDeComando").transform;
                painel.SetParent(raiz.transform, false);
                painel.localPosition = CentroPainel;
                Gabinete(painel);
                Estrutura(painel);
                Disjuntor(painel, PosicaoComponentes["DJ1"]);
                Contator(painel, PosicaoComponentes["K1"]);
                ReleSobrecarga(painel, PosicaoComponentes["FT1"]);
                ReguaDeBornes(painel, -0.215f);
                Cabos(painel);
                Etiqueta(painel, "DJ1", new Vector3(-0.12f, 0.175f, 0f));
                Etiqueta(painel, "K1", new Vector3(0.15f, 0.045f, 0f));
                Etiqueta(painel, "FT1", new Vector3(0.05f, -0.083f, 0f));
                Etiqueta(painel, "X1", new Vector3(-0.205f, -0.178f, 0f));
                Marcadores(painel, biblioteca);

                var prefab = PrefabUtility.SaveAsPrefabAsset(raiz, AmbientePath);
                Debug.Log("[PainelAR] Ambiente de simulação gerado com " + biblioteca.count + " marcador(es): " + AmbientePath);
                return prefab;
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(raiz);
            }
        }

        // SimulationEnvironment é interno no AR Foundation; adiciona pelo nome do tipo
        static void ConfigurarAmbiente(GameObject raiz)
        {
            var tipo = typeof(SimulatedTrackedImage).Assembly.GetType("UnityEngine.XR.Simulation.SimulationEnvironment", true);
            var so = new SerializedObject(raiz.AddComponent(tipo));
            var cameraPos = CentroPainel + new Vector3(0f, 0f, -DistanciaCamera);
            so.FindProperty("m_CameraStartingPose.position").vector3Value = cameraPos;
            so.FindProperty("m_CameraStartingPose.rotation").quaternionValue = Quaternion.identity;
            so.FindProperty("m_CameraMovementBounds").boundsValue = new Bounds(new Vector3(0f, 1.5f, -1.2f), new Vector3(4f, 3f, 2.4f));
            so.FindProperty("m_DefaultViewPose.position").vector3Value = cameraPos + new Vector3(0.7f, 0.3f, -0.6f);
            so.FindProperty("m_DefaultViewPose.rotation").quaternionValue = Quaternion.Euler(15f, -35f, 0f);
            so.FindProperty("m_DefaultViewPivot").vector3Value = CentroPainel;
            so.FindProperty("m_DefaultViewSize").floatValue = 1.5f;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ---------- sala e gabinete ----------

        static void Sala(Transform pai)
        {
            Caixa(pai, "Chao", new Vector3(0f, -0.005f, -1.2f), new Vector3(4f, 0.01f, 3f), "Piso", Hex("6b6b70"));
            Caixa(pai, "Parede", new Vector3(0f, 1.5f, 0.3f), new Vector3(4f, 3f, 0.1f), "Parede", Hex("d9d4c7"));
            Caixa(pai, "Bancada", new Vector3(0.75f, 0.45f, -0.35f), new Vector3(0.6f, 0.9f, 0.5f), "Bancada", Hex("4a5a6a"));

            var luz = new GameObject("Luz").AddComponent<Light>();
            luz.type = LightType.Directional;
            luz.intensity = 1.1f;
            luz.transform.SetParent(pai, false);
            luz.transform.rotation = Quaternion.Euler(30f, 15f, 0f);

            var luzPainel = new GameObject("LuzPainel").AddComponent<Light>();
            luzPainel.type = LightType.Point;
            luzPainel.range = 1.5f;
            luzPainel.intensity = 0.8f;
            luzPainel.transform.SetParent(pai, false);
            luzPainel.transform.localPosition = CentroPainel + new Vector3(0f, 0.3f, -0.6f);
        }

        // Caixa metálica de 60 × 80 × 25 cm presa na parede, com a porta aberta
        static void Gabinete(Transform p)
        {
            var cinza = Hex("c8cbcf");
            Caixa(p, "Fundo", new Vector3(0f, 0f, 0.025f), new Vector3(0.62f, 0.82f, 0.01f), "Gabinete", cinza);
            Caixa(p, "LateralEsq", new Vector3(-0.305f, 0f, -0.1f), new Vector3(0.01f, 0.82f, 0.26f), "Gabinete", cinza);
            Caixa(p, "LateralDir", new Vector3(0.305f, 0f, -0.1f), new Vector3(0.01f, 0.82f, 0.26f), "Gabinete", cinza);
            Caixa(p, "Topo", new Vector3(0f, 0.405f, -0.1f), new Vector3(0.62f, 0.01f, 0.26f), "Gabinete", cinza);
            Caixa(p, "Base", new Vector3(0f, -0.405f, -0.1f), new Vector3(0.62f, 0.01f, 0.26f), "Gabinete", cinza);

            // Placa de montagem laranja (cor típica), frente em z = 0
            Caixa(p, "PlacaMontagem", new Vector3(0f, 0f, 0.01f), new Vector3(0.56f, 0.76f, 0.02f), "PlacaMontagem", Hex("e07b39"));

            // Porta aberta para a esquerda, com dobradiça na lateral esquerda
            var dobradica = new GameObject("Dobradica").transform;
            dobradica.SetParent(p, false);
            dobradica.localPosition = new Vector3(-0.31f, 0f, -0.23f);
            dobradica.localRotation = Quaternion.Euler(0f, 115f, 0f);
            Caixa(dobradica, "Porta", new Vector3(0.31f, 0f, 0f), new Vector3(0.62f, 0.82f, 0.015f), "Gabinete", cinza);
            Caixa(dobradica, "Puxador", new Vector3(0.57f, 0f, 0.02f), new Vector3(0.02f, 0.12f, 0.025f), "Plastico", Hex("202020"));
            Texto(dobradica, "PlacaPerigo", "PERIGO\nRISCO DE CHOQUE", new Vector3(0.31f, 0.2f, 0.0085f), 0.0016f, Hex("111111"),
                Quaternion.Euler(0f, 180f, 0f), Hex("f2c200"), new Vector2(0.18f, 0.08f));
        }

        // Trilhos DIN e canaletas
        static void Estrutura(Transform p)
        {
            var metal = Hex("b9bcc0");
            Caixa(p, "TrilhoDIN_1", new Vector3(0f, 0.13f, -0.004f), new Vector3(0.52f, 0.035f, 0.0075f), "Metal", metal);
            Caixa(p, "TrilhoDIN_2", new Vector3(0.15f, 0.005f, -0.004f), new Vector3(0.2f, 0.035f, 0.0075f), "Metal", metal);
            Caixa(p, "TrilhoDIN_3", new Vector3(0f, -0.215f, -0.004f), new Vector3(0.52f, 0.035f, 0.0075f), "Metal", metal);

            var canaleta = Hex("9a9ea3");
            Caixa(p, "CanaletaSup", new Vector3(0f, 0.3f, -0.03f), new Vector3(0.52f, 0.06f, 0.06f), "Canaleta", canaleta);
            Caixa(p, "CanaletaInf", new Vector3(0f, -0.32f, -0.03f), new Vector3(0.52f, 0.06f, 0.06f), "Canaleta", canaleta);
            Caixa(p, "CanaletaEsq", new Vector3(-0.25f, -0.01f, -0.03f), new Vector3(0.04f, 0.56f, 0.06f), "Canaleta", canaleta);
            Caixa(p, "CanaletaDir", new Vector3(0.25f, -0.01f, -0.03f), new Vector3(0.04f, 0.56f, 0.06f), "Canaleta", canaleta);
        }

        // ---------- componentes ----------

        // Disjuntor tripolar (3 módulos de 18 mm), alavanca em DESLIGADO
        static void Disjuntor(Transform p, Vector3 pos)
        {
            var c = Grupo(p, "DJ1_Disjuntor", pos);
            var branco = Hex("eceae4");
            Caixa(c, "Corpo", new Vector3(0f, 0f, -0.032f), new Vector3(0.054f, 0.09f, 0.056f), "Plastico", branco);
            Caixa(c, "Frente", new Vector3(0f, 0f, -0.066f), new Vector3(0.054f, 0.045f, 0.012f), "Plastico", branco);
            Caixa(c, "Alavanca", new Vector3(0f, -0.012f, -0.08f), new Vector3(0.046f, 0.01f, 0.018f), "Alavanca", Hex("2f3a45"));
            Texto(c, "Liga", "I ON", new Vector3(0f, 0.016f, -0.0725f), 0.0006f, Hex("2e7d32"));
            Texto(c, "Desliga", "O OFF", new Vector3(0f, -0.019f, -0.0725f), 0.0006f, Hex("c62828"));
            Texto(c, "Modelo", "C32  3P  400V~", new Vector3(0f, 0.034f, -0.0605f), 0.0005f, Hex("333333"));
            for (int i = -1; i <= 1; i++)
            {
                Parafuso(c, new Vector3(i * 0.018f, 0.04f, -0.061f));
                Parafuso(c, new Vector3(i * 0.018f, -0.04f, -0.061f));
            }
            Texto(c, "Circuito", "ALIMENTAÇÃO GERAL", new Vector3(0f, -0.033f, -0.0605f), 0.00042f, Hex("333333"));
        }

        // Contator tripolar com bornes de potência e bobina A1/A2
        static void Contator(Transform p, Vector3 pos)
        {
            var c = Grupo(p, "K1_Contator", pos);
            var cinza = Hex("d6d8db");
            Caixa(c, "Corpo", new Vector3(0f, 0f, -0.04f), new Vector3(0.045f, 0.08f, 0.072f), "Plastico", cinza);
            Caixa(c, "Frente", new Vector3(0f, 0f, -0.083f), new Vector3(0.036f, 0.04f, 0.014f), "Plastico", cinza);
            Caixa(c, "Janela", new Vector3(0f, 0f, -0.0905f), new Vector3(0.016f, 0.012f, 0.002f), "Plastico", Hex("e8e8e8"));
            Texto(c, "Rotulo", "AC-3 25A  220V", new Vector3(0f, -0.013f, -0.0905f), 0.00045f, Hex("333333"));
            string[] cima = { "1/L1", "3/L2", "5/L3" }, baixo = { "2/T1", "4/T2", "6/T3" };
            for (int i = 0; i < 3; i++)
            {
                float x = (i - 1) * 0.0135f;
                Parafuso(c, new Vector3(x, 0.033f, -0.077f));
                Parafuso(c, new Vector3(x, -0.033f, -0.077f));
                Texto(c, "Borne_" + cima[i], cima[i], new Vector3(x, 0.024f, -0.0765f), 0.00034f, Hex("222222"));
                Texto(c, "Borne_" + baixo[i], baixo[i], new Vector3(x, -0.025f, -0.0765f), 0.00034f, Hex("222222"));
            }
            Texto(c, "A1", "A1", new Vector3(-0.019f, 0.036f, -0.0765f), 0.00038f, Hex("1a237e"));
            Texto(c, "A2", "A2", new Vector3(0.019f, 0.036f, -0.0765f), 0.00038f, Hex("1a237e"));
        }

        // Relé de sobrecarga encaixado embaixo do contator
        static void ReleSobrecarga(Transform p, Vector3 pos)
        {
            var c = Grupo(p, "FT1_ReleSobrecarga", pos);
            Caixa(c, "Corpo", new Vector3(0f, 0f, -0.035f), new Vector3(0.045f, 0.05f, 0.066f), "PlasticoEscuro", Hex("3a3d42"));
            var dial = Cilindro(c, "DialAjuste", new Vector3(-0.008f, 0.007f, -0.069f), 0.016f, 0.004f, "Dial", Hex("1e63b4"));
            dial.localRotation = Quaternion.Euler(90f, 0f, 0f);
            Caixa(c, "Ponteiro", new Vector3(-0.008f, 0.011f, -0.0715f), new Vector3(0.002f, 0.008f, 0.001f), "Plastico", Hex("ffffff"));
            Texto(c, "Faixa", "17-25A", new Vector3(-0.008f, -0.006f, -0.0685f), 0.00038f, Hex("ffffff"));
            var reset = Cilindro(c, "BotaoRearme", new Vector3(0.012f, 0.011f, -0.07f), 0.009f, 0.006f, "Rearme", Hex("1565c0"));
            reset.localRotation = Quaternion.Euler(90f, 0f, 0f);
            Texto(c, "Reset", "RESET", new Vector3(0.012f, 0.002f, -0.0685f), 0.00032f, Hex("ffffff"));
            Texto(c, "Contatos", "95 96   97 98", new Vector3(0f, -0.018f, -0.0685f), 0.00034f, Hex("ffffff"));
            for (int i = -1; i <= 1; i++) Parafuso(c, new Vector3(i * 0.0135f, -0.026f, -0.06f));
        }

        // Régua de bornes X1 (10 bornes cinza e 2 azuis de neutro)
        static void ReguaDeBornes(Transform p, float y)
        {
            var c = Grupo(p, "X1_ReguaDeBornes", new Vector3(0f, y, 0f));
            for (int i = 0; i < 12; i++)
            {
                float x = -0.18f + i * 0.0082f + (i >= 10 ? 0.004f : 0f);
                var cor = i >= 10 ? Hex("1e88e5") : Hex("9e9e9e");
                Caixa(c, "Borne_" + (i + 1), new Vector3(x, 0f, -0.025f), new Vector3(0.0062f, 0.048f, 0.042f), i >= 10 ? "BorneAzul" : "Borne", cor);
                Texto(c, "Num_" + (i + 1), (i + 1).ToString(), new Vector3(x, 0f, -0.0465f), 0.00036f, Hex("111111"));
            }
            Caixa(c, "Batente", new Vector3(-0.188f, 0f, -0.02f), new Vector3(0.006f, 0.04f, 0.03f), "Metal", Hex("b9bcc0"));
        }

        // Cabos dos componentes até as canaletas (sem colisor, para não esconder os marcadores)
        static void Cabos(Transform p)
        {
            var preto = Hex("1b1b1b");
            for (int i = -1; i <= 1; i++)
            {
                float x = -0.17f + i * 0.018f;
                Cabo(p, new Vector3(x, 0.175f, -0.06f), new Vector3(x, 0.27f, -0.04f), preto);   // DJ1 → canaleta superior
                float xk = 0.10f + i * 0.0135f;
                Cabo(p, new Vector3(xk, 0.045f, -0.075f), new Vector3(xk, 0.27f, -0.04f), preto); // K1 → canaleta superior
                Cabo(p, new Vector3(xk, -0.108f, -0.055f), new Vector3(xk, -0.29f, -0.04f), preto); // FT1 → canaleta inferior
            }
            Cabo(p, new Vector3(0.081f, 0.045f, -0.075f), new Vector3(0.081f, 0.27f, -0.04f), Hex("1e63b4")); // bobina A1
            Cabo(p, new Vector3(0.119f, 0.045f, -0.075f), new Vector3(0.119f, 0.27f, -0.04f), Hex("1e63b4")); // bobina A2
            for (int i = 0; i < 12; i++)
            {
                float x = -0.18f + i * 0.0082f + (i >= 10 ? 0.004f : 0f);
                Cabo(p, new Vector3(x, -0.19f, -0.035f), new Vector3(x, -0.29f, -0.04f), i >= 10 ? Hex("1e88e5") : preto);
            }
        }

        static void Marcadores(Transform p, XRReferenceImageLibrary biblioteca)
        {
            float xExtra = -0.2f;
            for (int i = 0; i < biblioteca.count; i++)
            {
                var referencia = biblioteca[i];
                var textura = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(referencia.textureGuid.ToString("N")));
                if (textura == null) throw new InvalidOperationException("[PainelAR] Textura do marcador " + referencia.name + " não encontrada");
                Vector2 tamanho = referencia.specifySize ? referencia.size : new Vector2(0.08f, 0.08f);

                // Marcador sem posição definida vai para uma fileira livre no alto da placa
                if (!PosicaoMarcadores.TryGetValue(referencia.name, out var pos)) { pos = new Vector2(xExtra, 0.22f); xExtra += 0.1f; }

                var marcador = new GameObject("Marcador_" + referencia.name);
                marcador.transform.SetParent(p, false);
                marcador.transform.localPosition = new Vector3(pos.x, pos.y, -0.0005f);
                // A imagem fica no plano XZ local com a frente em +Y: vira +Y para a câmera e o topo para cima
                marcador.transform.localRotation = Quaternion.LookRotation(Vector3.up, Vector3.back);
                marcador.AddComponent<MeshFilter>();
                marcador.AddComponent<MeshRenderer>();
                var simulada = marcador.AddComponent<SimulatedTrackedImage>();
                var sm = new SerializedObject(simulada);
                sm.FindProperty("m_Image").objectReferenceValue = textura;
                sm.FindProperty("m_ImagePhysicalSizeMeters").vector2Value = tamanho;
                sm.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        // ---------- primitivas ----------

        static Transform Grupo(Transform pai, string nome, Vector3 pos)
        {
            var g = new GameObject(nome).transform;
            g.SetParent(pai, false);
            g.localPosition = pos;
            return g;
        }

        static Transform Caixa(Transform pai, string nome, Vector3 pos, Vector3 escala, string material, Color cor)
        {
            var o = GameObject.CreatePrimitive(PrimitiveType.Cube);
            o.name = nome;
            o.transform.SetParent(pai, false);
            o.transform.localPosition = pos;
            o.transform.localScale = escala;
            o.GetComponent<Renderer>().sharedMaterial = Mat(material, cor);
            return o.transform;
        }

        static Transform Cilindro(Transform pai, string nome, Vector3 pos, float diametro, float altura, string material, Color cor)
        {
            var o = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            o.name = nome;
            o.transform.SetParent(pai, false);
            o.transform.localPosition = pos;
            o.transform.localScale = new Vector3(diametro, altura / 2f, diametro);
            o.GetComponent<Renderer>().sharedMaterial = Mat(material, cor);
            return o.transform;
        }

        static void Parafuso(Transform pai, Vector3 pos)
        {
            var o = Cilindro(pai, "Parafuso", pos, 0.006f, 0.002f, "Parafuso", Hex("8d8f93"));
            o.localRotation = Quaternion.Euler(90f, 0f, 0f);
            UnityEngine.Object.DestroyImmediate(o.GetComponent<Collider>());
        }

        static void Cabo(Transform pai, Vector3 de, Vector3 ate, Color cor)
        {
            var o = Cilindro(pai, "Cabo", (de + ate) / 2f, 0.004f, Vector3.Distance(de, ate), "Cabo_" + ColorUtility.ToHtmlStringRGB(cor), cor);
            o.localRotation = Quaternion.FromToRotation(Vector3.up, ate - de);
            UnityEngine.Object.DestroyImmediate(o.GetComponent<Collider>());
        }

        // Etiqueta branca de identificação, como as coladas em painéis reais
        static void Etiqueta(Transform pai, string texto, Vector3 pos)
        {
            Texto(pai, "Etiqueta_" + texto, texto, pos + new Vector3(0f, 0f, -0.0015f), 0.0022f, Hex("111111"),
                Quaternion.identity, Hex("ffffff"), new Vector2(0.036f, 0.018f));
        }

        static void Texto(Transform pai, string nome, string texto, Vector3 pos, float escala, Color cor,
            Quaternion? rotacao = null, Color? fundo = null, Vector2 tamanhoFundo = default)
        {
            var o = new GameObject(nome);
            o.transform.SetParent(pai, false);
            o.transform.localPosition = pos;
            o.transform.localRotation = rotacao ?? Quaternion.identity;
            if (fundo.HasValue)
            {
                var placa = Caixa(o.transform, "Fundo", new Vector3(0f, 0f, 0.0008f), new Vector3(tamanhoFundo.x, tamanhoFundo.y, 0.001f), "Etiqueta_" + ColorUtility.ToHtmlStringRGB(fundo.Value), fundo.Value);
                UnityEngine.Object.DestroyImmediate(placa.GetComponent<Collider>());
            }
            var t = new GameObject("Texto");
            t.transform.SetParent(o.transform, false);
            t.transform.localScale = Vector3.one * escala;
            var tm = t.AddComponent<TextMesh>();
            tm.text = texto;
            tm.font = s_Fonte;
            tm.fontSize = 48;
            tm.characterSize = 1f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = cor;
            t.GetComponent<MeshRenderer>().sharedMaterial = s_Fonte.material;
        }

        // Um material por tipo e cor (o mesmo tipo aparece em cores diferentes, ex.: plástico branco e preto)
        static Material Mat(string tipo, Color cor)
        {
            string nome = tipo.Contains("_") ? tipo : tipo + "_" + ColorUtility.ToHtmlStringRGB(cor);
            if (s_Materiais.TryGetValue(nome, out var existente)) return existente;
            var caminho = MateriaisPath + "/" + nome + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(caminho);
            if (material == null)
            {
                material = new Material(Shader.Find("Standard"));
                AssetDatabase.CreateAsset(material, caminho);
            }
            material.color = cor;
            bool metalico = tipo == "Metal" || tipo == "Parafuso";
            material.SetFloat("_Glossiness", metalico ? 0.5f : 0.25f);
            material.SetFloat("_Metallic", metalico ? 0.5f : 0f);
            EditorUtility.SetDirty(material);
            s_Materiais[nome] = material;
            return material;
        }

        static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString("#" + hex, out var cor);
            return cor;
        }
    }
}
#endif
