nuget pack Transformalize.Provider.Ado.nuspec -OutputDirectory "c:\temp\modules" 
nuget pack Transformalize.Provider.Ado.Autofac.nuspec -OutputDirectory "c:\temp\modules" 

nuget pack Transformalize.Transform.Ado.nuspec -OutputDirectory "c:\temp\modules" 
nuget pack Transformalize.Transform.Ado.Autofac.nuspec -OutputDirectory "c:\temp\modules" 
 
nuget push "c:\temp\modules\Transformalize.Provider.Ado.0.11.2-beta.nupkg" -source https://www.myget.org/F/transformalize/api/v3/index.json
nuget push "c:\temp\modules\Transformalize.Provider.Ado.Autofac.0.11.2-beta.nupkg" -source https://www.myget.org/F/transformalize/api/v3/index.json
 
nuget push "c:\temp\modules\Transformalize.Transform.Ado.0.11.2-beta.nupkg" -source https://www.myget.org/F/transformalize/api/v3/index.json
nuget push "c:\temp\modules\Transformalize.Transform.Ado.Autofac.0.11.2-beta.nupkg" -source https://www.myget.org/F/transformalize/api/v3/index.json
