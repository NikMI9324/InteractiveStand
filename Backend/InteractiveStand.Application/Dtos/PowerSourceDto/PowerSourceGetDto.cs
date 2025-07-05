namespace InteractiveStand.Application.Dtos.PowerSourceDto
{
    public class PowerSourceGetDto
    {
        public int Id { get; set; }
        public double NPP_Percentage { get; set; }
        public double HPP_Percentage { get; set; }
        public double CGPP_Percentage { get; set; }
        public double WPP_Percentage { get; set; }
        public double SPP_Percentage { get; set; }
        public double TotalPercentage => NPP_Percentage + HPP_Percentage + CGPP_Percentage + WPP_Percentage + SPP_Percentage;
    }
}
