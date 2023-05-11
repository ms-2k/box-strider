using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class PlayerController : MonoBehaviour
{
    //player ID
    //0 = apple
    //1 = barrel
    //2 = broccoli
    //3 = rice ball
    public int id = -1;

    //is the player the it
    public bool it = false;

    //base player speed
    private float speed = 5.0f;

    //how much to slow the player down by on wall collisioni (default 5)
    private float obstacleMult = 5f;

    //TODO
    //load from playerprefs and use this instead of public keycodes
    private readonly KeyCode[,] controlKeys;

    //control keys (default WASD)
    public KeyCode upKey = KeyCode.W;
    public KeyCode downKey = KeyCode.S;
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;

    //the rigid body for collision
    private Rigidbody playerRigidbody;

    //the game controller (to get variables)
    public GameController gameController;

    //it indicator
    private GameObject itIndicator;

    //sound controller object
    private SoundController soundController;

    //center of body level at which the object is no longer "on the ground"
    private float onGoundThreshold = 0.37f;

    //it transfer cooldown
    float cooldown = 0;

    private void Start()
    {
        //get rigid body
        playerRigidbody = GetComponent<Rigidbody>();

        //set base speed
        speed = gameController.baseSpeed;

        //set obstacle speed multipler
        obstacleMult = gameController.obstacleMult;

        //get it indicator controller
        itIndicator = gameController.itIndicator;

        //find sound controller object
        soundController = GameObject.FindGameObjectWithTag("SFX").GetComponent<SoundController>();
    }

    void Update()
    {
        if (!gameController.gameStart)
            return;

        //check if game over (disable controls if it's game over)
        if (gameController.gameOver)
            return;

        //don't do anything during cooldown
        if (cooldown > 0)
        {
            playerRigidbody.velocity = new Vector3(0f, 0f, 0f);
            cooldown -= Time.deltaTime;
            return;
        }

        //move up
        if (Input.GetKey(upKey) && playerRigidbody.worldCenterOfMass.y < onGoundThreshold)
        {
            playerRigidbody.velocity = new Vector3(playerRigidbody.velocity.x, 0f, speed);
        }
        //move down
        else if (Input.GetKey(downKey) && playerRigidbody.worldCenterOfMass.y < onGoundThreshold)
        {
            playerRigidbody.velocity = new Vector3(playerRigidbody.velocity.x, 0f, -speed);
        }
        //stop moving up/down if neither up/down keys are pressed
        else if (playerRigidbody.worldCenterOfMass.y < onGoundThreshold)
        {
            playerRigidbody.velocity = new Vector3(playerRigidbody.velocity.x, 0f, 0f);
        }

        //move left
        if (Input.GetKey(leftKey) && playerRigidbody.worldCenterOfMass.y < onGoundThreshold)
        {
            playerRigidbody.velocity = new Vector3(-speed, 0f, playerRigidbody.velocity.z);
        }
        //move right
        else if (Input.GetKey(rightKey) && playerRigidbody.worldCenterOfMass.y < onGoundThreshold)
        {
            playerRigidbody.velocity = new Vector3(speed, 0f, playerRigidbody.velocity.z);
        }
        //stop moving left/right if neither left/right keys are pressed
        else if (playerRigidbody.worldCenterOfMass.y < onGoundThreshold)
        {
            playerRigidbody.velocity = new Vector3(0f, 0f, playerRigidbody.velocity.z);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {

        //check if the player collided with an obstacle (wall)
        if (collision.gameObject.CompareTag("Wall"))
        {
            //make the player slower if so
            speed /= obstacleMult;
        }
        if (collision.gameObject.CompareTag("Player") && it && cooldown <= 0 && !gameController.gameOver)
        {
            //shortcut to collision target player controller
            PlayerController target = collision.gameObject.GetComponent<PlayerController>();

            //make this no longer it
            RevokeIt(gameController.itScale, gameController.itSpeed);

            //make the target it
            target.BecomeIt(gameController.itScale, gameController.itSpeed);

            //move it indicator to the new it
            itIndicator.GetComponent<IndicatorController>().ChangeTarget(collision.gameObject);

            //play hit sound
            soundController.playSwish();
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        //reverse collision slow down
        if (collision.gameObject.CompareTag("Wall"))
        {
            speed *= obstacleMult;
        }
    }

    //makes the player it
    public void BecomeIt(float scaleMult, float speedMult)
    {
        //update it ID in game controller
        gameController.itId = id;

        //set it
        it = true;

        //set cooldown of 3 seconds
        cooldown = 3;

        //make the it faster (otherwise it can't catch other players)
        speed *= speedMult;

        //upscale the it player to make it bigger
        //this makes the hitbox bigger too
        transform.localScale = new Vector3(
            scaleMult * transform.localScale.x,
            scaleMult * transform.localScale.y,
            scaleMult * transform.localScale.z
        );

        //increase threshold for flying
        onGoundThreshold *= scaleMult;
    }

    //makes the player not it
    public void RevokeIt(float scaleMult, float speedMult)
    {
        it = false;

        speed /= speedMult;
        transform.localScale = new Vector3(
            transform.localScale.x / scaleMult,
            transform.localScale.y / scaleMult,
            transform.localScale.z / scaleMult
        );

        onGoundThreshold /= scaleMult;
    }

    public void SetID(int id)
    {
        this.id = id;
    }
}
