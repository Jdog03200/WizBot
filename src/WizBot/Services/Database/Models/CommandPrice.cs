﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WizBot.Services.Database.Models
{
    public class CommandPrice : DbEntity
    {
        public int Price { get; set; }
        public string CommandName { get; set; }
    }
}