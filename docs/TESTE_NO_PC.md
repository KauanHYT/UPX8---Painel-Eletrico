# Testar no PC com o XR Simulation

O Play do editor roda o app com o **XR Simulation** do AR Foundation: em vez da câmera do celular, o Unity mostra um ambiente virtual (`Assets/Simulacao/AmbientePainel.prefab`) com um quadro de comando completo e os marcadores da Reference Image Library colados ao lado de cada componente. Não precisa baixar nada nem conectar celular.

Serve para testar a lógica do app e ensaiar o experimento. Não substitui os testes técnicos no celular (iluminação, distância, ângulo, FPS), porque o reconhecimento simulado não processa imagem de verdade.

## O painel virtual

Uma caixa metálica de 60 × 80 cm presa na parede, com a porta aberta e placa de montagem laranja:

| Componente | Onde fica | Detalhes modelados |
| --- | --- | --- |
| Disjuntor geral **DJ1** | trilho de cima, à esquerda | 3 polos, alavanca em DESLIGADO (O OFF), etiqueta "ALIMENTAÇÃO GERAL" |
| Contator **K1** | trilho do meio, à direita | bornes 1/L1, 3/L2, 5/L3 em cima, 2/T1, 4/T2, 6/T3 embaixo, bobina A1/A2 |
| Relé de sobrecarga **FT1** | encaixado embaixo do K1 | dial azul de ajuste (17–25 A), botão RESET, contatos 95-96 / 97-98 |
| Régua de bornes **X1** | trilho de baixo | 10 bornes cinza e 2 azuis |

Também há canaletas, cabos, etiquetas de identificação e, colado ao lado de cada componente, o seu marcador de 8 cm. O painel é gerado por `Assets/Editor/PainelARSimulacao.cs`; a posição dos marcadores fica em `PosicaoMarcadores`.

## Passo a passo

1. Abra o **Unity Hub** → **Projects** → **Add** → **Add project from disk** e escolha `C:\AR\PainelAR\painel-ar` (se já adicionou, só clique nele).
2. Se o Hub perguntar a versão do editor, escolha **6000.6.7f1**.
3. A cena `Assets/Scenes/WL03ContextualAR` deve abrir sozinha. Se não abrir, dê dois cliques nela na janela **Project**.
4. Na aba **Game**, escolha no menu de resolução (canto superior esquerdo) uma proporção em pé, como **1080x1920**, para a tela ficar como a de um celular.
5. Clique em **Play** (▶).

| Ação | Controle |
| --- | --- |
| Olhar em volta | segure o **botão direito** do mouse e mova o mouse |
| Andar | segure o botão direito e use **W A S D** |
| Subir / descer | **E** / **Q** (com o botão direito pressionado) |
| Andar mais rápido | **Shift** |
| Tocar nos botões do app | **clique com o botão esquerdo** |

## Roteiro de teste (igual ao teste de mesa da Parte 9 do guia)

1. Na tela inicial, deixe **P01** e clique em **INICIAR SESSÃO**.
2. Na etapa 1 aparece a tela vermelha de alerta. Clique em **LI E ENTENDI**.
3. Cada marcador ganha uma placa azul e o nome do componente; o pedido na etapa fica **laranja**.
4. Ande até deixar a **cruz** (um pouco acima do meio da tela) sobre o marcador laranja. A cruz fica verde e aparece "Na mira: …" com a função e o alerta do componente.
5. Clique em **CONFIRMAR COMPONENTE**.
6. Em algumas etapas, confirme de propósito o marcador errado.
7. Ao fim das 15 etapas, confira acertos, erros, taxa de erros e tempo na tela final.

Clique em **Play** de novo para parar. O CSV de cada sessão fica em `%USERPROFILE%\AppData\LocalLow\Facens\Painel AR\` (no celular, em `Android/data/br.edu.facens.painelar/files`).

## Teste automático (sem abrir o editor)

Com o Unity **fechado**:

```
powershell -ExecutionPolicy Bypass -File tools\unity.ps1 TesteSimulacao.Executar -SemSair
```

Ele entra em Play e faz uma sessão completa como um participante: espera os 3 marcadores serem reconhecidos, aponta a câmera para o componente pedido em cada etapa, confirma os alertas e erra de propósito nas etapas 4 e 12. No fim confere que houve 13 acertos, 2 erros e 15 identificações no CSV, e termina com `TESTE SIMULACAO OK` ou `FALHOU`. Deixa em `Logs/` duas imagens da câmera e uma cópia do CSV (`teste_simulacao.csv`).

![Painel virtual visto da posição inicial](images/simulacao-painel.png)

![Etapa 1: marcador do DJ1 destacado em laranja](images/simulacao-etapa.png)

## Se algo der errado

| Sintoma | O que fazer |
| --- | --- |
| Tela preta ou cinza vazia no Play | Confira em **Edit → Project Settings → XR Plug-in Management**, aba do PC (monitor), se **XR Simulation** está marcado. Se não estiver, rode **Painel AR → Simulação → Configurar simulação no PC** |
| A câmera não se mexe | Clique dentro da janela **Game** e mantenha o **botão direito** pressionado enquanto usa as teclas |
| Aviso pedindo para reiniciar por causa do Input System | Aceite; o projeto usa "Both" (Input Manager + Input System) |
| Marcadores aparecem, mas nunca ganham a placa azul | Rode **Painel AR → Configurar projeto** (zera o Camera Y Offset do XR Origin) e depois a configuração da simulação |
| "Na mira: nenhum componente" com o marcador na tela | Leve o marcador até a **cruz**, que fica acima do meio da tela, não no centro |
| Tela "Erro de configuração" | Rode **Painel AR → Validar projeto** e leia o erro no Console |
