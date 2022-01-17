param($param1)

dotnet-ef.exe migrations add $param1 --project .\Database\.Database.csproj --startup-project .\NovumLobbyServer\NovumLobbyServer.csproj