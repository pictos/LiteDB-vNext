﻿//using ConsoleApp1.Tests;

//using LiteDB.Engine;

//var extendValues = new uint[]
//{
//    0b01100010_111_111_111_111_111_111_011_000,
//    0b01100010_000_000_000_000_000_000_000_000,

//    0b01100010_111_111_111_111_111_111_111_000,
//    0b01100010_111_111_111_111_111_111_111_001,
//    0b01100010_111_111_111_111_111_111_111_010,
//    0b01100010_111_111_111_111_111_111_111_011,
//    0b01100010_111_111_111_111_111_111_111_100,
//    0b01100010_111_111_111_111_111_111_111_101,
//    0b01100010_111_111_111_111_111_111_111_110,
//    0b01100010_111_111_111_111_111_111_111_111,

//    0b01100010_000_111_111_111_111_111_111_111,
//    0b01100010_001_111_111_111_111_111_111_111,
//    0b01100010_010_111_111_111_111_111_111_111,
//    0b01100010_011_111_111_111_111_111_111_111,
//    0b01100010_100_111_111_111_111_111_111_111,
//    0b01100010_101_111_111_111_111_111_111_111,
//    0b01100010_110_111_111_111_111_111_111_111,
//    0b01100010_111_111_111_111_111_111_111_111,

//    0b01100010_111_001_110_110_000_110_110_110,

//};

//byte colID = 98;
//var pageType = PageType.Data;
//var length = 3500; /* 300,3500,5500,7500,8050*/

//foreach (var value in extendValues)
//{
//    Print(value);
//}


//void Print(uint value)
//{
//    var result = AllocationMapPage.HasFreeSpaceInExtend(value, colID, pageType, length);

//    Console.WriteLine(value.ToBinaryString() + " => " + result);
//}

