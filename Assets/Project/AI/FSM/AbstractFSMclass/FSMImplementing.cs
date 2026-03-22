// Esempio di implementazione della classe FSM
using UnityEngine;

// 1. Eredita la classe : FSM
// 1. La tua classe AI eredita da FSM, così ottiene automaticamente il lifecycle Unity già cablato.
public class FSMImplementing : FSM
{
    // 2. Definisci i tuoi stati come enum
    // 2. L'enum ti dà un elenco finito e tipizzato di stati possibili. curState tiene traccia dello stato attuale
    public enum FSMState
    {
        None, Idle, Walk
    }
    // 2. curState tiene traccia dello stato attuale
    public FSMState curState = FSMState.Idle;

    // 3. Override di Initialize()
    protected override void Initialize()
    {
        // Do Something
        // Viene chiamato una volta sola (Start). Qui prepari tutto ciò che serve all'AI per funzionare.
    }

    // 4. Override di FSMUpdate — logica ogni frame
    protected override void FSMUpdate()
    {
        // switch (curState)
        {
            // case FSMState.Idle: UpdateIdleState(); break;
            // case FSMState.Walk: UpdateWalkState(); break;
        }
    }

    // 5. Ogni metodo di stato fa due cose
    // A) Verifica transizioni — controlla se deve cambiare stato
    // B) Esegue comportamento — fa quello che lo stato richiede
}
