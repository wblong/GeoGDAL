using System;
using System.IO;
using UnrealBuildTool;

public class GDAL : ModuleRules
{
	
	public GDAL(ReadOnlyTargetRules Target) : base(Target)
	{
		Type = ModuleType.External;
		
		if (Target.IsInPlatformGroup(UnrealPlatformGroup.Windows))
		{
			HandleWindowsPlatform(Target);
		}
		
	}

	private string GetThirdPartyVersionString()
	{
		return "_2.4.0";
	}
	
	private string GetTargetPlatformString(ReadOnlyTargetRules Target)
	{
		string Temp = "undefined";
		
		if (UnrealTargetPlatform.Win64 == Target.Platform){
			Temp = "windows\\win64";
		}

		if (UnrealTargetPlatform.Win32 == Target.Platform) {
			Temp = "windows\\win32";
		}
		
		if (UnrealTargetPlatform.Android == Target.Platform) {
			Temp = "android";
		}

		return Temp;
	}
	
	private string GetBuildConfigurationString(ReadOnlyTargetRules Target)
	{
		string ConfigurationType = "release";
		
		bool bDebug = (Target.Configuration == UnrealTargetConfiguration.Debug && Target.bDebugBuildsActuallyUseDebugCRT);
		if(bDebug){
			ConfigurationType = "debug";
		}
		
		return ConfigurationType;
	}

	private void HandleWindowsPlatform(ReadOnlyTargetRules Target)
	{
		string ThirdPartyName = "GDAL";
		string BaseDir = Path.Combine(ModuleDirectory, ThirdPartyName + GetThirdPartyVersionString());
		string IncludeDir = Path.Combine(BaseDir, "includes");
		string LibDir = Path.Combine(BaseDir, "lib");
		string BinDir = Path.Combine(BaseDir, "bin");
		
		//add *.h
		PublicSystemIncludePaths.Add(IncludeDir);
		
		//add *.lib
		LibDir = Path.Combine(LibDir, GetTargetPlatformString(Target), GetBuildConfigurationString(Target));
		foreach (string LibPath in Directory.EnumerateFiles(LibDir, "*.lib", SearchOption.TopDirectoryOnly))
		{ 
			PublicAdditionalLibraries.Add(LibPath);
		}
		
		//add *.dll
		BinDir = Path.Combine(BinDir, GetTargetPlatformString(Target), GetBuildConfigurationString(Target));
		foreach (string DllPath in Directory.EnumerateFiles(BinDir, "*.dll", SearchOption.TopDirectoryOnly))
		{
			//for runtime
			FileInfo FileInfo = new FileInfo(DllPath);
			PublicDelayLoadDLLs.Add(FileInfo.Name);
			RuntimeDependencies.Add(Path.Combine("$(TargetOutputDir)", FileInfo.Name), DllPath, StagedFileType.NonUFS);
			
			DirectoryInfo DirInfo =  new DirectoryInfo(Path.Combine(Target.ProjectFile.Directory.ToString(), "Binaries/Win64"));
			if (!DirInfo.Exists)
			{
				DirInfo.Create();
			}
			
			//for Editor
			FileInfo DllFile = new FileInfo(Path.Combine(Target.ProjectFile.Directory.ToString(), "Binaries/Win64", FileInfo.Name));
			if (!DllFile.Exists)
			{
				FileInfo.CopyTo(DllFile.ToString());
			}
		}
		
		// add Engine ThirdParty
		AddEngineThirdPartyPrivateStaticDependencies(Target, "UElibPNG");
		AddEngineThirdPartyPrivateStaticDependencies(Target, "libcurl");
		AddEngineThirdPartyPrivateStaticDependencies(Target, "zlib");
		
		//copy data
		string DatPath = Path.Combine(BaseDir, "data");
		string TargetDir = Path.Combine(Target.ProjectFile.Directory.ToString(), "Binaries", "Data/GDAL");
		if(!Directory.Exists(TargetDir))
		{
			Directory.CreateDirectory(TargetDir);
		}

		foreach (string FilePath in Directory.EnumerateFiles(DatPath, "*.*", SearchOption.TopDirectoryOnly))
		{
			FileInfo FileInfo = new FileInfo(FilePath);
   
			RuntimeDependencies.Add(Path.Combine("$(TargetOutputDir)", FileInfo.Name), FilePath, StagedFileType.NonUFS);
			
			//Editor Only
			if (Target.bBuildEditor == true)
			{
				string TargetFile = Path.Combine(Target.ProjectFile.Directory.ToString(), "Binaries", "Data/GDAL",FileInfo.Name);
				
				if (!File.Exists(TargetFile))
				{
					FileInfo.CopyTo(TargetFile);
				}
			}
		}
	}
	
}
