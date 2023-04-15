using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using static Utils;

public class FollowEffects : UdonSharpBehaviour {
	public float runIntervalInSeconds = 2f;
	private bool runScheduled = false;
	private bool _running = false;
	public bool running {
		get => _running;
		set {
			_running = value;
			if(!runScheduled) {
				runScheduled = true;
				dynamicUpdate();
			}
		}
	}

	public Transform cloudShip1;
	public Transform cloudShip2;

	public void Start() {
		running = true;
	}

	public void dynamicUpdate() {
		if(Networking.LocalPlayer != null) {
			Vector3 position = Networking.LocalPlayer.GetPosition();
			position.y += 25;

			float minHeight = float.NegativeInfinity;

			float homeDistance = Vector2.Distance(new Vector2(position.x, position.z), new Vector2(0, 0));
			float cloudShip1Distance = Vector2.Distance(new Vector2(position.x, position.z), new Vector2(cloudShip1.position.x, cloudShip1.position.z));
			float cloudShip2Distance = Vector2.Distance(new Vector2(position.x, position.z), new Vector2(cloudShip1.position.x, cloudShip1.position.z));

			if(homeDistance < 150) {
				minHeight = Utils.Gradient(homeDistance, new double[] { 100, 150 }, new double[] { 25, 100 });
			}
			else if(cloudShip1Distance < 50) {
				minHeight = Utils.Gradient(cloudShip1Distance, new double[] { 40, 50 }, new double[] { 25, 100 });
			}
			else if(cloudShip2Distance < 50) {
				minHeight = Utils.Gradient(cloudShip2Distance, new double[] { 40, 50 }, new double[] { 25, 100 });
			}

			position.y = Mathf.Max(position.y, minHeight);

			transform.position = position;
		}

		if(_running) SendCustomEventDelayedSeconds(nameof(dynamicUpdate), runIntervalInSeconds);
		else runScheduled = false;
	}
}