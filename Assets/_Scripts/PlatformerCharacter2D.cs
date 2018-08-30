using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class PlatformerCharacter2D : MonoBehaviour
{

    [SerializeField] private float m_MaxSpeed = 10f;
    // The fastest the player can travel in the x axis.
    [SerializeField] private float m_JumpForce = 400f;
    // Amount of force added when the player jumps.
    [Range(0, 1)] [SerializeField] private float m_CrouchSpeed = .36f;
    // Amount of maxSpeed applied to crouching movement. 1 = 100%
    [SerializeField] private bool m_AirControl = false;
    // Whether or not a player can steer while jumping;
    [SerializeField] private LayerMask m_WhatIsGround;
    // A mask determining what is ground to the character

    private Transform m_GroundCheck;
    // A position marking where to check if the player is grounded.
    const float k_GroundedRadius = .2f;
    // Radius of the overlap circle to determine if grounded
    private bool m_Grounded;
    // Whether or not the player is grounded.
    private Transform m_CeilingCheck;
    // A position marking where to check for ceilings
    const float k_CeilingRadius = .01f;
    // Radius of the overlap circle to determine if the player can stand up
    [SerializeField] private Animator m_Anim;
    // Reference to the player's animator component.
    private Rigidbody2D m_Rigidbody2D;
    [HideInInspector] public bool m_FacingRight = true;
    // For determining which way the player is currently facing.

    public PensamentoController pensamentoController_Script;
    private bool cont = false;
    private GameController GOController;



    [SerializeField] private Camera2DFollow camera2DFollow_Script;
    private SystemDialogueMental systemDialogueMental_Script;
    private Canvas_MainScript mainScript_Canvas;
    [SerializeField] private Camera mainCamera;

    #region Trial Logger
    public TrialLogger trialLogger;
    //// participant id (string)
    //public string participantID = "0001";
    #endregion

    [HideInInspector] public string cityStateInfo;

    [Space(10)]
    [Header("Anima")]
    public GameObject[] anima_Idle;
    public GameObject[] anima_Walk;

    private bool increaseSpeed;

    public bool changeCameraSize = true;

    private void Awake()
    {
        // Setting up references.
        m_GroundCheck = transform.Find("GroundCheck");
        m_CeilingCheck = transform.Find("CeilingCheck");
        m_Anim = GetComponentInChildren<Animator>();
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        GOController = GameObject.Find("GameController").GetComponent<GameController>();
        camera2DFollow_Script = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera2DFollow>();
        systemDialogueMental_Script = GetComponent<SystemDialogueMental>();
        mainScript_Canvas = GameObject.Find("_MainCanvas").GetComponent<Canvas_MainScript>();
    }

    private void Start()
    {
        //List<string> columnList = new List<string> { "Local" };
        //trialLogger.Initialize(columnList);

    }

    private void FixedUpdate()
    {


        m_Grounded = false;

        // The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
        // This can be done using layers instead but Sample Assets will not overwrite your project settings.
        Collider2D[] colliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != gameObject)
                m_Grounded = true;
        }
        m_Anim.SetBool("Ground", m_Grounded);

        // Set the vertical animation
        m_Anim.SetFloat("vSpeed", m_Rigidbody2D.velocity.y);



    }

    void Update()
    {

        changeAnimation();

        if (Input.GetKeyDown(KeyCode.Y))
        {
            increaseSpeed = !increaseSpeed;
        }

        if (increaseSpeed)
        {
            if (m_MaxSpeed != 10)
            {
                m_MaxSpeed = 10;
            }
        }

        if (!increaseSpeed)
        {
            if (m_MaxSpeed != 5)
            {
                m_MaxSpeed = 5;
            }
        }

    }

    private void changeAnimation()
    {
        if (m_Anim.GetFloat("Speed") == 0f && m_Grounded)
        {
            for (int i = 0; i < anima_Idle.Length; i++)
            {
                if (anima_Idle[i].activeSelf == false)
                {
                    anima_Idle[i].SetActive(true);
                }
            }
            for (int i = 0; i < anima_Walk.Length; i++)
            {
                if (anima_Walk[i].activeSelf == true)
                {
                    anima_Walk[i].SetActive(false);
                }
            }
        }
        else
        {
            for (int i = 0; i < anima_Idle.Length; i++)
            {
                if (anima_Idle[i].activeSelf == true)
                {
                    anima_Idle[i].SetActive(false);
                }
            }
            for (int i = 0; i < anima_Walk.Length; i++)
            {
                if (anima_Walk[i].activeSelf == false)
                {
                    anima_Walk[i].SetActive(true);
                }
            }
        }
    }

    public void Move(float move, bool jump)
    {

        //only control the player if grounded or airControl is turned on
        if (m_Grounded || m_AirControl)
        {
            // The Speed animator parameter is set to the absolute value of the horizontal input.
            m_Anim.SetFloat("Speed", Mathf.Abs(move));

            // Move the character
            m_Rigidbody2D.velocity = new Vector2(move * m_MaxSpeed, m_Rigidbody2D.velocity.y);

            // If the input is moving the player right and the player is facing left...
            if (move > 0 && !m_FacingRight)
            {
                // ... flip the player.
                Flip();
            }
            // Otherwise if the input is moving the player left and the player is facing right...
            else if (move < 0 && m_FacingRight)
            {
                // ... flip the player.
                Flip();
            }
        }
        // If the player should jump...
        if (m_Grounded && jump /*&& m_Anim.GetBool("Ground")*/ && pensamentoController_Script.isThinking)
        {
            // Add a vertical force to the player.
            m_Grounded = false;
            m_Anim.SetBool("Ground", false);
            m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));
        }
    }

    private void Flip()
    {
        // Switch the way the player is labelled as facing.
        m_FacingRight = !m_FacingRight;

        // Multiply the player's x local scale by -1.
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }



    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "ThinkPlatforms_Tipo2")
        {
            if (other.gameObject.GetComponent<PensamentoNumberInfo>() != null)
            {
                if (other.gameObject.GetComponent<PensamentoNumberInfo>().numberPensamento == 1)
                {
                    pensamentoController_Script.tipo_resposta_Pesamento = "Resposta 1";
                }
                if (other.gameObject.GetComponent<PensamentoNumberInfo>().numberPensamento == 2)
                {
                    pensamentoController_Script.tipo_resposta_Pesamento = "Resposta 2";
                }
                if (other.gameObject.GetComponent<PensamentoNumberInfo>().numberPensamento == 3)
                {
                    pensamentoController_Script.tipo_resposta_Pesamento = "Resposta 3";
                }
            }
        }

        if (other.gameObject.tag == "Botao")
        {
            other.gameObject.SetActive(false);
        }

        if (other.gameObject.CompareTag("FimDemo"))
        {
            mainScript_Canvas.agradecimentosNoah();
        }


        if (other.gameObject.tag == "BeginCityTest")
        {
            trialLogger.StartTrial();
            other.gameObject.SetActive(false);
        }

        if (other.gameObject.tag == "EndCityTest")
        {
            if (cityStateInfo == "MetroEscola")
            {
                trialLogger.trial["Local"] = "Metro Escola";
            }

            if (cityStateInfo == "PortariaMetro")
            {
                trialLogger.trial["Local"] = "Portaria Metro";
            }

            if (cityStateInfo == "PapelariaMercado")
            {
                trialLogger.trial["Local"] = "Papelaria Mercado";
            }
            trialLogger.EndTrial();
            other.gameObject.SetActive(false);
        }

        if (other.gameObject.tag == "DayTimeChange")
        {
            other.gameObject.SetActive(false);
            GOController.dayChangeNumber += 1;
            if (GOController.dayChangeNumber == 4)
            {
                GOController.activateDayChange = true;
            }
            //if (GOController.dayChangeNumber == 6)
            //{
            //    GOController.activateDayChange = true;
            //}
            if (GOController.dayChangeNumber <= 4)
            {
                StartCoroutine(ReturnDayTimeChangeOBJ(10, other.gameObject));
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("PlatformEscolha"))
        {
            transform.SetParent(collision.transform);
        }

        if (collision.gameObject.CompareTag("PlatformBase"))
        {
            transform.SetParent(collision.transform);
        }

    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("PlatformEscolha"))
        {
            transform.parent = null;
        }

        if (collision.gameObject.CompareTag("PlatformBase"))
        {
            transform.parent = null;
        }


    }

    private void OnTriggerStay2D(Collider2D other)
    {
        //if (other.gameObject.tag == "ThinkPlatforms_Tipo2")
        //{
        //    if (other.gameObject.GetComponent<PensamentoNumberInfo>() != null)
        //    {
        //        PensamentoNumberInfo pensamementoNumber_script = other.gameObject.GetComponent<PensamentoNumberInfo>();
        //        if (Input.GetKeyDown(KeyCode.E))
        //        {
        //            //if (pensamementoNumber_script.gameStage_Info == "isPrologo")
        //            //{
        //            //    GOController.Atendente_Obj.SetActive(true);
        //            //}
        //            //pensamentoController_Script.voltaPosicao = true;
        //        }
        //    }
        //}

        if (changeCameraSize)
        {
            Debug.Log("Nisu");
            #region Camera Bounds
            if (camera2DFollow_Script.Papelaria_BG != null)
            {
                Debug.Log("Esta entrando no Bounds da papelaria");
                if (other.gameObject.transform.name == camera2DFollow_Script.Papelaria_BG.name)
                {
                    camera2DFollow_Script.minCameraPos = camera2DFollow_Script.Papelaria_minCam_Pos;
                    camera2DFollow_Script.maxCameraPos = camera2DFollow_Script.Papelaria_maxCam_Pos;
                }
            }

            if (other.gameObject.transform.CompareTag("PensamentoEscolha"))
            {
                camera2DFollow_Script.minCameraPos = camera2DFollow_Script.Pensamento_Tipo2_minCam_Pos;
                camera2DFollow_Script.maxCameraPos = camera2DFollow_Script.Pensamento_Tipo2_maxCam_Pos;
                mainCamera.orthographicSize = 7.05f;
            }

            if(camera2DFollow_Script.Quarto_Noah_BG != null)
            {
                if (other.gameObject.transform.name == camera2DFollow_Script.Quarto_Noah_BG.name)
                {
                    camera2DFollow_Script.minCameraPos = camera2DFollow_Script.QuartoNoah_minCam_Pos;
                    camera2DFollow_Script.maxCameraPos = camera2DFollow_Script.QuartoNoah_maxCam_Pos;
                    mainCamera.orthographicSize = 5f;
                }
            }
            if(camera2DFollow_Script.SalaDeEstar_Noah_BG != null)
            {
                if (other.gameObject.transform.name == camera2DFollow_Script.SalaDeEstar_Noah_BG.name)
                {
                    camera2DFollow_Script.minCameraPos = camera2DFollow_Script.SalaDeEstar_Noah_minCam_Pos;
                    camera2DFollow_Script.maxCameraPos = camera2DFollow_Script.SalaDeEstar_Noah__maxCam_Pos;
                    mainCamera.orthographicSize = 5f;
                }
            }

            if(camera2DFollow_Script.Portaria_Manha_BG != null || camera2DFollow_Script.Portaria_Tarde_BG != null)
            {
                if (other.gameObject.transform.name == camera2DFollow_Script.Portaria_Manha_BG.name || other.gameObject.transform.name == camera2DFollow_Script.Portaria_Tarde_BG.name)
                {
                    camera2DFollow_Script.minCameraPos = camera2DFollow_Script.Portaria_minCam_Pos;
                    camera2DFollow_Script.maxCameraPos = camera2DFollow_Script.Portaria_maxCam_Pos;

                    mainCamera.orthographicSize = 5.36f;
                }
            }

            if(camera2DFollow_Script.ParteBaixo_MetroEscola_BG != null)
            {
                if (other.gameObject.transform.name == camera2DFollow_Script.ParteBaixo_MetroEscola_BG.name)
                {
                    camera2DFollow_Script.minCameraPos = camera2DFollow_Script.ParteBaixo_MetroEscola_minCam_Pos;
                    camera2DFollow_Script.maxCameraPos = camera2DFollow_Script.ParteBaixo_MetroEscola_maxCam_Pos;

                    mainCamera.orthographicSize = 5.83f;
                    cityStateInfo = "MetroEscola";
                }
            }

            if(camera2DFollow_Script.ParteCima_MetroEscola_BG != null)
            {
                if (other.gameObject.transform.name == camera2DFollow_Script.ParteCima_MetroEscola_BG.name)
                {
                    camera2DFollow_Script.minCameraPos = camera2DFollow_Script.ParteCima_MetroEscola_minCam_Pos;
                    camera2DFollow_Script.maxCameraPos = camera2DFollow_Script.ParteCima_MetroEscola_maxCam_Pos;

                    mainCamera.orthographicSize = 5.83f;
                    cityStateInfo = "MetroEscola";

                }
            }


            if (SceneManager.GetActiveScene().name == "Dia_2" || SceneManager.GetActiveScene().name == "Dia_3")
            {
                if (other.gameObject.transform.name == camera2DFollow_Script.Nivel1_PapelariaMercado_BG.name)
                {
                    camera2DFollow_Script.minCameraPos = camera2DFollow_Script.Nivel1_PapelariaMercado_minCam_Pos;
                    camera2DFollow_Script.maxCameraPos = camera2DFollow_Script.Nivel1_PapelariaMercado_maxCam_Pos;

                    mainCamera.orthographicSize = 5.83f;
                    cityStateInfo = "PapelariaMercado";

                }

                if (other.gameObject.transform.name == camera2DFollow_Script.Nivel2_PapelariaMercado_BG.name)
                {
                    camera2DFollow_Script.minCameraPos = camera2DFollow_Script.Nivel2_PapelariaMercado_minCam_Pos;
                    camera2DFollow_Script.maxCameraPos = camera2DFollow_Script.Nivel2_PapelariaMercado_maxCam_Pos;

                    mainCamera.orthographicSize = 5.83f;
                    cityStateInfo = "PapelariaMercado";

                }

                if (other.gameObject.transform.name == camera2DFollow_Script.Nivel3_PapelariaMercado_BG.name)
                {
                    camera2DFollow_Script.minCameraPos = camera2DFollow_Script.Nivel3_PapelariaMercado_minCam_Pos;
                    camera2DFollow_Script.maxCameraPos = camera2DFollow_Script.Nivel3_PapelariaMercado_maxCam_Pos;

                    mainCamera.orthographicSize = 5.83f;
                    cityStateInfo = "PapelariaMercado";

                }
            }

            if(camera2DFollow_Script.Escola_BG != null)
            {
                if (other.gameObject.transform.name == camera2DFollow_Script.Escola_BG.name)
                {
                    camera2DFollow_Script.minCameraPos = camera2DFollow_Script.Escola_minCam_Pos;
                    camera2DFollow_Script.maxCameraPos = camera2DFollow_Script.Escola_maxCam_Pos;

                    mainCamera.orthographicSize = 5f;
                }
            }

            if(camera2DFollow_Script.Refeitorio_BG != null)
            {
                if (other.gameObject.transform.name == camera2DFollow_Script.Refeitorio_BG.name)
                {
                    camera2DFollow_Script.minCameraPos = camera2DFollow_Script.Refeitorio_minCam_Pos;
                    camera2DFollow_Script.maxCameraPos = camera2DFollow_Script.Refeitorio_maxCam_Pos;

                    mainCamera.orthographicSize = 5f;

                }
            }

            if(camera2DFollow_Script.SalaDeAula_BG != null)
            {
                if (other.gameObject.transform.name == camera2DFollow_Script.SalaDeAula_BG.name)
                {
                    camera2DFollow_Script.minCameraPos = camera2DFollow_Script.SalaDeAula_minCam_Pos;
                    camera2DFollow_Script.maxCameraPos = camera2DFollow_Script.SalaDeAula_maxCam_Pos;

                    mainCamera.orthographicSize = 5f;
                }
            }

            #endregion
        }
    }

    private IEnumerator ReturnDayTimeChangeOBJ(float seconds, GameObject dayTimeChangeOBJ)
    {
        yield return new WaitForSecondsRealtime(seconds);
        dayTimeChangeOBJ.SetActive(true);
    }
}

