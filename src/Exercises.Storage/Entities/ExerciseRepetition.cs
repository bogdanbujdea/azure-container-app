namespace Exercises.Storage.Entities;

public class ExerciseRepetition
{
    public Exercise Exercise { get; set; }  // Exercise entity
    public int Reps { get; set; }  // Number of repetitions
}