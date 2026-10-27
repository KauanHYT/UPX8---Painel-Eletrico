# Coleta e análise dos dados

A variável principal é a **taxa de erros de identificação** (Seção 3.4 do artigo):

```
taxa de erros (%) = identificações incorretas / identificações solicitadas × 100
```

Cada participante faz 15 identificações em cada condição. Os dados das duas condições usam **as mesmas colunas** (Parte 11 do guia), para entrar juntos na análise.

## Materiais (gerados do mesmo `conteudo_painel.json` do app)

| Material | Arquivo | Como gerar de novo |
| --- | --- | --- |
| Manual impresso (condição papel) | `docs/impressao/Documentacao_Impressa.html` / `.pdf` | `Painel AR → Gerar documentação impressa` |
| Foto do painel com os componentes indicados | `docs/impressao/foto_painel.jpg` | `Painel AR → Gerar foto do painel (simulação)` |
| Ficha de observação do pesquisador | `docs/impressao/Ficha_Observacao_Papel.html` / `.pdf` | `Painel AR → Gerar ficha de observação (papel)` |
| Modelo de CSV da condição papel | `docs/analise/modelo_papel.csv` | gerado junto com a ficha |

Sempre que o JSON mudar (por exemplo, depois da revisão do profissional), gere os quatro de novo, reimprima e gere um novo APK.

A **foto atual é do painel virtual** da simulação. Antes do experimento, troque por uma foto frontal do painel didático real, com o nome de cada componente indicado (Parte 10 do guia), salva com o mesmo nome `foto_painel.jpg`.

## Durante o experimento

**Condição RA:** o app grava sozinho um CSV por sessão no celular (`Android/data/br.edu.facens.painelar/files`). Use o código do participante (P01, P02…) na tela inicial, nunca o nome (RNF08).

**Condição papel:** o participante usa o manual impresso; o pesquisador usa a **ficha de observação** (que tem o gabarito, então não entregue ao participante):

1. Preencha código, data, ordem das condições e familiaridade com painéis.
2. Anote a hora de início, cronometre a tarefa e, em cada etapa, marque qual componente o participante indicou.
3. Anote o tempo total em segundos.

## Depois de cada sessão

1. **RA:** copie os CSVs do celular para uma pasta do PC, por exemplo `C:\AR\dados` (Parte 11.1 do guia; com o celular no cabo, `adb pull /sdcard/Android/data/br.edu.facens.painelar/files/ C:\AR\dados`).
2. **Papel:** copie `docs/analise/modelo_papel.csv` para a mesma pasta com o nome do participante (ex.: `P01_PAPEL.csv`) e abra no Excel:
   - troque todos os `Pxx` pelo código do participante;
   - em cada linha `IDENTIFICACAO`, preencha `componente_indicado` com o que foi marcado na ficha (`DJ1`, `K1`, `FT1` ou `outro`);
   - na linha `SESSAO_FIM`, preencha `tempo_s` com o tempo total em segundos;
   - a coluna `correto` pode ficar em branco: o script compara o indicado com o solicitado;
   - salve como **CSV (separado por ponto e vírgula)**.
3. Os CSVs dos testes técnicos e de ensaio **não** devem ficar nessa pasta (Parte 12 do guia).

## Calcular

```
powershell -ExecutionPolicy Bypass -File tools\analisar.ps1 -Pasta C:\AR\dados
```

O script lê todos os CSVs da pasta e mostra, para cada participante e condição: identificações, acertos, erros, taxa de erros e tempo. Gera em `C:\AR\dados\resultados\`:

- `resumo.csv`: uma linha por participante e condição;
- `comparativo.csv`: uma linha por participante, com a taxa na RA e no papel lado a lado, a diferença (RA − papel) e em qual condição houve menos erros.

No fim, mostra a taxa agregada de cada condição (soma dos erros ÷ soma das identificações). Como a amostra é pequena (2 a 4 participantes), a análise é descritiva, sem teste de significância (Seção 3.10 do artigo): apresente os valores individuais e as porcentagens.

Avisos que o script pode dar:

- **"código do participante não preenchido (Pxx)"**: faltou trocar o `Pxx` no CSV do papel;
- **"etapa(s) sem componente indicado"**: alguma linha `IDENTIFICACAO` ficou sem resposta; ela fica fora do total, então confira a ficha.
