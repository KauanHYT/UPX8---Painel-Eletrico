# Marcadores

| Nome na biblioteca | Componente | Imagem | Tamanho impresso |
| --- | --- | --- | --- |
| `marcador_DJ1` | Disjuntor geral DJ1 | `Assets/Markers/marcador_DJ1.png` | 8 × 8 cm |
| `marcador_K1` | Contator K1 | `Assets/Markers/marcador_K1.png` | 8 × 8 cm |
| `marcador_FT1` | Relé de sobrecarga FT1 | `Assets/Markers/marcador_FT1.png` | 8 × 8 cm |

O nome na biblioteca deve ser idêntico ao campo `marcador` do `conteudo_painel.json` (Fase 3).

## Como foram feitos

- **Imagens:** geradas por `Assets/Editor/GeradorMarcadores.cs` (menu `Painel AR → Marcadores → Gerar imagens`). São procedurais e com semente fixa, então gerar de novo produz as mesmas imagens. Cada uma tem paleta própria, formas em três escalas e linhas em vários ângulos, sem simetria nem repetição, como pede o guia (Parte 5.1). A barra escura embaixo e à esquerda indica a orientação.
- **Biblioteca:** `Assets/ReferenceImages/PainelReferenceImageLibrary.asset`, criada pelo menu `Painel AR → Marcadores → Criar biblioteca e ligar à cena`, com Physical Size de 0,08 m, Keep Texture at Runtime desmarcado e Requested Max Number Of Moving Images = 3 no AR Tracked Image Manager.
- **Qualidade (RNF02):** no build Android, o plugin ARCore do Unity avalia cada imagem com o `arcoreimg eval-img` e registra um aviso "Low scoring image report" para qualquer nota abaixo de 75. O build de 27/10/2026 não gerou o aviso, ou seja, as três ficaram com nota ≥ 75. O valor exato de cada nota não aparece no log.

## Impressão

Use `docs/impressao/marcadores_para_imprimir.pdf` (uma folha A4, gerada de `docs/impressao/marcadores.html`):

1. Imprima em **tamanho real (100%)**, sem "ajustar à página", em papel fosco.
2. Meça com régua: cada marcador deve ter 8,0 cm e a régua da folha, 5,0 cm. Se der diferente, corrija o Physical Size na biblioteca e gere o APK de novo.
3. Recorte pela linha tracejada e cole cada marcador plano, ao lado do componente, sem cobrir bornes nem etiquetas. O nome do componente fica fora do recorte.
