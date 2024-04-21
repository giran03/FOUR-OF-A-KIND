using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollisionHandler : MonoBehaviour
{
    [SerializeField] GameObject finishText;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Finish"))
            StartCoroutine(FinishReached());
    }

    IEnumerator FinishReached()
    {
        Debug.Log("Treasure Chest Reached!\nCongrats! :D");
        finishText.SetActive(true);
        yield return new WaitForSeconds(2f);
        SceneHandler.Instance.LevelFinish();
    }
}
