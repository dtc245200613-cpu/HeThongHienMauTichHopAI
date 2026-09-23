# File: ai_web_test.ps1
# Kiem tra tich hop Web app -> RAG API.
# Luu y: Razor ma hoa ky tu tieng Viet thanh HTML entity (vd 'B&#x1B0;&#x1EDB;c').
# Nen truoc khi so khop phai giai ma entity -> van ban that.

$ProgressPreference='SilentlyContinue'
$base='http://localhost:5121'
$sess=New-Object Microsoft.PowerShell.Commands.WebRequestSession

# Giai ma HTML entity ve van ban that de regex tieng Viet co dau hoat dong.
function Get-PlainText {
    param([string]$html)

    $decoded = [System.Net.WebUtility]::HtmlDecode($html)

    $sb = New-Object System.Text.StringBuilder
    $inTag = $false

    foreach ($ch in $decoded.ToCharArray()) {
        if ($ch -eq [char]60) {
            $inTag = $true
            [void]$sb.Append(' ')
        }
        elseif ($ch -eq [char]62) {
            $inTag = $false
        }
        elseif (-not $inTag) {
            [void]$sb.Append($ch)
        }
    }

    return $sb.ToString()
}

# Kiem tra RAG API con song
try {
    $r = Invoke-WebRequest -Uri 'http://127.0.0.1:8000/' -UseBasicParsing -ErrorAction Stop
    Write-Output ("RAG API -> " + $r.StatusCode)
} catch {
    Write-Output ("RAG API -> DOWN: " + $_.Exception.Message)
}

# GET trang AI
try {
    $r = Invoke-WebRequest -Uri "$base/AI" -WebSession $sess -UseBasicParsing -ErrorAction Stop
    Write-Output ("GET /AI -> " + $r.StatusCode)
} catch {
    Write-Output ("GET /AI -> ERR " + $_.Exception.Message)
}

# POST cau hoi
$q = 'Quy trinh dang ky hien mau gom may buoc?'
try {
    $r = Invoke-WebRequest -Uri "$base/AI/Ask" -Method Post -Body @{question=$q} -WebSession $sess -UseBasicParsing -TimeoutSec 180 -ErrorAction Stop
    Write-Output ("POST /AI/Ask -> " + $r.StatusCode)

    # Van ban da giai ma entity -> so khop bang tieng Viet co dau binh thuong
    $text = Get-PlainText -html $r.Content

    # "Bước 1" va "Không thể kết nối" viet bang ma Unicode de tranh loi encoding file
    $patOk    = 'B\u01B0\u1EDBc 1'
    $patErr   = 'Kh\u00F4ng th\u1EC3 k\u1EBFt n\u1ED1i'

    $ok    = [regex]::IsMatch($text, $patOk)
    $isErr = [regex]::IsMatch($text, $patErr)

    Write-Output ("  body length = " + $r.Content.Length)
    Write-Output ("  co cau tra loi  = " + $ok)
    Write-Output ("  bao loi ket noi = " + $isErr)

    if (-not $ok) {
        Write-Output "  --- KHONG tim thay 'Buoc 1', trich doan tra loi: ---"
        $i = $text.IndexOf('li')
        if ($i -ge 0) {
            Write-Output $text.Substring($i, [Math]::Min(300, $text.Length - $i))
        }
    }
} catch {
    Write-Output ("POST /AI/Ask -> ERR " + $_.Exception.Message)
}
