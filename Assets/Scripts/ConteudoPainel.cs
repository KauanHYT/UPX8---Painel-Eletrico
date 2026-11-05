using System;
using System.Linq;
using UnityEngine;

namespace PainelAR
{
    [Serializable]
    public class ConteudoPainel
    {
        public string titulo;
        public string versao;
        public Componente[] componentes;
        public ConjuntoEtapas[] conjuntos;

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

        public ConjuntoEtapas BuscarConjunto(string id)
        {
            foreach (var c in conjuntos) if (c.id == id) return c;
            return null;
        }

        // Lê Assets/Resources/conteudo_painel.json
        public static ConteudoPainel Carregar()
        {
            var arquivo = Resources.Load<TextAsset>("conteudo_painel");
            if (arquivo == null)
                throw new Exception("Não encontrei Assets/Resources/conteudo_painel.json");

            var conteudo = JsonUtility.FromJson<ConteudoPainel>(arquivo.text);
            if (conteudo == null || conteudo.componentes == null || conteudo.conjuntos == null)
                throw new Exception("conteudo_painel.json está vazio ou com erro de formato");

            var a = conteudo.BuscarConjunto("A");
            var b = conteudo.BuscarConjunto("B");
            if (a == null || b == null || conteudo.conjuntos.Length != 2)
                throw new Exception("conteudo_painel.json precisa ter exatamente os conjuntos A e B");

            foreach (var conjunto in conteudo.conjuntos)
            {
                if (conjunto.etapas == null || conjunto.etapas.Length == 0)
                    throw new Exception("Conjunto " + conjunto.id + " sem etapas");
                for (int i = 0; i < conjunto.etapas.Length; i++)
                {
                    var etapa = conjunto.etapas[i];
                    if (etapa.numero != i + 1)
                        throw new Exception("Conjunto " + conjunto.id + ": etapa na posição " + (i + 1) + " está numerada como " + etapa.numero);
                    if (conteudo.BuscarPorId(etapa.componenteId) == null)
                        throw new Exception("Conjunto " + conjunto.id + ", etapa " + etapa.numero + ": componente '" + etapa.componenteId + "' não existe no JSON");
                }
            }

            // Tarefas equivalentes nas duas condições (Seção 3.5 do artigo)
            if (a.Perfil() != b.Perfil())
                throw new Exception("Conjuntos A e B não são equivalentes: " + a.Perfil() + " x " + b.Perfil());

            return conteudo;
        }
    }

    [Serializable]
    public class ConjuntoEtapas
    {
        public string id;
        public Etapa[] etapas;

        // Etapas por componente e total de alertas. Precisa ser igual nos dois conjuntos.
        public string Perfil()
        {
            var porComponente = etapas.GroupBy(e => e.componenteId).OrderBy(g => g.Key, StringComparer.Ordinal).Select(g => g.Key + "=" + g.Count());
            return string.Join(", ", porComponente) + ", alertas=" + etapas.Count(e => !string.IsNullOrEmpty(e.alerta));
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
