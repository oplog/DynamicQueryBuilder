ApiKey=$1
Source=$2

nuget pack ./DynamicQueryBuilder/DynamicQueryBuilder.nuspec -Verbosity detailed

nuget push ./DynamicQueryBuilder.*.nupkg -Verbosity detailed -ApiKey $ApiKey -Source $Source