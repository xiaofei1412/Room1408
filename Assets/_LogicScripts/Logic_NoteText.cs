using UnityEngine;

public class Logic_NoteText : MonoBehaviour
{
    [Header("Note Info")]
    public string noteTitle;

    [TextArea(5, 15)]
    public string noteContent;
}