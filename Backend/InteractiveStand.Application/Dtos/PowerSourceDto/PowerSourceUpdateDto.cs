namespace InteractiveStand.Application.Dtos.PowerSourceDto
{
    public class PowerSourceUpdateDto
    {
        public int Id { get; set; }
        public double NPP_Capacity { get; set; } 
        public double HPP_Capacity { get; set; } 
        public double CGPP_Capacity { get; set; } 
        public double WPP_Capacity { get; set; } 
        public double SPP_Capacity { get; set; } 
    }
}
