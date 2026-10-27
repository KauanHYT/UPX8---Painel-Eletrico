using System;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

namespace PainelAR
{
    public class RegistroExperimento
    {
        public string CaminhoArquivo { get; private set; }
        public float TempoDecorrido => Time.realtimeSinceStartup - _inicio;

        readonly string _participante;
        readonly float _inicio;

        public RegistroExperimento(string participante)
        {
            _participante = participante;
            _inicio = Time.realtimeSinceStartup;
            string nome = participante + "_RA_" + DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture) + ".csv";
            CaminhoArquivo = Path.Combine(Application.persistentDataPath, nome);
            // UTF-8 com BOM para o Excel mostrar os acentos corretamente
            File.WriteAllText(CaminhoArquivo,
                "participante;condicao;data_hora;tempo_s;evento;etapa;componente_solicitado;componente_indicado;correto;detalhe\n",
                new UTF8Encoding(true));
        }

        public void Registrar(string evento, int etapa = 0, string solicitado = "", string indicado = "", string correto = "", string detalhe = "")
        {
            // Vírgula decimal para o Excel em português
            string tempo = TempoDecorrido.ToString("F2", CultureInfo.InvariantCulture).Replace('.', ',');
            string linha = string.Join(";", new[]
            {
                _participante,
                "RA",
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                tempo,
                evento,
                etapa > 0 ? etapa.ToString() : "",
                solicitado,
                indicado,
                correto,
                Limpar(detalhe)
            });
            File.AppendAllText(CaminhoArquivo, linha + "\n", new UTF8Encoding(false));
            Debug.Log("[PainelAR] " + linha);
        }

        static string Limpar(string texto)
        {
            return string.IsNullOrEmpty(texto) ? "" : texto.Replace(";", ",").Replace("\n", " ");
        }
    }
}
