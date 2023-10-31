namespace Exercises.Storage.Entities;

public class ExerciseRoutine
{
    public string Id { get; set; }  // Vertex ID from the graph
    public string Name { get; set; }  // Routine name
    public List<ExerciseRepetition> Exercises { get; set; }  // List of exercises with repetitions
}