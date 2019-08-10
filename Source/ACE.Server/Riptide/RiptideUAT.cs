namespace ACE.Server.Riptide
{
    public static class RiptideUAT
    {
        public static bool Fix_Point_Blank_Projectiles()
        {
            return CustomPropertiesManager.GetBool("fix_point_blank_missiles").Item;
        }

        public static double Fix_Point_Blank_Projectiles_Factor()
        {
            return CustomPropertiesManager.GetDouble("fix_point_blank_missiles_factor").Item;
        }
    }
}
