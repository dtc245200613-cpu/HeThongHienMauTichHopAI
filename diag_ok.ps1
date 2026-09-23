$ProgressPreference = 'SilentlyContinue'
$base = 'http://localhost:5121'
$sess = New-Object Microsoft.PowerShell.Commands.WebRequestSession

$q = 'Quy trinh dang ky hien mau gom may buoc?'
$r = Invoke-WebRequest -Uri "$base/AI/Ask" -Method Post -Body @{question = $q } -WebSession $sess -UseBasicParsing -TimeoutSec 180

# Luu HTML ra file de soi
$r.Content | Out-File -FilePath ai_answer_dump.html -Encoding utf8
Write-Output ("body length = " + $r.Content.Length)

# Dem so lan xuat hien 'Buoc 1' (khong dau)
$c1 = ([regex]::Matches($r.Content, 'Buoc 1')).Count
Write-Output ("so lan khop 'Buoc 1' (khong dau) = " + $c1)

# Tim xem 'Buoc 1' nam trong doan nao
$idx = $r.Content.IndexOf('Buoc 1')
if ($idx -ge 0) {
  $start = [Math]::Max(0, $idx - 120)
  $frag = $r.Content.Substring($start, [Math]::Min(240, $r.Content.Length - $start))
  Write-Output "--- NGU CANH chua 'Buoc 1' ---"
  Write-Output $frag
}

# Kiem tra co phai nam trong HTML comment / script tag khong
Write-Output "--- Co nam trong script/comment? ---"
Write-Output ("script tag truoc do: " + ($r.Content.LastIndexOf('<script', $idx)))
Write-Output ("comment tag truoc do: " + ($r.Content.LastIndexOf('<!--', $idx)))
