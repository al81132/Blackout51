using UnityEngine;
using UnityEngine.InputSystem;

// Vida do agente: recebe dano, cura e, ao morrer, regressa ao último checkpoint.
public class VidaJogador : MonoBehaviour
{
    public float vidaMaxima = 100f;
    public float vidaAtual;

    private Vector3 posicaoCheckpoint;

    void Start()
    {
        vidaAtual = vidaMaxima;
        posicaoCheckpoint = transform.position;
    }

    void Update()
    {
#if UNITY_EDITOR
        // TESTE (só no editor): K tira 10 de vida, para ver a barra. Retirar mais tarde.
        if (Keyboard.current != null && Keyboard.current.kKey.wasPressedThisFrame)
        {
            ReceberDano(10f);
        }
#endif
    }

    public void ReceberDano(float dano)
    {
        vidaAtual -= dano;
        if (vidaAtual <= 0f)
        {
            Morrer();
        }
    }

    public void Curar(float quantidade)
    {
        vidaAtual = Mathf.Min(vidaMaxima, vidaAtual + quantidade);
    }

    public void DefinirCheckpoint(Vector3 posicao)
    {
        posicaoCheckpoint = posicao;
    }

    void Morrer()
    {
        // Desliga o CharacterController para poder mudar a posição à mão
        CharacterController cc = GetComponent<CharacterController>();
        cc.enabled = false;
        transform.position = posicaoCheckpoint;
        cc.enabled = true;

        vidaAtual = vidaMaxima;
    }
}