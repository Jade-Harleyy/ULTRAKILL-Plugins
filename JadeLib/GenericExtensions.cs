namespace JadeLib
{
    public static class GenericExtensions
    {
        public static string GetRankText(int index, bool rich = false) => index switch
        {
            0 => rich ? "<color=blue>D</color>ESTRUCTIVE" : "DESTRUCTIVE",
            2 => rich ? "<color=green>C</color>HAOTIC" : "CHAOTIC",
            1 => rich ? "<color=yellow>B</color>RUTAL" : "BRUTAL",
            3 => rich ? "<color=orange>A</color>NARCHIC" : "ANARCHIC",
            4 => rich ? "<color=red>S</color>UPREME" : "SUPREME",
            5 => rich ? "<color=red>SS</color>ADISTIC" : "SSADISTIC",
            6 => rich ? "<color=red>SSS</color>HITSTORM" : "SSSHITSTORM",
            7 => rich ? "<color=orange>ULTRAKILL</color>" : "ULTRAKILL",
            _ => rich ? "<color=red><u>UNKNOWN</u></color>" : "UNKNOWN"
        };
    }
}