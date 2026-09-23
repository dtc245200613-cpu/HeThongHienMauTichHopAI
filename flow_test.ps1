$ProgressPreference='SilentlyContinue'
$base='http://localhost:5121'
$sess=New-Object Microsoft.PowerShell.Commands.WebRequestSession

$r=Invoke-WebRequest -Uri "$base/Account/Login" -Method Post -Body @{email='admin@gmail.com';password='123456'} -WebSession $sess -UseBasicParsing -ErrorAction Stop
Write-Output ("LOGIN -> "+$r.StatusCode)

$r=Invoke-WebRequest -Uri "$base/DonationRegistration/Register" -WebSession $sess -UseBasicParsing -ErrorAction Stop
$tok2=''
if ($r.Content -match 'name="__RequestVerificationToken" type="hidden" value="([^"]+)"') { $tok2=$matches[1] }

$campId=''
$opts=[regex]::Matches($r.Content, '<option value="(\d+)">([\s\S]*?)</option>')
foreach ($o in $opts) {
  if ($o.Groups[2].Value -match 'Chien dich Test') {
    $campId=$o.Groups[1].Value
    break
  }
}
Write-Output ("campId='"+$campId+"' token2 len="+$tok2.Length)

if ($campId -ne '') {
  $body2=@{ VolunteerName='Nguyen Van Test'; BloodType='O+'; CampaignId=$campId; __RequestVerificationToken=$tok2 }
  try {
    $r=Invoke-WebRequest -Uri "$base/DonationRegistration/Register" -Method Post -Body $body2 -WebSession $sess -UseBasicParsing -MaximumRedirection 5 -ErrorAction Stop
    Write-Output ("REGISTER donation -> "+$r.StatusCode+" final="+$r.BaseResponse.ResponseUri)
  } catch {
    Write-Output ("REGISTER donation -> ERR "+$_.Exception.Message)
  }
}

$r=Invoke-WebRequest -Uri "$base/RegistrationManagement" -WebSession $sess -UseBasicParsing -ErrorAction Stop
Write-Output ("RegMgmt co 'Nguyen Van Test' = "+($r.Content -match 'Nguyen Van Test'))

$r=Invoke-WebRequest -Uri "$base/Statistics" -WebSession $sess -UseBasicParsing -ErrorAction Stop
Write-Output ("Statistics -> "+$r.StatusCode)
