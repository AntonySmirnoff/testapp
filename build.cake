#tool nuget:?package=NUnit.ConsoleRunner&version=3.4.0

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////
var target = Argument("target", "Default");
var webAppName = Argument("AppName", "TestApp");
var projectFile = Argument("ProjectFile", "./src/TestApp/TestApp.csproj");
var testProjectFile = Argument("TestProjectFile", "./src/TestAppTests/TestAppTests.csproj");
var configuration = Argument("Configuration", "Release");
var env = Argument("Env", "Dev");
var outDir = Argument("OutDir", "./artifacts");
var deployDir = Argument("deployDir", "Release");
//var compName = new Dictionary<string, string>();
//compName.Add("Dev", "dev.domain.com");
//compName.Add("Test", "test.domain.com");


//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////



//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectory(outDir);
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
      // Use DotNetCoreBuild
      DotNetCoreBuild(projectFile,
	  new DotNetCoreBuildSettings()
		 {
			 Framework = "netcoreapp2.0",
			 Configuration = configuration,
			 OutputDirectory = outDir
		 }
	 );
    
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
	CleanDirectory(deployDir);
	CopyDirectory(outDir, deployDir);
});


Task("Publish")
    .IsDependentOn("Run-Unit-Tests")
    .Does(() =>
{
    Zip(outDir, $"{webAppName}.zip");
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
