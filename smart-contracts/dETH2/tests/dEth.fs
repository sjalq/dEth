module dEth

open Foundry.Contracts.dEth.ContractDefinition
    
let changeGulper (state:DEthDeployment) (gulper:string) = 
    state.Gulper <- gulper // todo redo dethdeployment class to record
    state

