
$ProgressPreference='SilentlyContinue'
$body = '{"question":"Quy trinh dang ky hien mau nhu the nao?"}'
$bytes = [System.Text.Encoding]::UTF8.GetBytes($body)

try {
  $r = Invoke-WebRequest -Uri 'http://127.0.0.1:8000/' -UseBasicParsing -ErrorAction Stop
  Write-Output ("GET / -> "+$r.StatusCode+" "+$r.Content)
} catch {
  Write-Output ("GET / -> ERR "+$_.Exception.Message)
}

try {
  $r = Invoke-WebRequest -Uri 'http://127.0.0.1:8000/ask' -Method Post -Body $bytes -ContentType 'application/json; charset=utf-8' -UseBasicParsing -TimeoutSec 180 -ErrorAction Stop
  Write-Output ("POST /ask -> "+$r.StatusCode)
  Write-Output ("BODY: "+$r.Content)
} catch {
  Write-Output ("POST /ask -> ERR "+$_.Exception.Message)
}
