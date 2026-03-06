$ErrorActionPreference='Stop'
$base='http://127.0.0.1:5085'
$suffix=[DateTimeOffset]::UtcNow.ToUnixTimeSeconds()
$email="ai.s1.$suffix@example.com"
$pwd='Pass@12345'
$resultFile=Join-Path $PSScriptRoot "s1_sentiment_result_$suffix.txt"
function Log([string]$line){$line|Tee-Object -FilePath $resultFile -Append}

$create = curl.exe -s -w "`nHTTPSTATUS:%{http_code}" -X POST `
  -F "userCreateDTO.UserType=2" `
  -F "userCreateDTO.FirstName=AI" `
  -F "userCreateDTO.LastName=S1" `
  -F "userCreateDTO.Email=$email" `
  -F "userCreateDTO.Password=$pwd" `
  "$base/api/User"
$createStatus=($create|Select-String -Pattern 'HTTPSTATUS:(\d+)' -AllMatches).Matches[0].Groups[1].Value
Log "CREATE_STATUS=$createStatus"

$loginBody=@{email=$email;password=$pwd}|ConvertTo-Json
$login=Invoke-RestMethod -Uri "$base/api/User/login" -Method POST -Body $loginBody -ContentType 'application/json'
$token=$login.token
Log "LOGIN_STATUS=200"
Log "LOGIN_USER_ID=$($login.id)"

$headers=@{Authorization="Bearer $token"}
$sentBody=@{message='I am confused and frustrated with recursion.';language='en'}|ConvertTo-Json
$sentResp=Invoke-RestMethod -Uri "$base/api/ai/sentiment" -Method POST -Headers $headers -Body $sentBody -ContentType 'application/json'
Log "SENTIMENT_STATUS=200"
Log "SENTIMENT_LABEL=$($sentResp.sentiment)"
Log "SENTIMENT_CONFIDENCE=$($sentResp.confidence)"
Log "SENTIMENT_PROVIDER=$($sentResp.provider)"
Log "RESULT_FILE=$resultFile"
