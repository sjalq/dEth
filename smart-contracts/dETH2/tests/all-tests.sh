cd ..
truffle compile
cd tests

# uncomment the line below if you would like to run the tests with only all-tests.sh
# ganache-cli --quiet --port 7545 --mnemonic "join topple vapor pepper sell enter isolate pact syrup shoulder route token" &

dotnet test --filter DisplayName~M000

# dotnet test --filter DisplayName~B_C000 # s: This doesn't seem to correspond to any test or spec line
dotnet test --filter DisplayName~B_C001
dotnet test --filter DisplayName~B_C008
dotnet test --filter DisplayName~B_EN001
dotnet test --filter DisplayName~B_EN002
dotnet test --filter DisplayName~B_EN003
dotnet test --filter DisplayName~B_EN004
dotnet test --filter DisplayName~B_EN005
dotnet test --filter DisplayName~B_EN006
dotnet test --filter DisplayName~B_EN007
dotnet test --filter DisplayName~B_EN008
dotnet test --filter DisplayName~B_EX001
dotnet test --filter DisplayName~B_EX002
dotnet test --filter DisplayName~B_EX003
dotnet test --filter DisplayName~B_EX004
dotnet test --filter DisplayName~B_EX005

dotnet test --filter DisplayName~F_C001
dotnet test --filter DisplayName~F_CO001
dotnet test --filter DisplayName~F_CO002
dotnet test --filter DisplayName~F_F001
dotnet test --filter DisplayName~F_F002
dotnet test --filter DisplayName~F_F003A
dotnet test --filter DisplayName~F_F003B
dotnet test --filter DisplayName~F_FB001