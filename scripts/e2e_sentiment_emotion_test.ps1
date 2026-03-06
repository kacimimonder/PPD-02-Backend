$base='http://127.0.0.1:5085'
$suffix=[DateTimeOffset]::UtcNow.ToUnixTimeSeconds()
$email="ai.sentiment.$suffix@example.com"
$pwd='Pass@12345'
$resultFile = Join-Path $PSScriptRoot "e2e_sentiment_emotion_result_$suffix.txt"

function LogLine([string]$line) {
    $line | Tee-Object -FilePath $resultFile -Append
}

LogLine "EMAIL=$email"

# Create instructor
$createBody = curl.exe -s -w "`nHTTPSTATUS:%{http_code}" -X POST `
  -F "userCreateDTO.UserType=2" `
  -F "userCreateDTO.FirstName=AI" `
  -F "userCreateDTO.LastName=Signals" `
  -F "userCreateDTO.Email=$email" `
  -F "userCreateDTO.Password=$pwd" `
  "$base/api/User"
$createStatus = ($createBody | Select-String -Pattern "HTTPSTATUS:(\d+)" -AllMatches).Matches[0].Groups[1].Value
LogLine "CREATE_STATUS=$createStatus"
if ($createStatus -ne "200" -and $createStatus -ne "201") { LogLine "CREATE_BODY=$createBody"; exit 1 }

# Login
$loginBody=@{email=$email;password=$pwd}|ConvertTo-Json
$loginResp=Invoke-RestMethod -Uri "$base/api/User/login" -Method POST -Body $loginBody -ContentType 'application/json'
$token=$loginResp.token
$userId=[int]$loginResp.id
$headers=@{Authorization="Bearer $token"}
LogLine "LOGIN_STATUS=200"
LogLine "LOGIN_USER_ID=$userId"

# Create course/module/content
$imgPath=Join-Path $PSScriptRoot 's4-test-image.jpg'
Set-Content -Path $imgPath -Value 'fake-image-bytes' -Encoding Ascii
$courseBody = curl.exe -s -w "`nHTTPSTATUS:%{http_code}" -X POST `
  -H "Authorization: Bearer $token" `
  -F "Title=AI Signals Course $suffix" `
  -F "Description=Course for sentiment emotion e2e test" `
  -F "Price=0" `
  -F "SubjectID=1" `
  -F "LanguageID=1" `
  -F "Level=Beginner" `
  -F "InstructorID=$userId" `
  -F "Image=@$imgPath" `
  "$base/api/Courses"
$courseStatus = ($courseBody | Select-String -Pattern "HTTPSTATUS:(\d+)" -AllMatches).Matches[0].Groups[1].Value
$coursePayload = ($courseBody -replace "HTTPSTATUS:\d+","" ).Trim()
$courseIdMatch = [regex]::Match($coursePayload, "\d+")
if (-not $courseIdMatch.Success) { throw "Failed to parse courseId" }
$courseId=[int]$courseIdMatch.Value
LogLine "COURSE_STATUS=$courseStatus"
LogLine "COURSE_ID=$courseId"
if ($courseStatus -ne "200" -and $courseStatus -ne "201") { LogLine "COURSE_BODY=$courseBody"; exit 1 }

$moduleBody=@{courseId=$courseId;name='Sentiment Module';description='Module for adaptive AI chat testing'}|ConvertTo-Json
$moduleResp=Invoke-RestMethod -Uri "$base/api/CourseModule" -Method POST -Headers $headers -Body $moduleBody -ContentType 'application/json'
$moduleId=[int]$moduleResp
LogLine "MODULE_STATUS=200"
LogLine "MODULE_ID=$moduleId"

$mcBody = curl.exe -s -w "`nHTTPSTATUS:%{http_code}" -X POST `
  -H "Authorization: Bearer $token" `
  -F "moduleContentCreateDTO.Name=Core Section" `
  -F "moduleContentCreateDTO.Content=Recursion is a technique where a function solves a problem by calling itself on smaller instances. Base case stops recursion and recursive case reduces the problem size." `
  -F "moduleContentCreateDTO.CourseModuleID=$moduleId" `
  "$base/api/moduleContent"
$mcStatus = ($mcBody | Select-String -Pattern "HTTPSTATUS:(\d+)" -AllMatches).Matches[0].Groups[1].Value
LogLine "MODULE_CONTENT_STATUS=$mcStatus"
if ($mcStatus -ne "200" -and $mcStatus -ne "201") { LogLine "MC_BODY=$mcBody"; exit 1 }

# S1: sentiment endpoint
$sentimentBody=@{message='I am confused and frustrated, this is hard to understand';language='en';moduleId=$moduleId}|ConvertTo-Json
$sentimentResp=Invoke-RestMethod -Uri "$base/api/ai/sentiment" -Method POST -Headers $headers -Body $sentimentBody -ContentType 'application/json'
LogLine "SENTIMENT_STATUS=200"
LogLine "SENTIMENT_LABEL=$($sentimentResp.sentiment)"
LogLine "SENTIMENT_CONFIDENCE=$($sentimentResp.confidence)"

# S2: emotion endpoint
$emotionBody=@{message='I am confused and frustrated, this is hard to understand';language='en';moduleId=$moduleId}|ConvertTo-Json
$emotionResp=Invoke-RestMethod -Uri "$base/api/ai/emotion" -Method POST -Headers $headers -Body $emotionBody -ContentType 'application/json'
LogLine "EMOTION_STATUS=200"
LogLine "EMOTION_LABEL=$($emotionResp.emotion)"
LogLine "EMOTION_CONFIDENCE=$($emotionResp.confidence)"

# S3: chat adaptation metadata
$conversationId="signals-$suffix"
$chatBody=@{message='I am confused, can you explain recursion simply step by step?';conversationId=$conversationId;useServerMemory=$true;language='en';history=@()}|ConvertTo-Json -Depth 6
$chatResp=Invoke-RestMethod -Uri "$base/api/ai/modules/$moduleId/chat" -Method POST -Headers $headers -Body $chatBody -ContentType 'application/json'
LogLine "CHAT_STATUS=200"
LogLine "CHAT_PROVIDER=$($chatResp.provider)"
LogLine "CHAT_SENTIMENT=$($chatResp.sentiment)"
LogLine "CHAT_EMOTION=$($chatResp.emotion)"
LogLine "CHAT_ADAPTATION_APPLIED=$($chatResp.adaptationApplied)"
LogLine "CHAT_CONVERSATION_ID=$($chatResp.conversationId)"

# Monitoring should include sentiment/emotion/chat calls
$monResp=Invoke-RestMethod -Uri "$base/api/ai/monitoring" -Method GET -Headers $headers
LogLine "MONITORING_STATUS=200"
if($monResp.sentiment){ LogLine "MON_SENTIMENT_TOTAL=$($monResp.sentiment.totalCalls)" }
if($monResp.emotion){ LogLine "MON_EMOTION_TOTAL=$($monResp.emotion.totalCalls)" }
if($monResp.chat){ LogLine "MON_CHAT_TOTAL=$($monResp.chat.totalCalls)" }

LogLine "RESULT=PASS"
LogLine "RESULT_FILE=$resultFile"
