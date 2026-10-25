# Roda um método do PainelARSetup no Unity em modo batch (sem abrir o editor).
# Uso:  powershell -File tools\unity.ps1 ConfigurarProjeto | ValidarProjeto | GerarApk | GerarApkEInstalar
param(
    [Parameter(Mandatory = $true)][string]$Metodo,
    [string]$Unity = "C:\Program Files\Unity\Hub\Editor\6000.6.7f1\Editor\Unity.exe"
)

$projeto = Split-Path -Parent $PSScriptRoot
$logs = Join-Path $projeto "Logs"
New-Item -ItemType Directory -Force $logs | Out-Null
$log = Join-Path $logs "batch_$Metodo.log"

# O Java 17 do Unity cria sockets Unix no TEMP. Com o TEMP padrão (C:\Users\ADMINI~1\...)
# o Gradle falha com "Unable to establish loopback connection". Um TEMP curto resolve.
$tmp = "C:\AR\tmp"
New-Item -ItemType Directory -Force $tmp | Out-Null
$env:TEMP = $tmp
$env:TMP = $tmp
$env:JAVA_TOOL_OPTIONS = "-Djdk.net.unixdomain.tmpdir=$tmp"

$argumentos = @('-batchmode', '-quit', '-projectPath', $projeto, '-buildTarget', 'Android',
    '-executeMethod', "PainelAR.EditorTools.PainelARSetup.$Metodo", '-logFile', $log)
$p = Start-Process -FilePath $Unity -ArgumentList $argumentos -PassThru -Wait

Select-String -Path $log -Pattern '\[PainelAR\]|error CS|Build Finished' | ForEach-Object { $_.Line }
"Log completo: $log"
exit $p.ExitCode
