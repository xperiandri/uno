set CombinedConfiguration="Release|AnyCPU"
msbuild uno.ui.build.csproj "/p:BUILD_BUILDNUMBER=test_test_8888;BUILD_ARTIFACTSTAGINGDIRECTORY=.\artifacts" /m:16 /clp:PerformanceSummary;Summary /bl
pause
