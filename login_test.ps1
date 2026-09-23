$ProgressPreference='SilentlyContinue'
$base='http://localhost:5121'
$sess=$null

# 1) Dang nhap voi tai khoan admin
$sess=New-Object Microsoft.PowerShell.Commands.WebRequestSession
try {
  $r=Invoke-WebRequest -Uri "$base/Account/Login" -Method Post -Body @{email='admin@gmail.com';password='123456'} -WebSession $sess -UseBasicParsing -MaximumRedirection 5 -ErrorAction Stop
  Write-Output ("LOGIN admin -> "+$r.StatusCode+" final="+$r.BaseResponse.ResponseUri)
  Write-Output ("COOKIE count = "+$sess.Cookies.Count)
} catch {
  Write-Output ("LOGIN admin -> ERR "+$_.Exception.Message)
}

# 2) Truy cap trang bao ve sau khi dang nhap
foreach($u in @('/Statistics','/Campaign','/RegistrationManagement')){
  try {
    $r=Invoke-WebRequest -Uri ($base+$u) -WebSession $sess -UseBasicParsing -ErrorAction Stop
    Write-Output ($u+" (auth) -> "+$r.StatusCode)
  } catch {
    Write-Output ($u+" (auth) -> ERR "+$_.Exception.Message)
  }
}

# 3) Dang nhap sai
$s2=New-Object Microsoft.PowerShell.Commands.WebRequestSession
try {
  $r=Invoke-WebRequest -Uri "$base/Account/Login" -Method Post -Body @{email='admin@gmail.com';password='wrong'} -WebSession $s2 -UseBasicParsing -ErrorAction Stop
  $hasErr = $r.Content.Length
  Write-Output ("LOGIN sai -> "+$r.StatusCode+" bodylen="+$hasErr)
} catch {
  Write-Output ("LOGIN sai -> ERR "+$_.Exception.Message)
}
