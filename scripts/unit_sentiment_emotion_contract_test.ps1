$ErrorActionPreference='Stop'
$base='http://127.0.0.1:8001'
$suffix=[DateTimeOffset]::UtcNow.ToUnixTimeSeconds()
$resultFile=Join-Path $PSScriptRoot "unit_sentiment_emotion_contract_result_$suffix.txt"
function Log([string]$line){$line|Tee-Object -FilePath $resultFile -Append}

# Unit-style contract check on AI microservice labels and confidence bounds
$sentBody=@{message='I am confused and frustrated with this concept';language='en'}|ConvertTo-Json
$sent=Invoke-RestMethod -Uri "$base/sentiment" -Method POST -Body $sentBody -ContentType 'application/json'
Log "SENTIMENT_LABEL=$($sent.sentiment)"
Log "SENTIMENT_CONFIDENCE=$($sent.confidence)"
if($sent.sentiment -notin @('positive','neutral','negative')){ throw "Invalid sentiment label" }
if([double]$sent.confidence -lt 0 -or [double]$sent.confidence -gt 1){ throw "Sentiment confidence out of range" }

$emoBody=@{message='I am confused and frustrated with this concept';language='en'}|ConvertTo-Json
$emo=Invoke-RestMethod -Uri "$base/emotion" -Method POST -Body $emoBody -ContentType 'application/json'
Log "EMOTION_LABEL=$($emo.emotion)"
Log "EMOTION_CONFIDENCE=$($emo.confidence)"
if($emo.emotion -notin @('confused','frustrated','engaged','confident','neutral')){ throw "Invalid emotion label" }
if([double]$emo.confidence -lt 0 -or [double]$emo.confidence -gt 1){ throw "Emotion confidence out of range" }

Log "RESULT=PASS"
Log "RESULT_FILE=$resultFile"
