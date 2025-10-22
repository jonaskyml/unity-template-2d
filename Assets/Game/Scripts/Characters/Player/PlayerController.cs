using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
	public Rigidbody2D rb;
	public float moveSpeed = 5f;

	private Vector2 movementDirection;

	public InputActionReference moveAction;

	private void Update()
	{
		movementDirection = moveAction.action.ReadValue<Vector2>();
	}

	private void FixedUpdate()
	{
	    rb.linearVelocity = new Vector2(movementDirection.x * moveSpeed, movementDirection.y * moveSpeed);
	}
}