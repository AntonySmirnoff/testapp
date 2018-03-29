#tool nuget:?package=NUnit.ConsoleRunner&version=3.4.0
//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");

var webAppName = Argument("AppName", "TestApp");
var projectFile = Argument("ProjectFile", "./src/TestApp/TestApp.csproj");
var testProjectFile = Argument("TestProjectFile", "./src/TestAppTests/TestAppTests.csproj");
var configuration = Argument("Configuration", "Release");
var outDir = Argument("OutDir", "./artifacts");
var deployDir = Argument("OutDir", "./publish");
var env = Argument("Env", "dev");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////
var buildSettings = new DotNetCoreBuildSettings
	 {
		 Framework = "netcoreapp2.0",
		 Configuration = configuration,
		 OutputDirectory = outDir
	 };
var publishSettings = new DotNetCorePublishSettings
     {
         Framework = "netcoreapp2.0",
         Configuration = configuration,
         OutputDirectory = deployDir,
     };
	 
var appcmdPath = @"C:\windows\system32\inetsrv\appcmd.exe";
//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectory(outDir);
	CleanDirectory(deployDir);
});

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
{
    DotNetCoreRestore(projectFile);
});

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
      DotNetCoreBuild(projectFile,buildSettings);
    
});

Task("Run-Unit-Tests")
    .IsDependentOn("Build")
    .Does(() =>
{
    DotNetCoreTest(testProjectFile);
});

Task("Deploy")
    .IsDependentOn("Run-Unit-Tests")
    .Does(() =>
{
	DotNetCorePublish(projectFile, publishSettings);
	
	//Set ASPNETCORE_ENVIRONMENT for target webapp
	using(var process = StartAndReturnProcess(appcmdPath, new ProcessSettings{ Arguments = "set config "+ webAppName +" /section:system.webServer/aspNetCore /+environmentVariables.[name='ASPNETCORE_ENVIRONMENT',value='"+env+"'] /commit:APPHOST" }))
{
    process.WaitForExit();
    Information("Exit code: {0}", process.GetExitCode());
}
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Deploy");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
