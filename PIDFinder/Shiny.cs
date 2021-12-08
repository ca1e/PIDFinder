namespace PIDFinder
{
    public enum Shiny : byte
    {
        /// <summary>
        /// PID is fixed to a specified value.
        /// </summary>
        FixedValue = 0,

        /// <summary>
        /// PID is purely random; can be shiny or not shiny.
        /// </summary>
        Random = 1,

        /// <summary>
        /// PID is randomly created and forced to be shiny.
        /// </summary>
        Always = 2,

        /// <summary>
        /// PID is randomly created and forced to be not shiny.
        /// </summary>
        Never = 3,

        /// <summary>
        /// PID is randomly created and forced to be shiny as Stars.
        /// </summary>
        AlwaysStar = 5,

        /// <summary>
        /// PID is randomly created and forced to be shiny as Squares.
        /// </summary>
        AlwaysSquare = 6,
    }
}
