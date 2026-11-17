using UnityEngine;
using UnityEngine.UI;
using TMPro;

// HUD base: barras de vida e stamina e contador de munição.
public class HUDJogador : MonoBehaviour
{
    public Image barraVida;
    public Image barraStamina;
    public TMP_Text textoMunicao;

    public Color corStaminaNormal = new Color(0.95f, 0.8f, 0.2f);
    public Color corStaminaExausto = new Color(0.85f, 0.25f, 0.2f);

    private ControloJogador controlo;
    private VidaJogador vida;

    void Start()
    {
        // O HUD é um prefab, por isso encontra o Jogador sozinho em cada cena
        controlo = FindAnyObjectByType<ControloJogador>();
        vida = FindAnyObjectByType<VidaJogador>();
    }

    void Update()
    {
        if (vida != null)
        {
            barraVida.fillAmount = vida.vidaAtual / vida.vidaMaxima;
        }

        if (controlo != null)
        {
            barraStamina.fillAmount = controlo.staminaAtual / controlo.staminaMaxima;
            barraStamina.color = controlo.estaExausto ? corStaminaExausto : corStaminaNormal;
        }
    }

    // Vai ser chamado pelo sistema de munição (Semana 4)
    public void AtualizarMunicao(int municaoAtual, int municaoMaxima)
    {
        textoMunicao.text = municaoAtual + " / " + municaoMaxima;
    }
}