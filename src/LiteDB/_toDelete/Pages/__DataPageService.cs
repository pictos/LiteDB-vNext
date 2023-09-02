﻿namespace LiteDB.Engine;

[AutoInterface]
internal class __DataPageService : __PageService, I__DataPageService
{
    /// <summary>
    /// Initialize an empty PageBuffer as DataPage
    /// </summary>
    public void InitializeDataPage(PageBuffer page, int pageID, byte colID)
    {
        page.Header.PageID = pageID;
        page.Header.PageType = PageType.Data;
        page.Header.ColID = colID;

        page.IsDirty = true;
    }

    /// <summary>
    /// Write a new document (or document fragment) into a DataPage and returns new __DataBlock
    /// </summary>
    public __DataBlock InsertDataBlock(PageBuffer page, Span<byte> content, bool extend)
    {
        // get required bytes this insert
        var bytesLength = (ushort)(content.Length + __DataBlock.DATA_BLOCK_FIXED_SIZE);

        // get a new index block
        var newIndex = page.Header.GetFreeIndex(page);

        // get page segment for this data block
        var segment = base.Insert(page, bytesLength, newIndex, true);

        // dataBlockID
        var dataBlockID = new PageAddress(page.Header.PageID, newIndex);

        // get datablock buffer segment
        var buffer = page.AsSpan(segment);

        // create new datablock
        var dataBlock = new __DataBlock(buffer, dataBlockID, extend);

        // copy content from span source to data block content area 
        content.CopyTo(buffer[__DataBlock.P_BUFFER..]);

        return dataBlock;
    }

    /// <summary>
    /// Update an existing document inside a single page. This new document must fit on this page
    /// </summary>
    public __DataBlock UpdateDataBlock(PageBuffer page, byte index, Span<byte> content, PageAddress nextBlock)
    {
        // get required bytes this update
        var bytesLength = (ushort)(content.Length + __DataBlock.DATA_BLOCK_FIXED_SIZE);

        page.IsDirty = true;

        // get page segment to update this buffer
        var segment = base.Update(page, index, bytesLength);

        // get datablock buffer segment
        var buffer = page.AsSpan(segment);

        var dataBlockID = new PageAddress(page.Header.PageID, index);

        var dataBlock = new __DataBlock(buffer, dataBlockID);

        dataBlock.SetNextBlockID(buffer, nextBlock);

        // copy content from span source to data block content area 
        content.CopyTo(buffer[__DataBlock.P_BUFFER..]);

        // return updated data block
        return dataBlock;
    }

    /// <summary>
    /// Delete a single datablock from page. Returns NextBlock from deleted data block
    /// </summary>
    public void DeleteDataBlock(PageBuffer page, byte index)
    {
        base.Delete(page, index);
    }
}