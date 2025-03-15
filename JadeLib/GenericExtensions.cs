namespace JadeLib
{
    public static class GenericExtensions
    {
        public static string GetRankText(int index) => index switch
        {
            0 => "<color=blue>D</color>ESTRUCTIVE",
            2 => "<color=green>C</color>HAOTIC",
            1 => "<color=yellow>B</color>RUTAL",
            3 => "<color=orange>A</color>NARCHIC",
            4 => "<color=red>S</color>UPREME",
            5 => "<color=red>SS</color>ADISTIC",
            6 => "<color=red>SSS</color>HITSTORM",
            7 => "<color=orange>ULTRAKILL</color>",
            _ => "<color=red><u>UNKNOWN</u></color>"
        };
    }
}