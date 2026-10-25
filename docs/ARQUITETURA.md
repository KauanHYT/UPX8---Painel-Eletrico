# Arquitetura — Unity

```text
AR Session
  └─ XR Origin
      ├─ Main Camera + AR Camera Manager/Background
      ├─ ARTrackedImageManager
      └─ WL03ProcedureController

Resources/wl03_procedure.json → etapas
XRReferenceImageLibrary → alvo físico
WL03StatusOverlay → UI/progresso/controles
```

A experiência usa o target como referencial espacial. Os componentes A/B/C são filhos do `ARTrackedImage`, portanto permanecem sincronizados com o equipamento enquanto o tracking estiver ativo.
