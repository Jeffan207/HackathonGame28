using UnityEngine;
using System.Collections;

public class Arrow : MonoBehaviour {

    public Sprite up;
    public Sprite left;
    public Sprite right;

    public enum Direction
    {
        UP,
        LEFT,
        RIGHT
    }

    public void SetSprite(Direction direction)
    {
        switch (direction)
        {
            case Direction.UP:
                GetComponent<SpriteRenderer>().sprite = up;
                break;
            case Direction.LEFT:
                GetComponent<SpriteRenderer>().sprite = left;
                break;
            case Direction.RIGHT:
                GetComponent<SpriteRenderer>().sprite = right;
                break;
        }
    }
}
