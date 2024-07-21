using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class Player : MonoBehaviour, IKitchenObjectParent {

    public static Player Instance { get; private set; }

    public event EventHandler<OnSelectedCounterChangedEventArgs> OnSelectedCounterChanged;
    public class OnSelectedCounterChangedEventArgs : EventArgs {
        public BaseCounter selectedCounter;
    }

    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float rotationSpeed = 7f;
    [SerializeField] private PlayerInput input;
    [SerializeField] private LayerMask countersLayerMask;
    [SerializeField] private Transform kitcheObjectHoldPoint;

    private KitchenObject kitchenObject;
    private BaseCounter selctedCounter;
    private bool isWalking;
    private Vector3 lastInteractionDir;



    private void Awake () {
        if (Instance != null) {
            Debug.LogError("Somhow here more than one player");
        }
        Instance = this;
    }

    private void Start () {

        input.OnInteractAction += Input_OnInteractAction;

    }



    private void Update () {
        HandleMovment();
        HandleInteractions();

    }
    #region Movment

    public bool IsWalking () {
        return isWalking;
    }

    private void HandleMovment () {
        float moveDistance = moveSpeed * Time.deltaTime;

        Vector2 inputVector = input.GetMovmentVectorNormalized();
        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);

        bool canMove = CanMove(moveDir, moveDistance);

        if (!canMove) {
            //Attempt only X movment
            Vector3 moveDirX = new Vector3(moveDir.x, 0, 0).normalized;
            canMove = CanMove(moveDirX, moveDistance);

            if (canMove) {
                moveDir = moveDirX;
            } else {
                //Attempt only Z movment
                Vector3 moveDirZ = new Vector3(0, 0, moveDir.z).normalized;
                canMove = CanMove(moveDirZ, moveDistance);
                if (canMove) {
                    moveDir = moveDirZ;
                }
            }

        }

        if (canMove) {
            transform.position += moveDir * moveDistance;
        }

        transform.forward = Vector3.Slerp(transform.forward, moveDir, Time.deltaTime * rotationSpeed);

        isWalking = moveDir != Vector3.zero;

    }

    private bool CanMove (Vector3 moveDir, float moveDistance) {
        float playerRadius = .7f;
        float playerHeight = 2f;

        return !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDir, moveDistance);
    }

    #endregion

    #region Interaction
    private void HandleInteractions () {
        Vector2 inputVector = input.GetMovmentVectorNormalized();
        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);
        float interactDistance = 2f;

        if (moveDir != Vector3.zero) {
            lastInteractionDir = moveDir;
        }

        if (Physics.Raycast(transform.position, lastInteractionDir, out RaycastHit raycastHit, interactDistance, countersLayerMask)) {
            if (raycastHit.transform.TryGetComponent(out BaseCounter counter)) {
                if (counter != selctedCounter) {
                    SetSelectedCounter(counter);
                }
            } else {
                SetSelectedCounter(null);
            }
        } else {
            SetSelectedCounter(null);
        }
    }

    private void Input_OnInteractAction (object sender, System.EventArgs e) {
        if (selctedCounter != null) {
            selctedCounter.Interact(this);
        }
    }

    private void SetSelectedCounter (BaseCounter selectedCounter) {
        this.selctedCounter = selectedCounter;

        OnSelectedCounterChanged?.Invoke(this, new OnSelectedCounterChangedEventArgs {
            selectedCounter = selctedCounter
        });
    }

    public Transform GetKitchenObjectFollowTransform () {
        return kitcheObjectHoldPoint;
    }

    public void SetKitchenObject (KitchenObject kitchenObject) {
        this.kitchenObject = kitchenObject;
    }

    public KitchenObject GetKitchenObject () {
        return kitchenObject;
    }

    public void ClearKitchenObject () {
        kitchenObject = null;
    }

    public bool HasKitchenObject () {
        return kitchenObject != null;
    }
    #endregion
}

