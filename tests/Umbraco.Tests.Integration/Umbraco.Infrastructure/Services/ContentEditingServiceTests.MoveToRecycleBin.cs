﻿using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

public partial class ContentEditingServiceTests
{
    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Move_To_Recycle_Bin(bool variant)
    {
        var content = await (variant ? CreateVariantContent() : CreateInvariantContent());
        var result = await ContentEditingService.MoveToRecycleBinAsync(content.Key, Constants.Security.SuperUserKey);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);

        // re-get and verify move
        content = await ContentEditingService.GetAsync(content.Key);
        Assert.IsNotNull(content);
        Assert.IsTrue(content.Trashed);
    }

    [Test]
    public async Task Cannot_Move_Non_Existing_To_Recycle_Bin()
    {
        var result = await ContentEditingService.MoveToRecycleBinAsync(Guid.NewGuid(), Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.NotFound, result.Status);
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Cannot_Move_To_Recycle_Bin_If_Already_In_Recycle_Bin(bool variant)
    {
        var content = await (variant ? CreateVariantContent() : CreateInvariantContent());
        await ContentEditingService.MoveToRecycleBinAsync(content.Key, Constants.Security.SuperUserKey);
        var result = await ContentEditingService.MoveToRecycleBinAsync(content.Key, Constants.Security.SuperUserKey);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.InTrash, result.Status);

        // re-get and verify that it still is in the recycle bin
        content = await ContentEditingService.GetAsync(content.Key);
        Assert.IsNotNull(content);
        Assert.IsTrue(content.Trashed);
    }
}
