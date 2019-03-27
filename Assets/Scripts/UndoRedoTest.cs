//============================================================
// Project: RuntimeUndoRedo
// Author: Zoranner@ZORANNER
// Datetime: 2018-10-18 17:56:34
//============================================================

using JetBrains.Annotations;
using UndoMethods;
using UnityEngine;

public class UndoRedoTest : MonoBehaviour
{
    private Color _Color;
    public GameObject Cube;

    [UsedImplicitly]
    private void Start()
    {
        //随机添加10种颜色
        for (var i = 0; i < 10; i++)
        {
            var seed = Random.Range(0, 10000);
            Random.InitState(seed);
            var index = Random.Range(0, 3000);
            switch (index % 3)
            {
                case 0:
                    _Color = Color.red;
                    break;
                case 1:
                    _Color = Color.green;
                    break;
                case 2:
                    _Color = Color.blue;
                    break;
                default:
                    _Color = Color.white;
                    break;
            }

            SetColor(_Color);
        }

        CreateCube(Cube);
    }

    [UsedImplicitly]
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            UndoRedoManager.Instance().Redo();
        }
        else if (Input.GetKeyDown(KeyCode.U))
        {
            UndoRedoManager.Instance().Undo();
        }
    }

    private void SetColor(Color color)
    {
        // 存储上一次Cube颜色
        UndoRedoManager.Instance().Push(SetColor, GetComponent<Renderer>().material.color, "新增颜色");
        GetComponent<Renderer>().material.color = color;
    }

    private void CreateCube(GameObject preCube)
    {
        var newCube = Instantiate(preCube);
        newCube.name = "This is a New Cube";
        newCube.transform.position = newCube.transform.position - Vector3.left;
        UndoRedoManager.Instance().Push(DestroyCube, newCube, "Create Cube");
    }

    private void DestroyCube(GameObject cube)
    {
        Destroy(cube);
        UndoRedoManager.Instance().Push(CreateCube, Cube, "Destroy Cube");
    }

    [UsedImplicitly]
    private void OnGUI()
    {
        var captionStyle = new GUIStyle
        {
            fontSize = 30,
            fontStyle = FontStyle.Bold,
            normal = {textColor = Color.white}
        };

        var tipStyle = new GUIStyle
        {
            fontSize = 18,
            fontStyle = FontStyle.Normal,
            normal = {textColor = Color.white}
        };

        GUI.Label(new Rect(10, 10, 200, 80), "Runtime Undo/Redo Example", captionStyle);
        GUI.Label(new Rect(Screen.width - 320, Screen.height - 30, 320, 30),
            "Press <color=#00ff00>U</color> to Undo, and press <color=#00ff00>R</color> to Redo.",
            tipStyle);
    }
}