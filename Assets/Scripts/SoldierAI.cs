using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum SoldierStates
{
    Idle,
    Roam,
    Danger
}

public class SoldierAI : MonoBehaviour
{

    [SerializeField] private NavMeshAgent _agent;

    public Animator soldierAnimator;

    [SerializeField] private Transform _enemy;
    [SerializeField] private float _enemyDistance;

    [SerializeField] private SoldierStates _state;

    [SerializeField] private float _visualHeighMultiplier;
    [SerializeField] private float _visualDistance;

    [SerializeField] private bool _canRoam;
    [SerializeField] private bool _canHide;
    [SerializeField] private bool _canControl;
    private bool _isNear;
    private bool _isInSpot;
    private bool _isInDanger;
    private bool _dontMove;
    [SerializeField] private bool _isHalf;
    [SerializeField] private bool _columnSelectable;

    [SerializeField] private GameObject _nearestHideableGameobject;
    [SerializeField] private float _nearestHideableDistance;
    [SerializeField] private GameObject _nearestHideableSpot;
    [SerializeField] private float _nearestSpotDistance;

    [SerializeField] private Collider[] _hideableGameobjects;
    [SerializeField] private LayerMask _hideableLayer;

    void Start()
    {
        _agent = this.GetComponent<NavMeshAgent>();
        _canRoam = true;
    }


    void Update()
    {

        Debug.DrawRay(transform.position + Vector3.up * _visualHeighMultiplier, transform.forward * _visualDistance, Color.green);
        Debug.DrawRay(transform.position + Vector3.up * _visualHeighMultiplier, (transform.forward + transform.right).normalized * _visualDistance, Color.green);
        Debug.DrawRay(transform.position + Vector3.up * _visualHeighMultiplier, (transform.forward + transform.right / 2).normalized * _visualDistance, Color.green);
        Debug.DrawRay(transform.position + Vector3.up * _visualHeighMultiplier, (transform.forward + transform.right / 4).normalized * _visualDistance, Color.green);
        Debug.DrawRay(transform.position + Vector3.up * _visualHeighMultiplier, (transform.forward - transform.right).normalized * _visualDistance, Color.green);
        Debug.DrawRay(transform.position + Vector3.up * _visualHeighMultiplier, (transform.forward - transform.right / 2).normalized * _visualDistance, Color.green);
        Debug.DrawRay(transform.position + Vector3.up * _visualHeighMultiplier, (transform.forward - transform.right / 4).normalized * _visualDistance, Color.green);

        RaycastControl();

        switch (_state)
        {
            case SoldierStates.Idle:

                _canControl = false;

                soldierAnimator.SetBool("Walk", false);
                soldierAnimator.SetBool("Run", false);


                if (_isInSpot)
                {
                    

                    if (_isHalf)
                    {
                        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(_nearestHideableSpot.transform.rotation.x, _nearestHideableSpot.transform.rotation.y + 45, _nearestHideableSpot.transform.rotation.z), .02f);
                        soldierAnimator.SetBool("Crouch", true);
                        _columnSelectable = true;
                    }
                    else
                    {
                        transform.rotation = Quaternion.Lerp(transform.rotation, _nearestHideableSpot.transform.rotation, .02f);
                    }
                        

                }

                if (_canRoam && !_isInDanger)
                {
                    DoRoam(Random.Range(15, 20));
                }

                if (_isInDanger)
                {
                    ControlEnemyPosition();
                }


                break;

            case SoldierStates.Roam:

                if (!_isNear)
                    soldierAnimator.SetBool("Walk", true);
                else
                    soldierAnimator.SetBool("Walk", false);

                soldierAnimator.SetBool("Run", false);


                if (_agent.remainingDistance > 0)
                {
                    _canControl = true;
                    
                }


                if (_canControl)
                {

                    if (_agent.remainingDistance <= 0.75f)
                    {
                        _isNear = true;
                    }

                    if (_agent.remainingDistance == 0f)
                    {
                        _agent.velocity = Vector3.zero;
                        _agent.isStopped = true;
                        SetState(SoldierStates.Idle);
                        _canControl = false;
                        _isNear = false;
                    }
                }

                break;

            case SoldierStates.Danger:

                soldierAnimator.SetBool("Walk", false);
                soldierAnimator.SetBool("Run", true);

                if (_canHide)
                {
                    HideSomewhere();
                }

                if (_agent.remainingDistance > 0)
                {
                    _canControl = true;
                }

                if (_canControl)
                {
                    if (_agent.remainingDistance <= 0f)
                    {
                        _agent.velocity = Vector3.zero;
                        _agent.isStopped = true;
                        SetState(SoldierStates.Idle);
                        _canRoam = true;
                        _isInSpot = true;
                        _canControl = false;
                    }
                }

                break;
        }

        //ControlEnemyDistance();
    }


    public void SetState(SoldierStates stateName)
    {
        _state = stateName;
        _canControl = false;

        switch (stateName)
        {
            case SoldierStates.Roam:
                _agent.speed = 1f;
                break;
            case SoldierStates.Danger:
                _agent.speed = 3.5f;
                break;
            default:
                break;
        }
    }
    private void HideSomewhere()
    {
        _canHide = false;
        _agent.isStopped = false;

        _hideableGameobjects = Physics.OverlapSphere(transform.position, 50f, _hideableLayer.value);

        for (int i = 0; i < _hideableGameobjects.Length; i++)
        {

            if (_columnSelectable)
            {
                print(_hideableGameobjects[i].tag);
                print("1");
                if (_nearestHideableGameobject == null)
                {
                    if (_hideableGameobjects[i].tag != "HalfWall")
                    {
                        _nearestHideableGameobject = _hideableGameobjects[i].gameObject;

                        _nearestHideableDistance = Vector3.Distance(transform.position, _nearestHideableGameobject.transform.position);
                        print("2");
                    }
                }
                else
                {
                    if (Vector3.Distance(transform.position, _hideableGameobjects[i].transform.position) < _nearestHideableDistance)
                    {
                        if (_hideableGameobjects[i].tag != "HalfWall")
                        {
                            _nearestHideableGameobject = _hideableGameobjects[i].gameObject;

                            _nearestHideableDistance = Vector3.Distance(transform.position, _nearestHideableGameobject.transform.position);
                        }
                    }
                }

                print(_hideableGameobjects[i].tag);
            }
            else
            {
                print("4");
                if (_nearestHideableGameobject == null)
                {
                    _nearestHideableGameobject = _hideableGameobjects[i].gameObject;

                    _nearestHideableDistance = Vector3.Distance(transform.position, _nearestHideableGameobject.transform.position);
                }
                else
                {
                    if (Vector3.Distance(transform.position, _hideableGameobjects[i].transform.position) < _nearestHideableDistance)
                    {
                        _nearestHideableGameobject = _hideableGameobjects[i].gameObject;

                        _nearestHideableDistance = Vector3.Distance(transform.position, _nearestHideableGameobject.transform.position);
                    }
                }
            }
        }

        _columnSelectable = false;

        for (int i = 0; i < _nearestHideableGameobject.transform.childCount; i++)
        {
            if (_nearestHideableSpot == null)
            {
                _nearestHideableSpot = _nearestHideableGameobject.transform.GetChild(i).gameObject;

                _nearestSpotDistance = Vector3.Distance(_nearestHideableSpot.transform.position, _enemy.transform.position);
            }
            else
            {
                if (Vector3.Distance(_enemy.transform.position, _nearestHideableGameobject.transform.GetChild(i).position) > _nearestSpotDistance && _nearestHideableSpot != _nearestHideableGameobject.transform.GetChild(i).gameObject)
                {
                    _nearestHideableSpot = _nearestHideableGameobject.transform.GetChild(i).gameObject;

                    _nearestSpotDistance = Vector3.Distance(_nearestHideableSpot.transform.position, _enemy.transform.position);
                }
                else if(_nearestHideableSpot == _nearestHideableGameobject.transform.GetChild(i).gameObject)
                {

                    _nearestSpotDistance = 0f;
                    int notAllowedChild = i;

                    if (i >= _nearestHideableGameobject.transform.childCount - 1)
                    {
                        
                        for (int k = 0; k < _nearestHideableGameobject.transform.childCount; k++)
                        {

                            if (Vector3.Distance(_enemy.transform.position, _nearestHideableGameobject.transform.GetChild(k).position) > _nearestSpotDistance && k != notAllowedChild)
                            {
                                _nearestHideableSpot = _nearestHideableGameobject.transform.GetChild(k).gameObject;

                                _nearestSpotDistance = Vector3.Distance(_nearestHideableSpot.transform.position, _enemy.transform.position);
                            }
                        }
                    }
                }
            }
        }


        if (_nearestHideableGameobject.tag == "HalfWall")
        {
            _isHalf = true;
        }
        else
        {
            _isHalf = false;
        }

        SetState(SoldierStates.Danger);
        _agent.SetDestination(_nearestHideableSpot.transform.position);

    }
    private void ControlEnemyDistance()
    {
        
    }
    private void DoRoam(float time)
    {
        _canRoam = false;
        _agent.isStopped = false;
        SetState(SoldierStates.Roam);
        _agent.SetDestination(new Vector3(transform.position.x + Random.Range(-10, 10), transform.position.y, transform.position.z + Random.Range(-10, 10)));


        StartCoroutine(RoamState(time));
    }
    IEnumerator RoamState(float time)
    {
        yield return new WaitForSeconds(time);
        _canRoam = true;
    }
    private void RaycastControl()
    {
        if (_state != SoldierStates.Danger && !_isInDanger)
        {
            RaycastHit hit;


            if (Physics.Raycast(transform.position + Vector3.up * _visualHeighMultiplier, transform.forward, out hit, _visualDistance))
            {
                if (hit.collider.tag == "Enemy")
                {
                    SetState(SoldierStates.Danger);
                    _canHide = true;
                    _isInDanger = true;
                }
            }

            if (Physics.Raycast(transform.position + Vector3.up * _visualHeighMultiplier, (transform.forward + transform.right).normalized, out hit, _visualDistance))
            {
                if (hit.collider.tag == "Enemy")
                {
                    SetState(SoldierStates.Danger);
                    _canHide = true;

                    _isInDanger = true;
                }
            }

            if (Physics.Raycast(transform.position + Vector3.up * _visualHeighMultiplier, (transform.forward + transform.right / 2).normalized, out hit, _visualDistance))
            {
                if (hit.collider.tag == "Enemy")
                {
                    SetState(SoldierStates.Danger);
                    _canHide = true;

                    _isInDanger = true;
                }
            }

            if (Physics.Raycast(transform.position + Vector3.up * _visualHeighMultiplier, (transform.forward + transform.right / 4).normalized, out hit, _visualDistance))
            {
                if (hit.collider.tag == "Enemy")
                {
                    SetState(SoldierStates.Danger);
                    _canHide = true;

                    _isInDanger = true;
                }
            }

            if (Physics.Raycast(transform.position + Vector3.up * _visualHeighMultiplier, (transform.forward - transform.right).normalized, out hit, _visualDistance))
            {
                if (hit.collider.tag == "Enemy")
                {
                    SetState(SoldierStates.Danger);
                    _canHide = true;

                    _isInDanger = true;
                }
            }

            if (Physics.Raycast(transform.position + Vector3.up * _visualHeighMultiplier, (transform.forward - transform.right / 2).normalized, out hit, _visualDistance))
            {
                if (hit.collider.tag == "Enemy")
                {
                    SetState(SoldierStates.Danger);
                    _canHide = true;

                    _isInDanger = true;
                }
            }

            if (Physics.Raycast(transform.position + Vector3.up * _visualHeighMultiplier, (transform.forward - transform.right / 2).normalized, out hit, _visualDistance))
            {
                if (hit.collider.tag == "Enemy")
                {
                    SetState(SoldierStates.Danger);
                    _canHide = true;

                    _isInDanger = true;
                }
            }
        }
    }
    private void ControlEnemyPosition()
    {

        Vector3 dir = (_enemy.transform.position - transform.position).normalized;

        Debug.DrawLine(transform.position, transform.position + dir * 20, Color.red);
        
        RaycastHit hitted;

        if (Physics.Raycast(transform.position, dir, out hitted, Mathf.Infinity))
        {

            if (hitted.collider.tag == "Enemy")
            {
                _canHide = true;
                _agent.isStopped = false;
                _isInDanger = true;
                soldierAnimator.SetBool("Crouch", false);
                if (_isHalf)
                {
                    if (!_dontMove)
                    {
                        StartCoroutine(WaitForStand(1.5f));
                    }
                }
                else
                {
                    SetState(SoldierStates.Danger);
                }
                
            }
        }
    }


    IEnumerator WaitForStand(float time)
    {
        _dontMove = true;
        ResetYourDistance();
        yield return new WaitForSeconds(time);
        SetState(SoldierStates.Danger);
        _dontMove = false;
        _isHalf = false;
    }


    private void ResetYourDistance()
    {
        _nearestHideableGameobject = null;
        _nearestHideableDistance = 1000f;
        _nearestHideableSpot = null;
        _nearestSpotDistance = 0f;
    }

}




