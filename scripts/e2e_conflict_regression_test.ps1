$base='http://127.0.0.1:5085'
$suffix=[DateTimeOffset]::UtcNow.ToUnixTimeSeconds()
$email="ai.reg.$suffix@example.com"
$pwd='Pass@12345'
$resultFile = Join-Path $PSScriptRoot "e2e_conflict_regression_result_$suffix.txt"

function LogLine([string]$line) {
    $line | Tee-Object -FilePath $resultFile -Append
}

LogLine "EMAIL=$email"

try {
    $createBody = curl.exe -s -w "`nHTTPSTATUS:%{http_code}" -X POST `
      -F "userCreateDTO.UserType=2" `
      -F "userCreateDTO.FirstName=AI" `
      -F "userCreateDTO.LastName=Regression" `
      -F "userCreateDTO.Email=$email" `
      -F "userCreateDTO.Password=$pwd" `
      "$base/api/User"

    $createStatus = ($createBody | Select-String -Pattern "HTTPSTATUS:(\d+)" -AllMatches).Matches[0].Groups[1].Value
    LogLine "CREATE_STATUS=$createStatus"
    if ($createStatus -ne "200" -and $createStatus -ne "201") {
        LogLine "CREATE_BODY=$createBody"
        exit 1
    }
} catch {
    LogLine "CREATE_ERR=$($_.Exception.Message)"
    if($_.ErrorDetails){ LogLine "CREATE_BODY=$($_.ErrorDetails.Message)" }
    exit 1
}

try {
    $loginBody=@{email=$email;password=$pwd}|ConvertTo-Json
    $loginResp=Invoke-RestMethod -Uri "$base/api/User/login" -Method POST -Body $loginBody -ContentType 'application/json'
    $token=$loginResp.token
    $userId=[int]$loginResp.id
    LogLine "LOGIN_STATUS=200"
    LogLine "LOGIN_USER_ID=$userId"
} catch {
    LogLine "LOGIN_ERR=$($_.Exception.Message)"
    if($_.ErrorDetails){ LogLine "LOGIN_BODY=$($_.ErrorDetails.Message)" }
    exit 1
}

$headers=@{Authorization="Bearer $token"}

$imgPath=Join-Path $PSScriptRoot 'regression-test-image.jpg'
Set-Content -Path $imgPath -Value 'fake-image-bytes' -Encoding Ascii

try {
    $courseBody = curl.exe -s -w "`nHTTPSTATUS:%{http_code}" -X POST `
      -H "Authorization: Bearer $token" `
      -F "Title=AI Regression Course $suffix" `
      -F "Description=Course for conflict regression test" `
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
    if ($courseStatus -ne "200" -and $courseStatus -ne "201") {
        LogLine "COURSE_BODY=$courseBody"
        exit 1
    }
} catch {
    LogLine "COURSE_ERR=$($_.Exception.Message)"
    if($_.ErrorDetails){ LogLine "COURSE_BODY=$($_.ErrorDetails.Message)" }
    exit 1
}

try {
    $moduleBody=@{courseId=$courseId;name='Conflict Recovery Module';description='Module to verify merged AI flows'}|ConvertTo-Json
    $moduleResp=Invoke-RestMethod -Uri "$base/api/CourseModule" -Method POST -Headers $headers -Body $moduleBody -ContentType 'application/json'
    $moduleId=[int]$moduleResp
    LogLine "MODULE_STATUS=200"
    LogLine "MODULE_ID=$moduleId"
} catch {
    LogLine "MODULE_ERR=$($_.Exception.Message)"
    if($_.ErrorDetails){ LogLine "MODULE_BODY=$($_.ErrorDetails.Message)" }
    exit 1
}

try {
    $mcBody = curl.exe -s -w "`nHTTPSTATUS:%{http_code}" -X POST `
      -H "Authorization: Bearer $token" `
      -F "moduleContentCreateDTO.Name=Section A" `
      -F "moduleContentCreateDTO.Content=Recursion solves a problem by breaking it into smaller similar problems. Base case stops recursion and recursive case continues it." `
      -F "moduleContentCreateDTO.CourseModuleID=$moduleId" `
      "$base/api/moduleContent"

    $mcStatus = ($mcBody | Select-String -Pattern "HTTPSTATUS:(\d+)" -AllMatches).Matches[0].Groups[1].Value
    LogLine "MODULE_CONTENT_STATUS=$mcStatus"
    if ($mcStatus -ne "200" -and $mcStatus -ne "201") {
        LogLine "MC_BODY=$mcBody"
        exit 1
    }
} catch {
    LogLine "MC_ERR=$($_.Exception.Message)"
    if($_.ErrorDetails){ LogLine "MC_BODY=$($_.ErrorDetails.Message)" }
    exit 1
}

# Summary endpoint (teammate changes: mode/language/limits)
try {
    $summaryBody=@{ maxBullets=6; language='en'; mode='Detailed' }|ConvertTo-Json
    $summaryResp=Invoke-RestMethod -Uri "$base/api/ai/modules/$moduleId/summary" -Method POST -Headers $headers -Body $summaryBody -ContentType 'application/json'
    LogLine "SUMMARY_STATUS=200"
    LogLine "SUMMARY_PROVIDER=$($summaryResp.provider)"
    LogLine "SUMMARY_MODEL=$($summaryResp.model)"
} catch {
    LogLine "SUMMARY_ERR=$($_.Exception.Message)"
    if($_.ErrorDetails){ LogLine "SUMMARY_BODY=$($_.ErrorDetails.Message)" }
    exit 1
}

# Quiz endpoint (teammate changes: difficulty/explanations)
try {
    $quizBody=@{ questionsCount=4; language='en'; difficulty='Hard'; includeExplanations=$true }|ConvertTo-Json
    $quizResp=Invoke-RestMethod -Uri "$base/api/ai/modules/$moduleId/quiz" -Method POST -Headers $headers -Body $quizBody -ContentType 'application/json'
    LogLine "QUIZ_STATUS=200"
    LogLine "QUIZ_PROVIDER=$($quizResp.provider)"
    LogLine "QUIZ_MODEL=$($quizResp.model)"
} catch {
    LogLine "QUIZ_ERR=$($_.Exception.Message)"
    if($_.ErrorDetails){ LogLine "QUIZ_BODY=$($_.ErrorDetails.Message)" }
    exit 1
}

# Chat endpoint (our changes: conversationId/useServerMemory/history merge)
$conversationId="reg-$suffix"
try {
    $chat1Body=@{message='Explain recursion in one sentence.';conversationId=$conversationId;useServerMemory=$true;language='en';history=@()}|ConvertTo-Json -Depth 6
    $chat1Resp=Invoke-RestMethod -Uri "$base/api/ai/modules/$moduleId/chat" -Method POST -Headers $headers -Body $chat1Body -ContentType 'application/json'
    LogLine "CHAT1_STATUS=200"
    LogLine "CHAT1_PROVIDER=$($chat1Resp.provider)"
    LogLine "CHAT1_CONVERSATION_ID=$($chat1Resp.conversationId)"
} catch {
    LogLine "CHAT1_ERR=$($_.Exception.Message)"
    if($_.ErrorDetails){ LogLine "CHAT1_BODY=$($_.ErrorDetails.Message)" }
    exit 1
}

try {
    $chat2Body=@{message='Now give a short real-life example.';conversationId=$conversationId;useServerMemory=$true;language='en';history=@()}|ConvertTo-Json -Depth 6
    $chat2Resp=Invoke-RestMethod -Uri "$base/api/ai/modules/$moduleId/chat" -Method POST -Headers $headers -Body $chat2Body -ContentType 'application/json'
    LogLine "CHAT2_STATUS=200"
    LogLine "CHAT2_PROVIDER=$($chat2Resp.provider)"
    LogLine "CHAT2_CONVERSATION_ID=$($chat2Resp.conversationId)"
} catch {
    LogLine "CHAT2_ERR=$($_.Exception.Message)"
    if($_.ErrorDetails){ LogLine "CHAT2_BODY=$($_.ErrorDetails.Message)" }
    exit 1
}

# Monitoring endpoint (our changes)
try {
    $monResp=Invoke-RestMethod -Uri "$base/api/ai/monitoring" -Method GET -Headers $headers
    LogLine "MONITORING_STATUS=200"
    if($monResp.chat){
        LogLine "MON_CHAT_TOTAL=$($monResp.chat.totalCalls)"
        LogLine "MON_CHAT_SUCCESS=$($monResp.chat.successCalls)"
        LogLine "MON_CHAT_FAILED=$($monResp.chat.failedCalls)"
    }
    if($monResp.summary){
        LogLine "MON_SUMMARY_TOTAL=$($monResp.summary.totalCalls)"
    }
    if($monResp.quiz){
        LogLine "MON_QUIZ_TOTAL=$($monResp.quiz.totalCalls)"
    }
} catch {
    LogLine "MONITORING_ERR=$($_.Exception.Message)"
    if($_.ErrorDetails){ LogLine "MONITORING_BODY=$($_.ErrorDetails.Message)" }
    exit 1
}

LogLine "RESULT=PASS"
LogLine "RESULT_FILE=$resultFile"
exit 0
