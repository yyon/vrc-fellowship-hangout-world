
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;
using VRC.Udon.Serialization.OdinSerializer;

public class Vroomba : UdonSharpBehaviour
{
    
    private Rigidbody _rb;
    private Animator _anim;
    private AudioSource _snd;
    private int _state;
    private float _rotateTimer;
    private float _rotateTarget = 2.5f;
    
    [UdonSynced] public bool isActive;
    public AudioClip[] antennaSounds;
    public UdonSharpBehaviour self;
    
    void Start()
    {
        _rb = (Rigidbody)transform.GetComponent(typeof(Rigidbody));
        _anim = (Animator)transform.GetComponentInChildren(typeof(Animator));
        _snd = (AudioSource)transform.GetComponentInChildren(typeof(AudioSource));
    }
    
    private void Update()
    {
        if (isActive)
        {
            _snd.volume = Mathf.Lerp(_snd.volume, 1, 0.1f);
            switch (_state)
            {
                case 0:
                    Forward();
                    break;
                case 1:
                    Rotate();
                    break;
            }
        }
        else
        {
            _snd.volume = Mathf.Lerp(_snd.volume, 0, 0.1f);
        }
    }
    
    public virtual void OnPickupUseDown()
    {
        self.SendCustomNetworkEvent(NetworkEventTarget.All, "ActivateIt");
    }
    
    private void OnCollisionEnter(Collision other)
    {
        if (other == null) return;
        CollideBaby();
    }
    
    private void Forward()
    {
        _rb.AddForce(transform.forward);
        _snd.pitch = Mathf.Lerp(_snd.pitch, 1, .05f);
    }
    
    private void Rotate()
    {
        _rotateTimer += Time.deltaTime;
        _snd.pitch = Mathf.Lerp(_snd.pitch, .75f, .05f);
        if (_rotateTimer < 0.5f) return;
        if (_rotateTimer < _rotateTarget)
        {
            transform.Rotate(transform.up);
        }
        else
        {
            _state = 0;
            _rotateTimer = 0;
            _rotateTarget = Random.Range(2f, 3f);
        }
    }
    
    public void CollideBaby()
    {
        if(!isActive || _state == 1) return;
        _state = 1;
        _snd.PlayOneShot(antennaSounds[2],1f);
    }
    
    public void ActivateIt()
    {
        isActive = !isActive;
        _snd.enabled = isActive;
        _anim.SetBool("Active", isActive);
        if (isActive)
        {
            _snd.PlayOneShot(antennaSounds[0],1f);
        }
        else
        {
            _snd.PlayOneShot(antennaSounds[1],1f);
        }
    }
}
