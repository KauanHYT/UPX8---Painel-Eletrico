# Calcula a taxa de erros de identificação por participante e por condição (Seção 3.10 do artigo).
# Lê todos os .csv de uma pasta: os do app (condicao = RA) e os preenchidos a partir do
# docs/analise/modelo_papel.csv (condicao = PAPEL). Mesmas colunas nos dois (Parte 11 do guia).
# Uso:  powershell -ExecutionPolicy Bypass -File tools\analisar.ps1 -Pasta C:\AR\dados
# Gera <Pasta>\resultados\resumo.csv e comparativo.csv (separador ";", abrem direto no Excel).
param(
    [Parameter(Mandatory = $true)][string]$Pasta
)

$ErrorActionPreference = 'Stop'
$ptBR = [Globalization.CultureInfo]::GetCultureInfo('pt-BR')

function Numero([string]$texto) {
    if ([string]::IsNullOrWhiteSpace($texto)) { return $null }
    return [double]::Parse($texto.Trim().Replace('.', ','), $ptBR)
}

$arquivos = Get-ChildItem -Path $Pasta -Filter *.csv -File
if (-not $arquivos) { throw "Nenhum .csv em $Pasta" }

$sessoes = @()
foreach ($arquivo in $arquivos) {
    $linhas = Import-Csv -Path $arquivo.FullName -Delimiter ';' -Encoding UTF8
    $idents = @($linhas | Where-Object { $_.evento -eq 'IDENTIFICACAO' })
    if ($idents.Count -eq 0) { Write-Warning "$($arquivo.Name): sem linhas IDENTIFICACAO, ignorado"; continue }

    $participante = $idents[0].participante
    $condicao = $idents[0].condicao
    if ($participante -eq 'Pxx') { Write-Warning "$($arquivo.Name): código do participante não preenchido (Pxx)" }

    $erros = 0; $semResposta = 0
    foreach ($l in $idents) {
        # Na ficha de papel o "correto" pode ficar em branco: compara indicado x solicitado
        $correto = $l.correto
        if ([string]::IsNullOrWhiteSpace($correto)) {
            if ([string]::IsNullOrWhiteSpace($l.componente_indicado)) { $semResposta++; continue }
            $correto = if ($l.componente_indicado.Trim() -eq $l.componente_solicitado.Trim()) { '1' } else { '0' }
        }
        if ($correto.Trim() -eq '0') { $erros++ }
    }
    if ($semResposta -gt 0) { Write-Warning "$($arquivo.Name): $semResposta etapa(s) sem componente indicado; contadas fora do total" }

    $total = $idents.Count - $semResposta
    $fim = $linhas | Where-Object { $_.evento -eq 'SESSAO_FIM' } | Select-Object -Last 1
    $tempo = if ($fim) { Numero $fim.tempo_s } else { $null }

    $sessoes += [pscustomobject]@{
        participante  = $participante
        condicao      = $condicao
        identificacoes = $total
        acertos       = $total - $erros
        erros         = $erros
        taxa_erros    = if ($total -gt 0) { [math]::Round(100.0 * $erros / $total, 1) } else { $null }
        tempo_s       = $tempo
        arquivo       = $arquivo.Name
    }
}

$saida = Join-Path $Pasta 'resultados'
New-Item -ItemType Directory -Force $saida | Out-Null

$sessoes = $sessoes | Sort-Object participante, condicao
$sessoes | Format-Table participante, condicao, identificacoes, acertos, erros, taxa_erros, tempo_s -AutoSize | Out-String | Write-Host
$sessoes | ForEach-Object {
    [pscustomobject]@{
        participante = $_.participante; condicao = $_.condicao; identificacoes = $_.identificacoes
        acertos = $_.acertos; erros = $_.erros
        taxa_erros = if ($_.taxa_erros -ne $null) { $_.taxa_erros.ToString('0.0', $ptBR) } else { '' }
        tempo_s = if ($_.tempo_s -ne $null) { $_.tempo_s.ToString('0.0', $ptBR) } else { '' }
        arquivo = $_.arquivo
    }
} | Export-Csv -Path (Join-Path $saida 'resumo.csv') -Delimiter ';' -NoTypeInformation -Encoding UTF8

# Um participante por linha, RA x PAPEL lado a lado (comparação individual, sem teste de significância)
$comparativo = $sessoes | Group-Object participante | ForEach-Object {
    $ra = $_.Group | Where-Object condicao -eq 'RA' | Select-Object -First 1
    $papel = $_.Group | Where-Object condicao -eq 'PAPEL' | Select-Object -First 1
    $dif = if ($ra -and $papel) { $ra.taxa_erros - $papel.taxa_erros } else { $null }
    [pscustomobject]@{
        participante        = $_.Name
        taxa_erros_RA       = if ($ra) { $ra.taxa_erros.ToString('0.0', $ptBR) } else { '' }
        taxa_erros_PAPEL    = if ($papel) { $papel.taxa_erros.ToString('0.0', $ptBR) } else { '' }
        diferenca_RA_menos_PAPEL = if ($dif -ne $null) { $dif.ToString('0.0', $ptBR) } else { '' }
        menos_erros_com     = if ($dif -eq $null) { '' } elseif ($dif -lt 0) { 'RA' } elseif ($dif -gt 0) { 'PAPEL' } else { 'empate' }
        tempo_RA_s          = if ($ra -and $ra.tempo_s -ne $null) { $ra.tempo_s.ToString('0.0', $ptBR) } else { '' }
        tempo_PAPEL_s       = if ($papel -and $papel.tempo_s -ne $null) { $papel.tempo_s.ToString('0.0', $ptBR) } else { '' }
    }
}
$comparativo | Format-Table -AutoSize | Out-String | Write-Host
$comparativo | Export-Csv -Path (Join-Path $saida 'comparativo.csv') -Delimiter ';' -NoTypeInformation -Encoding UTF8

# Taxa agregada por condição: soma dos erros / soma das identificações
foreach ($grupo in ($sessoes | Group-Object condicao)) {
    $e = ($grupo.Group | Measure-Object erros -Sum).Sum
    $t = ($grupo.Group | Measure-Object identificacoes -Sum).Sum
    Write-Host ("{0}: {1} erro(s) em {2} identificações = {3}% ({4} participante(s))" -f $grupo.Name, $e, $t, (100.0 * $e / $t).ToString('0.0', $ptBR), $grupo.Count)
}
Write-Host "Arquivos gerados em $saida"
