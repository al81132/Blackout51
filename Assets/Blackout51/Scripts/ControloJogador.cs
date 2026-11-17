using UnityEngine;
using UnityEngine.InputSystem;

// Controlo do agente em 1.ª pessoa: olhar, andar, saltar, correr (com stamina) e agachar.
// Player Input com "Broadcast Messages": cada Action chama o método On<NomeDaAction>.
//   DESAFIO "Sprint com stamina": SHIFT faz correr e gasta stamina; se esgotar, o agente fica exausto.
//   DESAFIO "Agachar": C alterna entre de pé e agachado (CharacterController e câmara mais baixos).
public class ControloJogador : MonoBehaviour
{
    [Header("Câmara e rato")]
    public GameObject objetoCamera;
    public float sensibilidadeX = 0.1f;
    public float sensibilidadeY = 0.1f;

    [Header("Movimento")]
    public float velocidadeMovimento = 5f;
    public float efeitoGravidade = 2f;
    public float forcaSalto = 5f;

    [Header("Sprint com stamina")]
    public float multiplicadorSprint = 1.8f;          // 1.8 = 80% mais rápido
    public float staminaMaxima = 100f;
    public float staminaAtual;
    public float gastoStaminaPorSegundo = 25f;
    public float recuperacaoStaminaPorSegundo = 15f;
    public float atrasoRecuperacao = 1f;               // segundos sem correr até começar a recuperar
    public float percentagemSairExaustao = 0.3f;       // só volta a correr com 30% de stamina
    public bool estaExausto;
    public bool estaACorrer;

    [Header("Agachar")]
    public float alturaAgachado = 1.1f;
    public float multiplicadorAgachado = 0.5f;         // metade da velocidade
    public float velocidadeTransicao = 8f;
    public bool estaAgachado;                          // os robôs vão usar isto para detetar mais perto

    private CharacterController characterController;
    private PlayerInput playerInput;
    private Vector2 vetorRotacao;
    private Vector2 inputMovimento;
    private Vector3 movimento;
    private float tempoUltimoGasto;
    private bool querLevantar;
    private float alturaNormal;
    private Vector3 centroNormal;
    private float alturaCameraNormal;

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
        // Agachado não salta
        if (context.isPressed && characterController.isGrounded && !estaAgachado)
        {
            movimento.y = forcaSalto;
        }
    }

    // Action "Crouch" (tecla C): alterna entre agachado e de pé
    public void OnCrouch(InputValue context)
    {
        if (!context.isPressed)
        {
            return;
        }

        if (!estaAgachado)
        {
            estaAgachado = true;
            querLevantar = false;
        }
        else
        {
            // Só levanta no Update, se não houver teto por cima
            querLevantar = true;
        }
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        characterController = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();

        // Guarda os valores de pé, para conseguir voltar a eles
        alturaNormal = characterController.height;
        centroNormal = characterController.center;
        alturaCameraNormal = objetoCamera.transform.localPosition.y;

        staminaAtual = staminaMaxima;
    }

    void Update()
    {
        // Rotação do jogador (Y) e da câmara (X)
        transform.localRotation = Quaternion.Euler(0f, vetorRotacao.x, 0f);
        objetoCamera.transform.localRotation = Quaternion.Euler(-vetorRotacao.y, 0f, 0f);

        AtualizarAgachar();
        AtualizarStamina();

        // Velocidade conforme o estado
        float velocidadeAtual = velocidadeMovimento;
        if (estaAgachado)
        {
            velocidadeAtual *= multiplicadorAgachado;
        }
        else if (estaACorrer)
        {
            velocidadeAtual *= multiplicadorSprint;
        }

        Vector3 direcao = transform.right * inputMovimento.x + transform.forward * inputMovimento.y;
        movimento.x = direcao.x * velocidadeAtual;
        movimento.z = direcao.z * velocidadeAtual;

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

    // ---------- SPRINT COM STAMINA ----------
    void AtualizarStamina()
    {
        // O SHIFT tem de ser lido enquanto está premido, por isso usamos IsPressed()
        bool sprintPremido = playerInput.actions["Sprint"].IsPressed();
        bool estaAMover = inputMovimento.sqrMagnitude > 0.01f;

        // Só corre se estiver a mover-se, de pé e sem estar exausto
        estaACorrer = sprintPremido && estaAMover && !estaAgachado && !estaExausto;

        if (estaACorrer)
        {
            staminaAtual -= gastoStaminaPorSegundo * Time.deltaTime;
            tempoUltimoGasto = Time.time;

            if (staminaAtual <= 0f)
            {
                staminaAtual = 0f;
                estaExausto = true;
                estaACorrer = false;
            }
        }
        else if (Time.time - tempoUltimoGasto >= atrasoRecuperacao)
        {
            staminaAtual = Mathf.Min(staminaMaxima, staminaAtual + recuperacaoStaminaPorSegundo * Time.deltaTime);

            if (estaExausto && staminaAtual >= staminaMaxima * percentagemSairExaustao)
            {
                estaExausto = false;
            }
        }
    }

    // ---------- AGACHAR ----------
    void AtualizarAgachar()
    {
        if (querLevantar && PodeLevantar())
        {
            estaAgachado = false;
            querLevantar = false;
        }

        float alturaAlvo = estaAgachado ? alturaAgachado : alturaNormal;

        // Altura do CharacterController, com transição suave
        characterController.height = Mathf.Lerp(characterController.height, alturaAlvo, velocidadeTransicao * Time.deltaTime);

        // O centro desce metade do que a altura diminui, para os pés ficarem no mesmo sítio
        float descida = (alturaNormal - characterController.height) / 2f;
        characterController.center = new Vector3(centroNormal.x, centroNormal.y - descida, centroNormal.z);

        // A câmara acompanha a altura
        Vector3 posCamera = objetoCamera.transform.localPosition;
        posCamera.y = alturaCameraNormal - (alturaNormal - characterController.height);
        objetoCamera.transform.localPosition = posCamera;
    }

    // Há espaço por cima da cabeça para voltar a ficar de pé?
    bool PodeLevantar()
    {
        float raio = characterController.radius * 0.9f;
        Vector3 topo = transform.position + characterController.center
                       + Vector3.up * (characterController.height / 2f - characterController.radius);
        float distancia = alturaNormal - characterController.height + 0.1f;

        return !Physics.SphereCast(topo, raio, Vector3.up, out _, distancia, ~0, QueryTriggerInteraction.Ignore);
    }
}