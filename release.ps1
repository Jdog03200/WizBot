function GitHub-Release($versionNumber)
{
    $ErrorActionPreference = "Stop"
    $env:WIZBOT_INSTALL_VERSION=$versionNumber
    
    $draft = $TRUE

    $lastTag = git describe --tags --abbrev=0
    $tag = "$lastTag..HEAD"
    $changelog = & 'git' 'log', $tag, '--oneline'

    $gitHubApiKey = $env:GITHUB_API_KEY

    $commitId = git rev-parse HEAD

    # set-alias sz "$env:ProgramFiles\7-Zip\7z.exe" 
    # $source = "src\WizBot\bin\Release\PublishOutput\win7-x64" 
    # $target = "src\WizBot\bin\Release\PublishOutput\WizBot.7z"

    # sz 'a' '-mx3' $target $source

    & "C:\Program Files (x86)\Inno Setup 5\iscc.exe" "/O+" ".\WizBot.iss"

    $artifact = "WizBot-setup-$versionNumber.exe";

    $auth = 'Basic ' + [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes($gitHubApiKey + ":x-oauth-basic"));

    $result = GitHubMake-Release $versionNumber $commitId $TRUE $gitHubApiKey $auth ""
    $releaseId = $result | Select -ExpandProperty id
    $uploadUri = $result | Select -ExpandProperty upload_url
    $uploadUri = $uploadUri -creplace '\{\?name,label\}', "?name=$artifact"
    Write-Host $releaseId $uploadUri
    $uploadFile = [Environment]::GetFolderPath('MyDocuments') + "\projekti\WizBotInstallerOutput\$artifact"

    $uploadParams = @{
      Uri = $uploadUri;
      Method = 'POST';
      Headers = @{
        Authorization = $auth;
      }
      ContentType = 'application/x-msdownload';
      InFile = $uploadFile
    }

    Write-Host 'Uploading artifact'
    $result = Invoke-RestMethod @uploadParams
    Write-Host 'Artifact upload finished.'
    $result = GitHubMake-Release $versionNumber $commitId $FALSE $gitHubApiKey $auth "$releaseId"
    Write-Host 'Done 🎉'
}

function GitHubMake-Release($versionNumber, $commitId, $draft, $gitHubApiKey, $auth, $releaseId = "")
{
    $releaseId = If ($releaseId -eq "") {""} Else {"/" + $releaseId};

    Write-Host $versionNumber
    Write-Host $commitId
    Write-Host $draft
    Write-Host $gitHubApiKey
    Write-Host $releaseId
    Write-Host $auth

    $releaseData = @{
       tag_name = $versionNumber;
       target_commitish = $commitId;
       name = [string]::Format("WizBot v{0}", $versionNumber);
       body = "test";
       draft = $draft;
       prerelease = $releaseId -ne "";
    }

    $releaseParams = @{
       Uri = "https://api.github.com/repos/Wizkiller96/WizBot/releases" + $releaseId;
       Method = 'POST';
       Headers = @{
         Authorization = $auth;
       }
       ContentType = 'application/json';
       Body = (ConvertTo-Json $releaseData -Compress)
    }
    return Invoke-RestMethod @releaseParams
}