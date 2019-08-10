namespace ACE.Server.Riptide
{
    public static class RiptideUAT
    {
        public static bool Fix_Point_Blank_Projectiles()
        {
            return CustomPropertiesManager.GetBool("fix_point_blank_missiles").Item;
        }
    }
}
