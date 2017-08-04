using CodeEffects.Rule.Attributes;
using ESPL.BRE.Web.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ESPL.BRE.Web.Models
{
    public class Address
    {
        [Parent("Home", "Home Street", "Current home street")]
        [Parent("Work", "Work Street", "Current work street")]
        [Field(Max = 50, StringComparison = StringComparison.InvariantCultureIgnoreCase)]
        public string Street { get; set; }

        [Parent("Home", "Home City", "Current home city")]
        [Parent("Work", "Work City", "Current work city")]
        [Field(Max = 30, StringComparison = StringComparison.InvariantCultureIgnoreCase)]
        public string City { get; set; }

        [Parent("Home", "Home State", "Current home state")]
        [Parent("Work", "Work State", "Current work state")]
        public State State { get; set; }

        [Parent("Home", "Home Zip", "Current home zip code")]
        [Parent("Work", "Work Zip", "Current work zip code")]
        [Field(Max = 5)]
        public string Zip { get; set; }

        public Address() { this.State = State.Unknown; }
    }
}