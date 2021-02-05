# How to run the tests

1. Install the [Microsoft dotnet](https://dotnet.microsoft.com/download) platform.
2. Install Truffle ``npm install truffle -g``
3. Install Ganache ``npm install -g ganache-cli``
4. Launch Ganache with the following command line in a terminal ``ganache-cli --port 7545 --mnemonic "join topple vapor pepper sell enter isolate pact syrup shoulder route token"``
5. In a separate terminal run ``all-tests.sh`` in the tests folder

If you want to run individual tests, launch ganache as above and then use ``dotnet test --filter DisplayName~@TESTCODE`` where @TESTCODE is the code the test begins with, indicated in the specification. 

Note: Due to state conflicts cause by running tests consecutively the dotnet test harness will not pass all tests when running ``dotnet test``. We will fix this in future.