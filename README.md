# AR Maintenance Guide

> **Projeto acadêmico — UPX: Realidade Aumentada**
> Aplicação móvel de Realidade Aumentada baseada em marcadores de imagem (ARCore) para apoio à identificação de componentes e à execução guiada de procedimentos de manutenção em painéis elétricos didáticos.

![Status](https://img.shields.io/badge/status-em%20desenvolvimento-yellow)
![Plataforma](https://img.shields.io/badge/plataforma-Android%20%2B%20ARCore-green)
![Tecnologia](https://img.shields.io/badge/RA-Marcadores%20de%20imagem-blue)
![Motor](https://img.shields.io/badge/Unity-6000.6.7f1-black)
![Projeto](https://img.shields.io/badge/UPX-2026.2-purple)

---

## 1. Identificação do Projeto

| Item            | Informação                                        |
| --------------- | ------------------------------------------------- |
| **Projeto**     | AR Maintenance Guide (aplicativo: **Painel AR**)  |
| **Curso**       | Engenharia de Computação                          |
| **Instituição** | Centro Universitário Facens                       |
| **Disciplina**  | UPX — Realidade Aumentada                         |
| **Semestre**    | 2026.2                                            |
| **Equipe**      | Enrico Affonso, Guilherme Ifanger e Kauan Michael |
| **Professor**   | Preencher com o nome do professor                 |

### Integrantes

| Integrante        | Responsabilidade      |
| ----------------- | --------------------- |
| Enrico Affonso    | A definir pela equipe |
| Guilherme Ifanger | A definir pela equipe |
| Kauan Michael     | A definir pela equipe |

> As responsabilidades individuais deverão ser atualizadas conforme a divisão efetiva das atividades.

---

## 2. Visão Geral

O **AR Maintenance Guide** é uma prova de conceito de uma aplicação móvel de Realidade Aumentada (RA) destinada a apoiar a identificação de componentes e a execução guiada de procedimentos de manutenção em um painel elétrico didático desenergizado.

A aplicação foi desenvolvida em **Unity** com **AR Foundation** e **ARCore**. Em vez dos marcadores ArUco previstos na proposta inicial, ela usa **marcadores de imagem** reconhecidos pelo rastreamento de imagens do ARCore (ver Seção 8). Cada marcador é colado ao lado de um componente selecionado e tem um nome único associado a esse componente no arquivo de conteúdo.

Ao reconhecer um marcador pela câmera do smartphone, a aplicação destaca o marcador na tela, mostra o nome do componente e, quando ele está na mira, apresenta a função e o alerta de segurança. O procedimento guiado tem **15 etapas**; cada uma exibe o alerta (quando houver) e a instrução, destaca em laranja o componente solicitado e registra se o participante identificou o componente correto.

Além do artefato, o projeto prevê uma avaliação experimental que compara o uso da aplicação com documentação impressa de conteúdo equivalente. O principal indicador é a **taxa de erros na identificação dos componentes**. A documentação impressa é gerada do mesmo arquivo de conteúdo da aplicação.

**Escopo:** a aplicação foi desenvolvida para um painel didático previamente configurado. Não reconhece automaticamente componentes de painéis desconhecidos nem orienta intervenções em instalações energizadas.

> **Estado atual (28/10/2026):** a aplicação está implementada e foi testada no PC, num painel virtual, com o XR Simulation do AR Foundation (Seção 19). **Ainda não foi testada em um smartphone**, porque a equipe ainda não dispõe de um aparelho compatível com ARCore, e o painel didático real ainda não foi preparado.

---

## 3. Problema

A identificação de componentes elétricos é uma etapa importante na execução de procedimentos de manutenção. Pessoas em treinamento ou com pouca familiaridade com determinado painel podem apresentar dificuldades para localizar os componentes solicitados e interpretar as informações disponíveis em etiquetas, manuais e checklists.

A consulta à documentação tradicional exige que o usuário relacione as instruções escritas aos componentes físicos presentes no painel. Uma apresentação contextual das informações pode facilitar essa associação, embora sua eficácia precise ser avaliada experimentalmente.

O projeto investiga se uma aplicação de Realidade Aumentada baseada em marcadores pode contribuir para reduzir erros de identificação em comparação com documentação impressa equivalente, em um cenário de treinamento controlado e seguro.

---

## 4. Objetivos

### 4.1 Objetivo geral

Desenvolver e avaliar uma prova de conceito de aplicação móvel de Realidade Aumentada baseada em marcadores para apoiar a identificação de componentes e a execução guiada de um procedimento de manutenção em um painel elétrico didático desenergizado.

### 4.2 Objetivos específicos

| Objetivo | Situação |
| --- | --- |
| Investigar trabalhos relacionados ao uso de RA em manutenção industrial e elétrica | Feito no artigo |
| Definir os requisitos funcionais e não funcionais | Feito (Seções 6 e 7) |
| Delimitar o painel, os componentes e o procedimento | Feito com conteúdo **provisório**: 3 componentes (DJ1, K1, FT1) e 15 etapas, aguardando revisão técnica |
| Implementar o reconhecimento de marcadores previamente cadastrados | Feito (marcadores de imagem do ARCore) |
| Associar os marcadores aos componentes | Feito |
| Apresentar informações contextuais e alertas de segurança | Feito |
| Implementar o procedimento guiado com etapas | Feito |
| Registrar os dados necessários à análise | Feito (CSV por sessão) |
| Executar testes técnicos de reconhecimento, estabilidade e desempenho | Feito **só na simulação**; testes no celular pendentes |
| Comparar experimentalmente a aplicação com documentação impressa | Materiais prontos; coleta pendente |
| Analisar a taxa de erros e o tempo | Script de análise pronto; aguarda os dados reais |

---

## 5. Público-alvo

O projeto tem como público de interesse pessoas em treinamento e usuários com pouca familiaridade com os componentes de um painel elétrico didático.

Na avaliação experimental, pretende-se utilizar de 2 a 4 participantes recrutados entre estudantes de Engenharia de Computação, conforme a disponibilidade da equipe.

A familiaridade prévia com painéis elétricos e procedimentos de manutenção é registrada na ficha de observação para caracterizar os participantes.

Os resultados obtidos não serão automaticamente generalizados para profissionais eletricistas ou para situações reais de manutenção.

---

## 6. Escopo do MVP

A primeira versão da aplicação prioriza somente as funcionalidades necessárias à execução do experimento.

### Funcionalidades previstas

| ID   | Funcionalidade              | Descrição e implementação | Status |
| ---- | --------------------------- | ------------------------- | ------ |
| RF01 | Captura de imagens          | Câmera do smartphone pela cena de RA (AR Session + XR Origin) | Implementado; testado só na simulação |
| RF02 | Reconhecimento de marcadores | Reference Image Library com 3 marcadores + `RastreadorMarcadores` | Implementado; testado só na simulação |
| RF03 | Identificação do componente | Marcador mais próximo da mira + botão **Confirmar componente** | Implementado; testado só na simulação |
| RF04 | Exibição de informações     | Nome do componente sobre o marcador; nome, função e alerta no cartão "Na mira" | Implementado; testado só na simulação |
| RF05 | Alertas de segurança        | Tela de alerta obrigatória antes das etapas que têm alerta | Implementado; testado só na simulação |
| RF06 | Procedimento guiado         | 15 etapas do `conteudo_painel.json`, com o componente pedido destacado em laranja | Implementado; testado só na simulação |
| RF07 | Registro de identificações  | `RegistroExperimento`: um CSV por sessão, gravado linha a linha | Implementado; testado só na simulação |
| RF08 | Registro de eventos         | Eventos de sessão, etapa, alerta, marcador visível, informação exibida e identificação (correto 1/0) | Implementado; testado só na simulação |

> "Testado só na simulação" significa que a funcionalidade passou no teste automático do XR Simulation no PC (Seção 19), mas ainda não foi verificada num smartphone com marcadores impressos.

### Funcionalidades fora do escopo inicial

Para manter o projeto viável no prazo disponível, não fazem parte do MVP inicial:

* Reconhecimento automático de componentes em painéis desconhecidos.
* Inteligência Artificial para interpretação de componentes.
* Integração com sensores ou sistemas IoT.
* Serviços de nuvem ou sincronização remota.
* Gerenciamento de múltiplos painéis não cadastrados.
* Diagnóstico automático de falhas elétricas.
* Execução ou controle físico de equipamentos elétricos.
* Uso da aplicação como substituta de treinamento técnico, normas de segurança ou supervisão profissional.

Funcionalidades adicionais somente serão consideradas se o núcleo da aplicação e a avaliação experimental estiverem concluídos (ver Seção 27).

---

## 7. Requisitos não funcionais

| ID    | Categoria                | Requisito | Como foi atendido |
| ----- | ------------------------ | --------- | ----------------- |
| RNF01 | Plataforma               | Executar no dispositivo Android selecionado e validado pela equipe | APK Android (ARM64, IL2CPP, ARCore obrigatório). Dispositivo ainda não definido |
| RNF02 | Rastreamento             | Reconhecer os marcadores nas condições de teste definidas | Marcadores com nota ≥ 75 no `arcoreimg` (Seção 11.1); testes de distância, ângulo e luz pendentes |
| RNF03 | Desempenho               | Manter desempenho suficiente durante a tarefa | Contador de FPS na tela; referência de 15 FPS a verificar no celular |
| RNF04 | Estabilidade             | Executar a sessão sem falhas que impeçam a continuidade | CSV gravado a cada evento (nada se perde se o app fechar); sessões completas na simulação |
| RNF05 | Disponibilidade          | Executar a tarefa sem internet | Conteúdo dentro do APK; nada é baixado durante o uso |
| RNF06 | Usabilidade              | Informações legíveis e interação compreensível | Fonte proporcional à largura da tela, botões grandes na parte de baixo |
| RNF07 | Equivalência de conteúdo | Informações equivalentes na aplicação e no impresso | App, manual impresso e ficha gerados do mesmo `conteudo_painel.json` |
| RNF08 | Segurança                | Painel desenergizado e conteúdo técnico revisado | Alertas no conteúdo; **revisão técnica ainda pendente** (conteúdo versão `0.1-provisoria`) |
| RNF09 | Privacidade              | Evitar identificação pessoal dos participantes | Participantes identificados só por código (P01, P02…), sem nome |

Os critérios quantitativos de aceitação propostos estão na Seção 19 e precisam ser confirmados pela equipe antes dos testes no celular.

---

## 8. Tecnologias utilizadas

A proposta inicial previa Flutter, Dart, OpenCV e marcadores ArUco. Seguindo a Seção 3.8 do artigo e o guia de implementação do grupo, a aplicação foi feita em Unity com AR Foundation e ARCore, partindo do projeto-base **WL03** da disciplina, que já tinha a configuração Android/ARCore funcionando.

| Tecnologia | Versão | Utilização |
| --- | --- | --- |
| Unity Editor | 6000.6.7f1 | Motor e editor do projeto (licença Personal) |
| AR Foundation | 6.6.2 | Camada de RA: sessão, câmera e rastreamento de imagens |
| ARCore XR Plugin | 6.6.2 | Provedor de RA no Android |
| XR Plugin Management | 4.7.0 | Ativação do ARCore (Android) e do XR Simulation (PC) |
| XR Simulation (do AR Foundation) | 6.6.2 | Testes no PC com painel virtual |
| Input System | 1.20.1 | Navegação no XR Simulation (o projeto usa "Both") |
| C# | — | Scripts da aplicação e do editor |
| JSON local (`Resources`) | — | Componentes, alertas e etapas |
| CSV | — | Registro dos eventos experimentais |
| PowerShell 5.1 | — | Automação do Unity e análise dos dados |
| Git | — | Controle de versão (repositório local) |

**Por que marcadores de imagem e não ArUco:** o rastreamento de imagens do ARCore precisa de imagens com muitos detalhes e sem simetria. Marcadores ArUco são quadrados simples em preto e branco, com poucos pontos de referência, e o próprio guia desaconselha esse tipo de padrão. Por isso foram criadas imagens próprias (Seção 11.1).

Nenhum servidor é necessário: o processamento e os dados ficam no dispositivo.

---

## 9. Hardware e ambiente de desenvolvimento

| Equipamento              | Modelo/configuração | Utilização |
| ------------------------ | ------------------- | ---------- |
| Computador               | Windows 11 Pro, Intel Core i7-13700, 32 GB de RAM, NVIDIA GeForce RTX 4060 | Desenvolvimento, simulação e geração do APK |
| Smartphone Android       | **Ainda não definido.** O aparelho testado inicialmente não é compatível com ARCore | Execução da aplicação |
| Painel elétrico didático | **Ainda não preparado.** Usado um painel virtual na simulação | Ambiente físico do experimento |
| Marcadores               | 3 imagens de 8 × 8 cm (`marcador_DJ1`, `marcador_K1`, `marcador_FT1`), ainda não impressas | Referências visuais dos componentes |

### Pré-requisitos

* Windows 10/11 64 bits.
* Unity Hub com o editor **6000.6.7f1** e os módulos **Android Build Support**, **OpenJDK** e **Android SDK & NDK Tools**.
* Projeto numa pasta **sem espaços nem acentos** (usado `C:\AR\PainelAR\painel-ar`).
* Smartphone Android **compatível com ARCore**, com o **Google Play Services for AR** instalado.
* Painel didático desenergizado, com os marcadores impressos em tamanho real e colados ao lado dos componentes.

---

## 10. Arquitetura do sistema

A aplicação é organizada em captura, reconhecimento, consulta ao conteúdo local, apresentação, procedimento guiado e registro dos dados experimentais.

```text
Painel elétrico didático (marcadores colados ao lado dos componentes)
        |
        v
Câmera do smartphone (AR Session + XR Origin)
        |
        v
ARCore: rastreamento de imagens (AR Tracked Image Manager
        + PainelReferenceImageLibrary)
        |
        v
RastreadorMarcadores: nome do marcador, destaque e marcador na mira
        |
        v
ConteudoPainel: conteudo_painel.json (componentes, alertas, etapas)
        |
        v
ProcedimentoController (Início → Alerta → Identificação → Concluído)
   |                         |
   v                         v
InterfaceApp              RegistroExperimento
(telas, mira, FPS)        (CSV por sessão)
                             |
                             v
                tools/analisar.ps1 (taxa de erros)

conteudo_painel.json --> GeradorDocumentoImpresso --> manual impresso e ficha de papel
```

O mesmo `conteudo_painel.json` alimenta o app, o manual impresso e a ficha de observação, o que garante a equivalência de conteúdo (RNF07).

Uma identificação é considerada correta quando o componente do marcador que está na mira, no momento da confirmação, é o componente solicitado na etapa. Na condição papel, o pesquisador marca a resposta numa ficha e o script compara com o gabarito.

**Figura da arquitetura:** a criar em `docs/diagrams/arquitetura.svg`. Por enquanto, vale o diagrama em texto acima.

---

## 11. Funcionamento da Realidade Aumentada

### 11.1 Marcadores

| Nome na biblioteca | Componente | Tamanho |
| --- | --- | --- |
| `marcador_DJ1` | Disjuntor geral DJ1 | 8 × 8 cm |
| `marcador_K1` | Contator K1 | 8 × 8 cm |
| `marcador_FT1` | Relé de sobrecarga FT1 | 8 × 8 cm |

* As imagens são **geradas por script** (`Painel AR → Marcadores → Gerar imagens`), com semente fixa: gerar de novo produz as mesmas imagens. Cada uma tem paleta própria, formas em várias escalas e nenhuma simetria. Não têm o nome do componente, porque os marcadores ficam no painel nas duas condições do experimento.
* A `PainelReferenceImageLibrary` cadastra as 3 imagens com Physical Size de 0,08 m. O AR Tracked Image Manager acompanha até 3 imagens ao mesmo tempo.
* **Qualidade:** no build Android, o plugin ARCore do Unity avalia cada imagem com o `arcoreimg eval-img` e avisa quando a nota fica abaixo de 75, o mínimo recomendado pelo Google. O build de 27/10/2026 não gerou aviso, ou seja, as três tiveram nota ≥ 75. O valor exato de cada nota não aparece no log.
* A folha de impressão está em `docs/impressao/marcadores_para_imprimir.pdf` (A4, tamanho real, com régua de 5 cm para conferência). Detalhes em `docs/MARCADORES.md`.

O reconhecimento de um marcador apenas localiza o registro correspondente no conteúdo. A aplicação não interpreta automaticamente componentes elétricos.

### 11.2 Fluxo técnico

1. O pesquisador escolhe o código do participante (P01, P02…) e toca em **Iniciar sessão**.
2. A câmera captura o painel e o ARCore procura as imagens da biblioteca.
3. Para cada marcador reconhecido, o `RastreadorMarcadores` desenha uma placa semitransparente e o nome do componente acima dela. O componente pedido na etapa fica **laranja**; os outros, **azuis**.
4. Se a etapa tem alerta, a tela vermelha de alerta aparece primeiro e só some com **Li e entendi**.
5. Quando um marcador fica perto da **mira** (cruz um pouco acima do centro da tela), a cruz fica verde e o cartão "Na mira" mostra nome, função e alerta do componente.
6. O participante toca em **Confirmar componente**. O app compara o componente na mira com o solicitado, grava a identificação e passa para a próxima etapa, sem dizer ao participante se acertou.
7. No fim das 15 etapas, a tela final mostra acertos, erros, taxa de erros, tempo e onde o CSV foi salvo.

### 11.3 Informações apresentadas

* Nome do componente (sobre o marcador e no cartão "Na mira").
* Função no painel.
* Alerta de segurança do componente.
* Alerta da etapa, quando houver.
* Etapa atual ("Etapa X de 15"), título e instrução.
* Destaque do componente solicitado (pode ser desligado no Inspector, em *Procedimento Controller → Destacar Componente Da Etapa*).

O conteúdo técnico atual é **provisório** (versão `0.1-provisoria`) e precisa ser revisado por um profissional com formação ou experiência na área elétrica antes do experimento.

---

## 12. Estrutura do repositório

```text
painel-ar/
│
├── Assets/
│   ├── Editor/                  # automação do editor (não vai para o celular)
│   │   ├── PainelARSetup.cs
│   │   ├── PainelARSimulacao.cs
│   │   ├── GeradorMarcadores.cs
│   │   ├── GeradorDocumentoImpresso.cs
│   │   └── TesteSimulacao.cs
│   ├── Scripts/                 # código da aplicação
│   │   ├── ConteudoPainel.cs
│   │   ├── RegistroExperimento.cs
│   │   ├── RastreadorMarcadores.cs
│   │   ├── ProcedimentoController.cs
│   │   └── InterfaceApp.cs
│   ├── Resources/conteudo_painel.json
│   ├── Markers/                 # marcador_DJ1.png, marcador_K1.png, marcador_FT1.png
│   ├── ReferenceImages/PainelReferenceImageLibrary.asset
│   ├── Scenes/WL03ContextualAR.unity
│   ├── Simulacao/               # painel virtual do XR Simulation e materiais
│   └── XR/                      # configurações do ARCore e do XR Simulation
│
├── docs/
│   ├── TESTE_NO_PC.md
│   ├── MARCADORES.md
│   ├── ANALISE.md
│   ├── analise/modelo_papel.csv
│   ├── impressao/               # marcadores, manual impresso, ficha e foto do painel
│   ├── images/                  # capturas da simulação
│   ├── ARQUITETURA.md           # herdado do WL03, desatualizado
│   └── CUSTOMIZACAO.md          # herdado do WL03, desatualizado
│
├── tools/
│   ├── unity.ps1                # roda o Unity em linha de comando
│   └── analisar.ps1             # taxa de erros por participante e condição
│
├── Packages/
├── ProjectSettings/
├── Builds/                      # APK gerado (fora do Git)
├── CHANGELOG.md                 # herdado do WL03, desatualizado
├── TESTE_DE_ACEITACAO.md        # herdado do WL03, desatualizado
├── .gitignore
└── README.md
```

### Finalidade das pastas

| Pasta | Finalidade |
| --- | --- |
| `Assets/Scripts` | Código que vai para o celular: conteúdo, rastreamento, procedimento, interface e registro |
| `Assets/Editor` | Menus **Painel AR** do editor: configurar e validar o projeto, gerar marcadores, simulação, documentos e APK |
| `Assets/Resources` | `conteudo_painel.json`, lido pelo app em tempo de execução |
| `Assets/Markers` | Imagens dos marcadores |
| `Assets/ReferenceImages` | Biblioteca de imagens do ARCore |
| `Assets/Simulacao` | Painel virtual usado pelo XR Simulation no PC (gerado por script) |
| `docs` | Documentação, materiais de impressão e capturas |
| `tools` | Scripts de linha de comando |

A cena ainda se chama `WL03ContextualAR`, nome herdado do projeto-base. Os arquivos marcados como herdados do WL03 ainda não foram atualizados.

---

## 13. Organização dos principais módulos

| Módulo | Responsabilidade |
| --- | --- |
| `ConteudoPainel` | Lê o `conteudo_painel.json`, guarda componentes e etapas e valida se cada etapa aponta para um componente existente |
| `RastreadorMarcadores` | Recebe os marcadores do ARCore, desenha o destaque e o nome, e diz qual marcador está na mira |
| `ProcedimentoController` | Fases da sessão (Início, Alerta, Identificação, Concluído), contagem de acertos e erros e envio dos eventos ao registro |
| `InterfaceApp` | Telas, botões, mira e contador de FPS (desenhados por código com IMGUI) |
| `RegistroExperimento` | Cria um CSV por sessão e grava uma linha por evento |
| `Componente`, `Etapa` | Representam um componente e uma etapa do procedimento |
| `PainelARSetup` (editor) | Configura nome e pacote do app, monta a cena, valida o projeto e gera o APK |
| `GeradorMarcadores` (editor) | Gera as imagens dos marcadores e a biblioteca de imagens |
| `PainelARSimulacao` (editor) | Liga o XR Simulation e gera o painel virtual com os marcadores |
| `GeradorDocumentoImpresso` (editor) | Gera manual impresso, foto do painel e ficha de observação a partir do JSON |
| `TesteSimulacao` (editor) | Teste automático de uma sessão completa no XR Simulation |

---

## 14. Dados da aplicação

### Conteúdo (`Assets/Resources/conteudo_painel.json`)

| Entidade | Campos |
| --- | --- |
| Conteúdo | `titulo`, `versao` |
| Componente | `id`, `marcador` (nome na biblioteca de imagens), `nome`, `funcao`, `alerta` |
| Etapa | `numero`, `componenteId`, `titulo`, `instrucao`, `alerta` (vazio = sem tela de alerta) |

Procedimento atual: **Inspeção do circuito de comando do motor**, com 15 etapas; cada componente (DJ1, K1, FT1) é solicitado em 5 delas.

### Registro experimental (CSV, separador `;`)

`participante;condicao;data_hora;tempo_s;evento;etapa;componente_solicitado;componente_indicado;correto;detalhe`

| Evento | Quando acontece | Requisito |
| --- | --- | --- |
| `SESSAO_INICIO` / `SESSAO_FIM` | Início da sessão / fim da etapa 15 (com o resumo) | RF01, RF07 |
| `ETAPA_INICIO` | Começa uma etapa | RF06 |
| `ALERTA_EXIBIDO` / `ALERTA_CONFIRMADO` | Tela de alerta aparece / toque em "Li e entendi" | RF05 |
| `MARCADOR_VISIVEL` | Um marcador passa a ser visto pela câmera | RF02 |
| `INFO_EXIBIDA` | Muda o componente na mira | RF04 |
| `IDENTIFICACAO` | Participante confirma um componente; `correto` = 1 ou 0 | RF03, RF08 |
| `CONFIRMACAO_SEM_MARCADOR` | Confirmação sem marcador na mira (não conta como identificação) | Diagnóstico |

O participante é identificado só por código. No celular, os arquivos ficam em `Android/data/br.edu.facens.painelar/files`.

---

## 15. Configuração do ambiente

### Pré-requisitos

* Unity Hub e o editor **6000.6.7f1** com os módulos Android (Seção 9).
* PowerShell 5.1 (já vem no Windows).
* Smartphone Android compatível com ARCore e com depuração USB ativada, para instalar pelo cabo.

### Procedimento inicial

1. Copie o projeto para uma pasta sem espaços nem acentos, por exemplo `C:\AR\PainelAR\painel-ar`.
2. No Unity Hub: **Projects → Add → Add project from disk** e escolha a pasta.
3. Abra com o editor **6000.6.7f1**. A primeira abertura importa os pacotes e leva alguns minutos.
4. Use o menu **Painel AR → Validar projeto** e confira `[PainelAR] VALIDACAO OK` no Console.

Também dá para operar o projeto em linha de comando, com o Unity **fechado**:

```bash
powershell -ExecutionPolicy Bypass -File tools\unity.ps1 ValidarProjeto
```

```bash
powershell -ExecutionPolicy Bypass -File tools\unity.ps1 GerarApk
```

O `tools\unity.ps1` já usa uma pasta temporária curta (`C:\AR\tmp`), necessária para o Gradle (Seção 26).

| Menu **Painel AR** | O que faz |
| --- | --- |
| Configurar projeto | Nome e pacote do app, monta a cena (rastreador + App) |
| Validar projeto | Confere cena, ARCore, pacote e JSON × biblioteca |
| Gerar APK / Gerar APK e instalar no celular | Gera `Builds/PainelAR.apk` |
| Marcadores → Gerar imagens / Criar biblioteca e ligar à cena | Fase de marcadores |
| Simulação → Configurar simulação no PC | Liga o XR Simulation e gera o painel virtual |
| Gerar documentação impressa / Gerar ficha de observação (papel) / Gerar foto do painel (simulação) | Materiais da condição papel |

---

## 16. Como executar a aplicação

### No smartphone

1. Gere o APK (**Painel AR → Gerar APK**). Ele fica em `Builds/PainelAR.apk`.
2. Instale no celular de uma destas formas:
   * pelo cabo, com depuração USB ligada: **Painel AR → Gerar APK e instalar no celular**;
   * copiando o arquivo `.apk` para o celular e abrindo-o (é preciso permitir a instalação de apps de fontes desconhecidas).
3. Abra o **Painel AR** e permita o uso da câmera.
4. Aponte a câmera para os marcadores impressos.

> **Ainda não executado em um smartphone.** O APK compila, mas a instalação e o uso no aparelho não foram verificados.

### No PC, sem celular (XR Simulation)

1. Abra o projeto no Unity e clique em **Play**.
2. Segure o botão direito do mouse e use **W A S D** para andar pelo painel virtual (**E**/**Q** sobem e descem).
3. Clique nos botões do app com o botão esquerdo.

Passo a passo completo em `docs/TESTE_NO_PC.md`.

---

## 17. Como utilizar a aplicação

1. Abrir o **Painel AR**.
2. Escolher o código do participante com **−** e **+** e tocar em **Iniciar sessão**.
3. Ler o alerta de segurança, quando aparecer, e tocar em **Li e entendi**.
4. Ler a instrução da etapa e localizar o marcador destacado em **laranja**.
5. Colocar a **cruz** sobre o marcador (não sobre o componente) até ela ficar verde e aparecer "Na mira".
6. Tocar em **Confirmar componente**.
7. Repetir até a etapa 15 e conferir o resumo final.
8. Tocar em **Próximo participante** para a sessão seguinte.

Atenção para o protocolo: o app identifica o **marcador** colado ao lado do componente, não o componente em si. Isso deve fazer parte das instruções padrão dadas aos participantes.

**Capturas de tela:** `docs/images/` (da simulação no PC; ver Seção 22).

---

## 18. Segurança e limitações de uso

A aplicação é um protótipo acadêmico para treinamento e avaliação em ambiente controlado. Não deve ser considerada um sistema certificado de segurança elétrica nem uma ferramenta de autorização para intervenções em instalações reais.

As seguintes condições deverão ser respeitadas:

* Utilizar exclusivamente um painel elétrico didático desenergizado durante o experimento.
* Submeter o conteúdo técnico e as instruções à revisão de profissional qualificado (pendente).
* Definir previamente as condições de segurança e o protocolo de execução.
* Não utilizar a aplicação como substituta de normas, procedimentos oficiais, treinamento ou supervisão profissional.
* Não presumir que o reconhecimento de um marcador confirma que um componente está eletricamente seguro.
* Não realizar intervenções em instalações energizadas como parte da avaliação acadêmica.

O escopo do sistema restringe-se aos componentes e procedimentos previamente cadastrados.

---

## 19. Testes técnicos

### 19.1 Testes já realizados (PC, XR Simulation)

| Teste | Procedimento | Resultado |
| --- | --- | --- |
| Validação do projeto | `Painel AR → Validar projeto` | `VALIDACAO OK` |
| Compilação Android | `Painel AR → Gerar APK` | APK gerado (build de 27/10/2026) |
| Qualidade dos marcadores | Avaliação do `arcoreimg` no build | Sem aviso de nota abaixo de 75 |
| Reconhecimento | 3 marcadores no painel virtual, câmera a 75 cm | 3 de 3 reconhecidos |
| Associação | Mira em cada marcador | Componente correto em todas as etapas |
| Procedimento e registro | Sessão automática de 15 etapas com erros propositais nas etapas 4 e 12 | 13 acertos, 2 erros, taxa de 13,3%; CSV com 15 identificações e `correto = 0` só nas etapas 4 e 12 |

O teste automático pode ser repetido com o Unity fechado:

```bash
powershell -ExecutionPolicy Bypass -File tools\unity.ps1 TesteSimulacao.Executar -SemSair
```

Esses testes verificam a lógica do app. Eles **não** medem o reconhecimento real, porque a simulação não processa a imagem da câmera.

### 19.2 Testes pendentes (smartphone e painel real)

Critérios propostos no guia de implementação (Parte 12), **a confirmar pela equipe antes dos testes**:

| Teste | Como fazer | Critério proposto | Requisito |
| --- | --- | --- | --- |
| Inicialização e câmera | Abrir o app e iniciar a sessão | Sem falha | RF01 |
| Reconhecimento por distância | Cada marcador a 30, 50 e 80 cm, 10 vezes cada | 10/10 a 30 e 50 cm; 9/10 a 80 cm | RNF02 |
| Reconhecimento por ângulo | A 50 cm, a 0°, 30° e 45° | 9/10 em cada ângulo | RNF02 |
| Iluminação | A 50 cm, com luz normal e reduzida | 9/10 em cada condição | RNF02 |
| Associação | Mira em cada marcador e conferência do "Na mira" | Nenhuma associação errada | RF02, RNF02 |
| Desempenho | FPS no canto da tela durante uma sessão | Sempre ≥ 15 FPS | RNF03 |
| Estabilidade | 3 sessões completas seguidas | Nenhum travamento ou reinício | RNF04 |
| Sem internet | Modo avião durante uma sessão | Sessão completa | RNF05 |
| Registro | Conferir os CSVs dessas sessões | 15 linhas `IDENTIFICACAO` por sessão, erros iguais aos cometidos | RF07, RF08 |

Os testes técnicos não substituem o experimento e não constituem, isoladamente, evidência de confirmação da hipótese científica. Os CSVs dos testes devem ser separados dos dados reais antes da primeira sessão.

---

## 20. Avaliação experimental

### 20.1 Objetivo

Investigar o efeito do uso da aplicação de RA baseada em marcadores, em comparação com documentação impressa, sobre a taxa de erros de identificação de componentes em uma tarefa controlada.

### 20.2 Condições de comparação

| Condição | Descrição | Material |
| --- | --- | --- |
| Aplicação de RA | O participante usa o app para consultar informações e seguir o procedimento | `Builds/PainelAR.apk` |
| Documentação impressa | O participante usa o manual impresso com o mesmo conteúdo | `docs/impressao/Documentacao_Impressa.pdf` |

O manual traz a foto do painel com os componentes indicados, a tabela dos componentes (função e alerta) e as 15 etapas com componente, instrução, alerta e uma coluna "OK". A foto atual é do **painel virtual** e deve ser trocada por uma foto do painel real antes do experimento.

### 20.3 Variável principal

Taxa de erros de identificação de componentes. Um erro ocorre quando o participante indica um componente diferente do solicitado na etapa.

$$
\text{Taxa de erros (\%)} =
\frac{\text{Identificações incorretas}}
{\text{Total de identificações solicitadas}}
\times 100
$$

Cada participante faz **15 identificações** em cada condição (3 componentes, 5 vezes cada).

### 20.4 Medidas complementares

* Tempo para concluir a tarefa (registrado pelo app; cronometrado na condição papel).
* Quantidade de identificações corretas.
* Dificuldades observadas (campo de observações da ficha).
* Relatos dos participantes sobre a experiência de uso.

### 20.5 Participantes e procedimento

* De 2 a 4 participantes, cada um nas duas condições, com a **ordem alternada** entre participantes para reduzir efeitos de aprendizagem.
* Decisões de protocolo já embutidas no app:
  1. uma identificação por etapa, sem nova tentativa;
  2. o app não diz ao participante se acertou; mostra só o resumo no fim, como na condição papel;
  3. o componente pedido é destacado em laranja (pode ser desligado no Inspector).
* Análise descritiva, com os valores individuais e as porcentagens, sem teste de significância (Seção 3.10 do artigo).

---

## 21. Registro e análise dos dados

| Campo do CSV | Descrição |
| --- | --- |
| `participante` | Código anônimo (P01, P02…) |
| `condicao` | `RA` ou `PAPEL` |
| `etapa` | Número da etapa (1 a 15) |
| `componente_solicitado` | Gabarito da etapa |
| `componente_indicado` | Componente indicado pelo participante |
| `correto` | 1 ou 0 (no papel pode ficar em branco: o script calcula) |
| `tempo_s` | Tempo desde o início da sessão; na linha `SESSAO_FIM`, o tempo total |
| `detalhe` | Texto do alerta, resumo da sessão etc. |

**Condição RA:** o app grava o CSV sozinho.

**Condição papel:** o pesquisador usa a **ficha de observação** (`docs/impressao/Ficha_Observacao_Papel.pdf`), que tem o gabarito e não deve ser entregue ao participante. Depois, passa as respostas para uma cópia do `docs/analise/modelo_papel.csv`.

**Análise:** com todos os CSVs numa pasta:

```bash
powershell -ExecutionPolicy Bypass -File tools\analisar.ps1 -Pasta C:\AR\dados
```

O script gera `resumo.csv` (participante × condição: identificações, acertos, erros, taxa e tempo) e `comparativo.csv` (RA e papel lado a lado, com a diferença), e mostra a taxa agregada de cada condição. Ele foi testado com a sessão simulada e uma ficha de papel inventada só para o teste, que não fazem parte do repositório. Passo a passo em `docs/ANALISE.md`.

A hipótese de que a RA reduz a taxa de erros será examinada com base nos resultados, sem presumir que a aplicação terá desempenho superior.

---

## 22. Evidências de funcionamento

Até agora, as evidências são da **simulação no PC**. Ainda não há capturas de um smartphone nem fotos do painel real.

| Evidência | Arquivo |
| --- | --- |
| Painel virtual com os 3 marcadores reconhecidos (placas azuis e nomes) | `docs/images/simulacao-painel.png` |
| Etapa com o marcador solicitado destacado em laranja | `docs/images/simulacao-etapa.png` |
| Foto do painel virtual com os componentes indicados (usada no manual) | `docs/impressao/foto_painel.jpg` |
| Folha de impressão dos marcadores | `docs/impressao/marcadores_para_imprimir.pdf` |
| Manual impresso e ficha de observação | `docs/impressao/*.pdf` |

![Painel virtual na simulação](docs/images/simulacao-painel.png)

Pendentes: tela inicial e resumo final no celular, câmera reconhecendo um marcador impresso, registro de um teste técnico, foto do painel didático desenergizado e vídeo demonstrativo.

---

## 23. Limitações

Previstas:

* Necessidade de cadastrar previamente os marcadores e os componentes.
* Dependência da visibilidade e das condições de captura dos marcadores.
* Possíveis falhas de reconhecimento em condições inadequadas de iluminação, distância ou ângulo.
* Escopo limitado ao painel didático configurado pela equipe.
* Dependência de revisão técnica do conteúdo e dos procedimentos.
* Amostra experimental pequena e composta por participantes disponíveis.
* Avaliação limitada a uma tarefa controlada, sem representar diretamente a manutenção profissional.
* Ausência de evidência de redução de acidentes elétricos, mesmo que haja diferença na taxa de erros.

Observadas até agora:

* **Só funciona em aparelhos compatíveis com ARCore.** O primeiro celular testado pela equipe não era compatível.
* **Testes só na simulação.** O XR Simulation não processa imagem real; o comportamento com marcadores impressos ainda precisa ser verificado.
* **Mira no marcador, não no componente.** Na simulação, foi natural mirar no próprio componente, que não é reconhecido. As instruções aos participantes precisam deixar isso claro.
* **Nota exata dos marcadores desconhecida.** Sabe-se só que ficou ≥ 75.
* **Conteúdo e foto provisórios.** O JSON é a versão `0.1-provisoria` e a foto do manual é do painel virtual.
* **Algumas instruções não citam o componente** (ex.: etapa 3). No app, o destaque laranja compensa; no manual, a coluna "Componente" traz o nome. Vale revisar com o profissional.

---

## 24. Cronograma e estimativa de esforço

O planejamento inicial considera aproximadamente oito semanas e 120 horas de trabalho do grupo. Esses valores são previsões e devem ser ajustados conforme a disponibilidade real.

| Etapa | Atividade | Situação |
| --- | --- | --- |
| Semana 1 | Delimitação do MVP, definição do procedimento e planejamento experimental | Concluído (procedimento provisório) |
| Semana 2 | Cadastro dos componentes, preparação dos marcadores e protótipo da interface | Concluído |
| Semanas 3–4 | Reconhecimento e apresentação das informações | Concluído (validado na simulação) |
| Semana 5 | Procedimento guiado e registro de eventos | Concluído (validado na simulação) |
| Semana 6 | Testes técnicos, correções e validação do conteúdo | **Pendente:** celular compatível, painel real e revisão técnica |
| Semana 7 | Preparação e realização da avaliação experimental | Materiais prontos; coleta pendente |
| Semana 8 | Organização dos dados, análise e documentação final | Script de análise pronto; documentação em andamento |

---

## 25. Controle de versão

O projeto está num repositório Git **local** (branch `main`), ainda sem repositório remoto.

| Commit | Data | Descrição |
| --- | --- | --- |
| `eaf64d7` | 24/10/2026 | `chore: cria projeto Painel AR a partir do WL03` |
| `d3cde53` | 24/10/2026 | `feat: adiciona teste no PC com XR Simulation` |
| `ed15ca2` | 27/10/2026 | `feat: cria os 3 marcadores do painel (Fase 2)` |
| `8847d7c` | 27/10/2026 | `feat: implementa o app do experimento e o painel 3D da simulação (Fase 3)` |
| `b11521e` | 27/10/2026 | `feat: gera manual impresso, ficha de papel e análise dos dados (Fase 4)` |

Ficam fora do Git: `Library/`, `Temp/`, `Logs/`, `Builds/` e arquivos `.apk`.

---

## 26. Problemas encontrados e soluções

### Problema — Build Android falhava no Gradle

**Sintoma:** `java.io.IOException: Unable to establish loopback connection` ao gerar o APK.

**Investigação:** o Gradle funcionava com `--version`, mas falhava ao iniciar o daemon. O Java 17 do Unity cria um socket Unix na pasta temporária, e a conexão falhava no caminho padrão (`C:\Users\ADMINI~1\...`, com nome curto 8.3).

**Solução aplicada:** usar uma pasta temporária curta (`TEMP`/`TMP` = `C:\AR\tmp` e `-Djdk.net.unixdomain.tmpdir=C:\AR\tmp`). O `tools\unity.ps1` faz isso sempre. O build passou em seguida.

### Problema — Marcadores não eram reconhecidos no XR Simulation

**Sintoma:** o marcador aparecia na imagem da câmera simulada, mas nunca era reconhecido.

**Investigação:** a câmera do teste estava em y = 2,52 m, e não em 1,40 m. O XR Origin herdado somava um Camera Y Offset de 1,12 m. O simulador usa a posição da câmera com esse deslocamento para decidir o que ela vê, mas desenha o ambiente sem ele.

**Solução aplicada:** Camera Y Offset = 0 (`Painel AR → Configurar projeto`). No celular não muda nada, porque câmera e marcadores usam o mesmo referencial. A validação do projeto passou a conferir esse valor e o teste reconheceu os marcadores.

### Problema — Componentes do painel virtual saíam pretos

**Sintoma:** disjuntor e contator apareciam pretos em vez de brancos/cinza.

**Investigação:** o gerador do painel guardava os materiais só pelo tipo ("Plastico"), então todas as peças de plástico herdavam a cor da primeira criada (o puxador preto da porta).

**Solução aplicada:** um material por tipo e cor. O painel foi gerado de novo com as cores certas.

### Problema — Foto do manual com legendas escondidas e imagem estourada

**Sintoma:** as linhas das legendas sumiam atrás das laterais do gabinete e a imagem saía clara demais.

**Solução aplicada:** legendas posicionadas na frente do gabinete, luz reduzida na cena de renderização, porta e bancada escondidas na foto.

### Problema — Celular sem suporte a ARCore

**Sintoma:** o Google Play Services for AR não aparecia na Play Store do celular usado nos primeiros testes.

**Solução aplicada:** testes no PC com o XR Simulation e um painel virtual completo; os testes reais aguardam um aparelho compatível.

### Problema — Unity em linha de comando não abria o projeto

**Sintoma:** os comandos do `tools\unity.ps1` terminavam com código 1 logo no início.

**Investigação:** o projeto estava aberto no editor; o Unity não abre o mesmo projeto duas vezes.

**Solução aplicada:** fechar o editor antes de usar a linha de comando (ou usar os mesmos comandos pelo menu **Painel AR**).

---

## 27. Melhorias futuras

Após a conclusão do MVP e do experimento, poderão ser consideradas:

* **Modo demonstração:** mostrar o painel virtual em RA no celular, preso a um marcador, para apresentações e treino (separado do modo do experimento).
* Exportar o painel virtual em glTF para visualização no celular.
* Aprimorar a estabilidade do reconhecimento e testar em mais dispositivos.
* Ampliar o conjunto de componentes cadastrados.
* Melhorar a apresentação visual das informações.
* Facilitar a exportação dos dados do celular.

---

## 28. Custos e recursos

| Recurso | Utilização | Custo |
| --- | --- | --- |
| Computador | Desenvolvimento e compilação | Equipamento já disponível |
| Smartphone Android compatível com ARCore | Execução e testes | A definir |
| Painel didático | Ambiente controlado de avaliação | A definir |
| Marcadores e manual impressos | Referências e condição papel | A registrar (impressão) |
| Unity (licença Personal), AR Foundation, ARCore | Implementação e testes | Gratuitos |
| Git | Controle de versão | Gratuito |

Os custos financeiros efetivos deverão ser registrados pela equipe.

---

## 29. Repositório e materiais complementares

| Recurso | Link |
| --- | --- |
| Código-fonte | Repositório Git local; inserir URL quando for publicado |
| Artigo científico | Inserir link, se disponível |
| Diagrama de arquitetura | A criar em `docs/diagrams/arquitetura.svg` (diagrama em texto na Seção 10) |
| Teste no PC | `docs/TESTE_NO_PC.md` |
| Marcadores | `docs/MARCADORES.md` |
| Coleta e análise | `docs/ANALISE.md` |
| Quadro de tarefas | Inserir link, se disponível |
| Vídeo demonstrativo | Inserir link, quando disponível |

---

## 30. Autores

* **Enrico Affonso**
* **Guilherme Ifanger**
* **Kauan Michael**

**Instituição:** Centro Universitário Facens
**Curso:** Engenharia de Computação
**Disciplina:** UPX — Realidade Aumentada
**Semestre:** 2026.2

---

## 31. Licença

Projeto acadêmico desenvolvido para fins educacionais no âmbito da disciplina UPX — Realidade Aumentada.

Caso a equipe decida adotar uma licença formal de software, deverá incluir o arquivo `LICENSE` e atualizar esta seção conforme os termos escolhidos.

---

## Checklist de entrega

* [x] Escopo do MVP definido.
* [x] Repositório organizado.
* [x] Tecnologias e versões reais documentadas.
* [x] Marcadores cadastrados (imagens do ARCore, não ArUco).
* [ ] Associação entre marcadores e componentes validada no celular (validada só na simulação).
* [x] Informações dos componentes implementadas.
* [ ] Alertas de segurança revisados.
* [x] Procedimento guiado implementado.
* [ ] Registro de eventos validado no celular (validado só na simulação).
* [ ] Aplicação testada no dispositivo escolhido.
* [ ] Critérios dos testes técnicos definidos (proposta na Seção 19.2, a confirmar).
* [ ] Painel didático desenergizado e protocolo de segurança preparados.
* [ ] Conteúdo técnico revisado por profissional da área.
* [ ] Protocolo experimental definido.
* [x] Documentação impressa de comparação preparada (foto do painel provisória).
* [ ] Coleta e análise dos dados realizadas (script de análise pronto).
* [ ] Capturas de tela e evidências reais adicionadas (há só capturas da simulação).
* [ ] Limitações e resultados documentados (limitações observadas na Seção 23; resultados pendentes).
* [x] README atualizado conforme a implementação real.

---

> **Observação:** este README descreve o estado do projeto em 28/10/2026. A aplicação está implementada e validada na simulação, mas ainda não foi testada em um smartphone nem com o painel real, e o conteúdo técnico é provisório. A aplicação destina-se a um cenário didático controlado e não substitui procedimentos oficiais de segurança elétrica nem a atuação de profissionais qualificados.
