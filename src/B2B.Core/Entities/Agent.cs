using System.Collections.Generic;

namespace B2B.Core.Entities
{
    public class Agent
    {
        public int AgentID { get; set; }
        public string AgentName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;

        // Navigation
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
