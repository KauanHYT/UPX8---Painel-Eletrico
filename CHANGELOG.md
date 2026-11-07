# Changelog

Evolução do Painel AR, a partir do histórico de commits do repositório.

## v0.2-dispositivo — 2026-11-07
- docs: testes técnicos no smartphone de 07/11/2026 registrados no README (Seção 22.3), com os CSVs das sessões em `docs/testes/sessoes/`; reconhecimento até cerca de 40 cm com marcadores de 8 cm.
- docs: README atualizado para o painel virtual em tela, os conjuntos A e B, as telas novas e os arquivos renomeados; validação por profissional retirada do escopo.

## 2026-11-05
- feat: conjuntos A e B no `conteudo_painel.json` (mesmas 15 tarefas em ordens diferentes), validação de equivalência e rodízio por participante.
- feat: coluna do conjunto no CSV e no nome do arquivo; manual, ficha e modelo de CSV da condição papel para cada conjunto; `analisar.ps1` com taxa por conjunto.
- feat: telas do app redesenhadas e menu para capturar as telas em `docs/images`; `INFO_EXIBIDA` só é gravado com o componente 0,3 s na mira.
- docs: planilha de testes técnicos em `docs/testes/Testes_Tecnicos.xlsx`.

## 2026-10-27 (preliminar)
- docs: teste preliminar no smartphone (Samsung Galaxy A56 5G, 27/10/2026) registrado no README; capturas das telas de etapa, alerta e resumo em `docs/images/`.

## v0.1-simulacao — 2026-10-31
- docs: README reescrito no modelo da disciplina, no enquadramento de estudo piloto, com controle de versão, testes e limitações.
- docs: diagramas de arquitetura, fluxo e caso de uso em `docs/diagrams/`; pasta `docs/documents/` para o artigo e os instrumentos de coleta.
- chore: remove arquivos de documentação herdados do WL03 (`TESTE_DE_ACEITACAO.md`, `docs/ARQUITETURA.md`, `docs/CUSTOMIZACAO.md`).

## 2026-10-27
- feat: manual impresso, ficha de observação da condição papel e script de análise dos dados (`tools/analisar.ps1`).
- feat: aplicativo do experimento (reconhecimento dos marcadores, mira, alertas, 15 etapas e registro em CSV) e painel 3D para a simulação.
- feat: três marcadores do painel (DJ1, K1 e FT1) e biblioteca de imagens de referência.

## 2026-10-24
- feat: teste no computador com o XR Simulation do AR Foundation.
- chore: criação do projeto Painel AR a partir do white label WL03 da disciplina.

## 2026-10-02
- Criação do repositório.
