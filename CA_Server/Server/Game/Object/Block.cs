﻿using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    public class Block : GameObject
    {
        public Block()
        {
            ObjectType = GameObjectType.Block;
        }
    }
}