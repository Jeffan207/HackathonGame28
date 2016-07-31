using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System;

public class GameList : MonoBehaviour {
    public Button gameButtonPrefab;

    public void AddGame(Action clickCallback, string gameName)
    {
        Button newButton = Instantiate(gameButtonPrefab);
        newButton.transform.SetParent(this.transform);

        newButton.onClick.AddListener(new UnityAction(clickCallback));
        newButton.GetComponentInChildren<Text>().text = gameName;
    }

    public void ClearList()
    {
        foreach(Transform child in this.transform)
        {
            Destroy(child.gameObject);
        }
    }
}
