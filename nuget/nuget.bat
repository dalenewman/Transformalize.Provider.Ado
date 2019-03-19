nuget pack Transformalize.Provider.Ado.nuspec -OutputDirectory "c:\temp\modules"
nuget pack Transformalize.Provider.Ado.Autofac.nuspec -OutputDirectory "c:\temp\modules"

nuget pack Transformalize.Transform.Ado.nuspec -OutputDirectory "c:\temp\modules"
nuget pack Transformalize.Transform.Ado.Autofac.nuspec -OutputDirectory "c:\temp\modules"

REM nuget push "c:\temp\modules\Transformalize.Provider.Ado.0.4.6-beta.nupkg" -source https://api.nuget.org/v3/index.json
REM nuget push "c:\temp\modules\Transformalize.Provider.Ado.Autofac.0.4.1-beta.nupkg" -source https://api.nuget.org/v3/index.json
REM 
REM nuget push "c:\temp\modules\Transformalize.Transform.Ado.0.4.1-beta.nupkg" -source https://api.nuget.org/v3/index.json
REM nuget push "c:\temp\modules\Transformalize.Transform.Ado.Autofac.0.4.1-beta.nupkg" -source https://api.nuget.org/v3/index.json







