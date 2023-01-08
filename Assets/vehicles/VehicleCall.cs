
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class VehicleCall : UdonSharpBehaviour {
	public Vehicle vehicle;
	public int location;

	public void Interact() {
		if(location == 0) vehicle.homeIsland();
		if(location == 1) vehicle.homeIslandView();
		if(location == 2) vehicle.building();
		if(location == 3) vehicle.blahaj();
	}
}
