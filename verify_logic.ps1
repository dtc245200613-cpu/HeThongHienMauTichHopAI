$ProgressPreference = 'SilentlyContinue'
$base = 'http://localhost:5121'
$sess = New-Object Microsoft.PowerShell.Commands.WebRequestSession

function Get-PlainText {
    param([string]$html)
    $decoded = [System.Net.WebUtility]::HtmlDecode($html)
    $sb = New-Object System.Text.StringBuilder
    $inTag = $false
    foreach ($ch in $decoded.ToCharArray()) {
        if ($ch -eq [char]60) { $inTag = $true; [void]$sb.Append(' ') }
        elseif ($ch -eq [char]62) { $inTag = $false }
        elseif (-not $inTag) { [void]$sb.Append($ch) }
    }
    return $sb.ToString()
}

$q = 'Quy trinh dang ky hien mau gom may buoc?'
$r = Invoke-WebRequest -Uri "$base/AI/Ask" -Method Post -Body @{question = $q } -WebSession $sess -UseBasicParsing -TimeoutSec 180
$text = Get-PlainText -html $r.Content

Write-Output "=== 1. Van ban da giai ma (tim 'B') ==="
$i = $text.IndexOf('Theo')
if ($i -lt 0) { $i = $text.IndexOf('B') }
Write-Output $text.Substring($i, [Math]::Min(400, $text.Length - $i))

Write-Output ""
Write-Output "=== 2. Kiem chung regex THAT SU phan biet dung/sai ==="
$patOk = 'B\u01B0\u1EDBc 1'          # "Bước 1"  -> PHAI khop
$patBad = 'XYZ_KHONG_TON_TAI_123'     # rac        -> PHAI khong khop
$patErr = 'Kh\u00F4ng th\u1EC3 k\u1EBFt n\u1ED1i'  # "Không thể kết nối" -> PHAI khong khop

Write-Output ("  'Bước 1' (phai True)          = " + [regex]::IsMatch($text, $patOk))
Write-Output ("  'XYZ_KHONG_TON_TAI_123' (False) = " + [regex]::IsMatch($text, $patBad))
Write-Output ("  'Không thể kết nối' (phai False) = " + [regex]::IsMatch($text, $patErr))
