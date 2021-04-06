module dEth

type address = string

type DethState = {
    gulper : address;
    cdpId : bigint;
    makerManager : address;
    ethGemJoin : address;
    saverProxyActions : address;
    oracle : address;
    minRedemptionRatio : bigint;
    automationFeePerc : bigint;
}

let changeGulper (dethState:DethState) (gulper:address) = 
    { dethState with gulper = gulper }