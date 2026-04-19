namespace ARPControl
{
    public class PowerPlanInfo
    {
        public string Guid { get; set; } = "";
        public string Name { get; set; } = "";

        public override string ToString() => Name;
    }
}