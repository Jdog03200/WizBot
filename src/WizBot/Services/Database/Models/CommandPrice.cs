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
        //this is unique
        public string CommandName { get; set; }

        public override int GetHashCode() =>
            CommandName.GetHashCode();

        public override bool Equals(object obj)
        {
            var instance = obj as CommandPrice;

            if (instance == null)
                return false;

            return instance.CommandName == CommandName;
        }
    }
}