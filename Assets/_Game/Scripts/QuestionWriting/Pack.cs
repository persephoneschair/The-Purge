using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class Pack
{
    public string author;
    public Question tiebreaker;
    public List<Question> mainGame = new List<Question>();
    public List<Question> purgeGame = new List<Question>();
    public List<Question> finalGame = new List<Question>();
}
