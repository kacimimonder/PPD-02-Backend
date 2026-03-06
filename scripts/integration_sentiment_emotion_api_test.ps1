$ErrorActionPreference='Stop'
$base='http://127.0.0.1:5085'
$suffix=[DateTimeOffset]::UtcNow.ToUnixTimeSeconds()
$email="ai.integration.$suffix@example.com"
$pwd='Pass@12345'
$resultFile=Join-Path $PSScriptRoot "integration_sentiment_emotion_result_$suffix.txt"
function Log([string]$line){$line|Tee-Object -FilePath $resultFile -Append}

$create = curl.exe -s -w "`nHTTPSTATUS:%{http_code}" -X POST `
  -F "userCreateDTO.UserType=2" `
  -F "userCreateDTO.FirstName=AI" `
  -F "userCreateDTO.LastName=Integration" `
  -F "userCreateDTO.Email=$email" `
  -F "userCreateDTO.Password=$pwd" `
  "$base/api/User"
$createStatus=($create|Select-String -Pattern 'HTTPSTATUS:(\d+)' -AllMatches).Matches[0].Groups[1].Value
Log "CREATE_STATUS=$createStatus"
if($createStatus -ne '200' -and $createStatus -ne '201'){ throw "Create user failed" }

$loginBody=@{email=$email;password=$pwd}|ConvertTo-Json
$login=Invoke-RestMethod -Uri "$base/api/User/login" -Method POST -Body $loginBody -ContentType 'application/json'
$token=$login.token
$headers=@{Authorization="Bearer $token"}
Log "LOGIN_STATUS=200"

$sentBody=@{message='This was difficult and I feel confused';language='en'}|ConvertTo-Json
$sent=Invoke-RestMethod -Uri "$base/api/ai/sentiment" -Method POST -Headers $headers -Body $sentBody -ContentType 'application/json'
Log "SENTIMENT_STATUS=200"
Log "SENTIMENT_LABEL=$($sent.sentiment)"

$emoBody=@{message='This was difficult and I feel confused';language='en'}|ConvertTo-Json
$emo=Invoke-RestMethod -Uri "$base/api/ai/emotion" -Method POST -Headers $headers -Body $emoBody -ContentType 'application/json'
Log "EMOTION_STATUS=200"
Log "EMOTION_LABEL=$($emo.emotion)"

$mon=Invoke-RestMethod -Uri "$base/api/ai/monitoring" -Method GET -Headers $headers
Log "MONITORING_STATUS=200"
if($mon.sentiment){ Log "MON_SENTIMENT_TOTAL=$($mon.sentiment.totalCalls)" }
if($mon.emotion){ Log "MON_EMOTION_TOTAL=$($mon.emotion.totalCalls)" }

Log "RESULT=PASS"
Log "RESULT_FILE=$resultFile"
