using UnityEngine;
using UnityEngine.InputSystem;

// Controlo base do agente em 1.ª pessoa: olhar, andar, saltar e gravidade.
// Player Input com "Broadcast Messages": cada Action chama o método On<NomeDaAction>.
// A FAZER (Rafael): sprint com stamina (Action "Sprint") e agachar (Action "Crouch").
public class ControloJogador : MonoBehaviour
{
    public GameObject objetoCamera;
    public float sensibilidadeX = 0.1f;
    public float sensibilidadeY = 0.1f;

    public float velocidadeMovimento = 5f;
    public float efeitoGravidade = 2f;
    public float forcaSalto = 5f;

    private CharacterController characterController;
    private Vector2 vetorRotacao;
    private Vector2 inputMovimento;
    private Vector3 movimento;

    public void OnLook(InputValue context)
    {
        Vector2 inputLook = context.Get<Vector2>();
        vetorRotacao.x += inputLook.x * sensibilidadeX;
        vetorRotacao.y = Mathf.Clamp(vetorRotacao.y + inputLook.y * sensibilidadeY, -90f, 90f);
    }

    public void OnMove(InputValue context)
    {
        inputMovimento = context.Get<Vector2>();
    }

    public void OnJump(InputValue context)
    {
        if (context.isPressed && characterController.isGrounded)
        {
            movimento.y = forcaSalto;
        }
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        characterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        // Rotação do jogador (Y) e da câmara (X)
        transform.localRotation = Quaternion.Euler(0f, vetorRotacao.x, 0f);
        objetoCamera.transform.localRotation = Quaternion.Euler(-vetorRotacao.y, 0f, 0f);

        // Movimento na direção para onde o jogador olha
        Vector3 direcao = transform.right * inputMovimento.x + transform.forward * inputMovimento.y;
        movimento.x = direcao.x * velocidadeMovimento;
        movimento.z = direcao.z * velocidadeMovimento;

        // Gravidade
        if (!characterController.isGrounded)
        {
            movimento.y += Physics.gravity.y * efeitoGravidade * Time.deltaTime;
        }
        else if (movimento.y < 0f)
        {
            movimento.y = Physics.gravity.y * efeitoGravidade;
        }

        characterController.Move(movimento * Time.deltaTime);
    }
}