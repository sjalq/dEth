set -e

if [ $# -eq 0 ]
  then
    echo "Need Alchemy api key as argument."
fi

echo "Installing dependencies..."
npm install

echo "Building contracts..."
cd smart-contracts/dETH2
npx truffle build

echo "Installing dotnet..."
wget https://packages.microsoft.com/config/ubuntu/21.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get update
sudo apt-get install -y apt-transport-https
sudo apt-get update
sudo apt-get install -y dotnet-sdk-5.0

echo "Setting Alchemy key..."
cd tests
dotnet user-secrets set "AlchemyKey" "$1"