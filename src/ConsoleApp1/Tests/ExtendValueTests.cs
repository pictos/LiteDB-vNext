﻿using LiteDB;
using LiteDB.Engine;

var extendValues = new uint[]
{
    0b01100010_11_11_11_11_11_11_11_11_11_11_11_11,
    0b01100010_00_00_00_00_00_00_00_00_00_00_00_00,

    0b01100010_11_11_11_11_11_11_11_11_11_11_11_00,
    0b01100010_11_11_11_11_11_11_11_11_11_11_11_01,
    0b01100010_11_11_11_11_11_11_11_11_11_11_11_10,

    0b01100010_00_11_11_11_11_11_11_11_11_11_11_11,
    0b01100010_01_11_11_11_11_11_11_11_11_11_11_11,
    0b01100010_10_11_11_11_11_11_11_11_11_11_11_11,

    0b01100010_00_11_11_11_11_11_11_11_11_11_11_11,
    0b01100010_11_00_11_11_11_11_11_11_11_11_11_11,
    0b01100010_11_11_00_11_11_11_11_11_11_11_11_11,
    0b01100010_11_11_11_00_11_11_11_11_11_11_11_11,
    0b01100010_11_11_11_11_00_11_11_11_11_11_11_11,
    0b01100010_11_11_11_11_11_00_11_11_11_11_11_11,
    0b01100010_11_11_11_11_11_11_00_11_11_11_11_11,
    0b01100010_11_11_11_11_11_11_11_00_11_11_11_11,
    0b01100010_11_11_11_11_11_11_11_11_00_11_11_11,
    0b01100010_11_11_11_11_11_11_11_11_11_00_11_11,
    0b01100010_11_11_11_11_11_11_11_11_11_11_00_11,
    0b01100010_11_11_11_11_11_11_11_11_11_11_11_00,


};

byte colID = 98;
var pageType = PageType.Data;

foreach (var value in extendValues)
{
    Print(value);
}


void Print(uint value)
{
    var result = AllocationMapPage.HasFreeSpaceInExtend(value, colID, pageType);

    Console.WriteLine(Dump.ExtendValue(value) + " => " + result);
}

