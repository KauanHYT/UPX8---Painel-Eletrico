# WL03 — Contextual / Procedural AR · Unity + AR Foundation

Baseline educacional para procedimentos técnicos guiados em RA. O projeto reconhece um alvo físico que representa um equipamento demonstrativo, sobrepõe três componentes A/B/C e conduz uma sequência de verificação com destaque contextual, progresso, confirmação e medição de tempo.

## Ambiente de referência
- Windows 10/11 64 bits
- Unity Hub
- Unity Editor 6.3 LTS (6000.3.x)
- Android Build Support + SDK/NDK + OpenJDK
- AR Foundation 6.3.5
- ARCore XR Plugin 6.3.5
- Smartphone Android físico compatível com ARCore

## Caminho sugerido
`C:\AR\WL03\wl03-contextual-ar-unity`

## Uso
1. Extraia o projeto e abra a pasta pelo Unity Hub com Unity 6.3 LTS.
2. Aguarde a importação. O script de Editor configura o baseline automaticamente.
3. Use `WL03 > Validar projeto`.
4. Imprima `Assets/Markers/wl03-equipment-target.png` com aproximadamente 20 cm de largura.
5. Conecte o Android com depuração USB.
6. Use `WL03 > Build and Run Android`.
7. Aponte a câmera para o alvo e confirme as etapas A → B → C.

## Menu WL03
- Configurar projeto completo
- Validar projeto
- Abrir cena principal
- Build Android APK
- Build and Run Android
- Recriar cena baseline

## Recursos
- `ARTrackedImageManager`: reconhecimento e tracking do equipamento.
- `XRReferenceImageLibrary`: biblioteca criada automaticamente com o alvo WL03.
- `WL03ProcedureController`: estado e lógica do procedimento.
- `WL03StatusOverlay`: interface, progresso e controles.
- `wl03_procedure.json`: conteúdo das etapas separado do código.

Consulte `docs/ARQUITETURA.md` e `docs/CUSTOMIZACAO.md`.
