namespace Exercises.Storage.Entities;

public class Gym
{
    public string Id { get; set; }  // Vertex ID from the graph
    public string Name { get; set; }  // Gym name
    public List<ExerciseRoutine> Routines { get; set; }  // List of exercise routines
}