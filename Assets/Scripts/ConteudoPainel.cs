using System;
using UnityEngine;

namespace PainelAR
{
    [Serializable]
    public class ConteudoPainel
    {
        public string titulo;
        public string versao;
        public Componente[] componentes;
        public Etapa[] etapas;

        public Componente BuscarPorId(string id)
        {
            foreach (var c in componentes) if (c.id == id) return c;
            return null;
        }

        public Componente BuscarPorMarcador(string marcador)
        {
            foreach (var c in componentes) if (c.marcador == marcador) return c;
            return null;
        }

        // Lê Assets/Resources/conteudo_painel.json
        public static ConteudoPainel Carregar()
        {
            var arquivo = Resources.Load<TextAsset>("conteudo_painel");
            if (arquivo == null)
                throw new Exception("Não encontrei Assets/Resources/conteudo_painel.json");

            var conteudo = JsonUtility.FromJson<ConteudoPainel>(arquivo.text);
            if (conteudo == null || conteudo.componentes == null || conteudo.etapas == null || conteudo.etapas.Length == 0)
                throw new Exception("conteudo_painel.json está vazio ou com erro de formato");

            foreach (var etapa in conteudo.etapas)
                if (conteudo.BuscarPorId(etapa.componenteId) == null)
                    throw new Exception("Etapa " + etapa.numero + ": componente '" + etapa.componenteId + "' não existe no JSON");

            return conteudo;
        }
    }

    [Serializable]
    public class Componente
    {
        public string id;
        public string marcador;
        public string nome;
        public string funcao;
        public string alerta;
    }

    [Serializable]
    public class Etapa
    {
        public int numero;
        public string componenteId;
        public string titulo;
        public string instrucao;
        public string alerta;
    }
}
