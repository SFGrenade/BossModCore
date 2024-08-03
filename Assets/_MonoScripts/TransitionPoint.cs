using UnityEngine;
//using UnityEngine.Audio;

public class TransitionPoint : MonoBehaviour
{
	public bool isADoor = false;
	public bool dontWalkOutOfDoor = false;
	public float entryDelay = 0.0f;
	public bool alwaysEnterRight = false;
	public bool alwaysEnterLeft = false;
	public bool hardLandOnExit = false;
	public string targetScene;
	public string entryPoint;
	public Vector2 entryOffset = new Vector2(0.0f, 0.0f);
	public PlayMakerFSM customFadeFSM = null;
	public bool nonHazardGate = false;
	public HazardRespawnMarker respawnMarker;
	[HideInInspector]
	public AudioMixerSnapshot atmosSnapshot = null;
	[HideInInspector]
	public AudioMixerSnapshot enviroSnapshot = null;
	[HideInInspector]
	public AudioMixerSnapshot actorSnapshot = null;
	[HideInInspector]
	public AudioMixerSnapshot musicSnapshot = null;
	public GameManager.SceneLoadVisualizations sceneLoadVisualization = GameManager.SceneLoadVisualizations.Default;
	public bool customFade = false;
	public bool forceWaitFetch = false;
}
