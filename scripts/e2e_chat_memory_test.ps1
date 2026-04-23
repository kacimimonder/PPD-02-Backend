param(
    [string]$Base = 'http://127.0.0.1:5194'
)

$base = $Base
$suffix=[DateTimeOffset]::UtcNow.ToUnixTimeSeconds()
$email="ai.e2e.$suffix@example.com"
$pwd='Pass@12345'
$resultFile = Join-Path $PSScriptRoot "e2e_chat_memory_result_$suffix.txt"

function LogLine([string]$line) {
    $line | Tee-Object -FilePath $resultFile -Append
}

LogLine "EMAIL=$email"

try {
        $createBody = curl.exe -s -w "`nHTTPSTATUS:%{http_code}" -X POST `
            -F "userCreateDTO.UserType=2" `
            -F "userCreateDTO.FirstName=AI" `
            -F "userCreateDTO.LastName=E2E" `
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

$imgPath=Join-Path $PSScriptRoot 'e2e-test-image.jpg'
Set-Content -Path $imgPath -Value 'fake-image-bytes' -Encoding Ascii

try {
        $courseBody = curl.exe -s -w "`nHTTPSTATUS:%{http_code}" -X POST `
            -H "Authorization: Bearer $token" `
            -F "Title=AI E2E Course $suffix" `
            -F "Description=Course for AI orchestration e2e test" `
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
        if (-not $courseIdMatch.Success) {
            LogLine "COURSE_PAYLOAD_RAW=$coursePayload"
            throw "Failed to parse courseId from response payload"
        }
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
    $moduleBody=@{courseId=$courseId;name='Memory Module';description='Module for conversation memory test'}|ConvertTo-Json
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
            -F "moduleContentCreateDTO.Name=Section 1" `
            -F "moduleContentCreateDTO.Content=Recursion is solving a problem by reducing it into smaller versions of the same problem." `
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

$conversationId="mem-$suffix"

try {
    $chat1Body=@{message='Explain recursion in one sentence.';conversationId=$conversationId;useServerMemory=$true;language='en';history=@()}|ConvertTo-Json -Depth 6
    $chat1Json=Invoke-RestMethod -Uri "$base/api/ai/modules/$moduleId/chat" -Method POST -Headers $headers -Body $chat1Body -ContentType 'application/json'
    LogLine "CHAT1_STATUS=200"
    LogLine "CHAT1_PROVIDER=$($chat1Json.provider)"
    LogLine "CHAT1_CONVERSATION_ID=$($chat1Json.conversationId)"
} catch {
    LogLine "CHAT1_ERR=$($_.Exception.Message)"
    if($_.ErrorDetails){ LogLine "CHAT1_BODY=$($_.ErrorDetails.Message)" }
    exit 1
}

try {
    $chat2Body=@{message='Now add a simple real-life example.';conversationId=$conversationId;useServerMemory=$true;language='en';history=@()}|ConvertTo-Json -Depth 6
    $chat2Json=Invoke-RestMethod -Uri "$base/api/ai/modules/$moduleId/chat" -Method POST -Headers $headers -Body $chat2Body -ContentType 'application/json'
    LogLine "CHAT2_STATUS=200"
    LogLine "CHAT2_PROVIDER=$($chat2Json.provider)"
    LogLine "CHAT2_CONVERSATION_ID=$($chat2Json.conversationId)"
} catch {
    LogLine "CHAT2_ERR=$($_.Exception.Message)"
    if($_.ErrorDetails){ LogLine "CHAT2_BODY=$($_.ErrorDetails.Message)" }
    exit 1
}

try {
    $monJson=Invoke-RestMethod -Uri "$base/api/ai/monitoring" -Method GET -Headers $headers
    LogLine "MON_STATUS=200"
    if($monJson.chat){
      LogLine "MON_CHAT_TOTAL=$($monJson.chat.totalCalls)"
      LogLine "MON_CHAT_SUCCESS=$($monJson.chat.successCalls)"
      LogLine "MON_CHAT_FAILED=$($monJson.chat.failedCalls)"
    }
} catch {
    LogLine "MON_ERR=$($_.Exception.Message)"
    if($_.ErrorDetails){ LogLine "MON_BODY=$($_.ErrorDetails.Message)" }
    exit 1
}

LogLine "RESULT_FILE=$resultFile"
exit 0
