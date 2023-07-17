using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Umbraco.Cms.Persistence.EFCore.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /*
             * Uncomment the block below when the NPOCO migrations are removed. This will create the tables in the same way as the NPOCO migrations.
             */

            //migrationBuilder.CreateTable(
            //    name: "cmsDictionary",
            //    columns: table => new
            //    {
            //        pk = table.Column<int>(type: "INTEGER", nullable: false)
            //            .Annotation("Sqlite:Autoincrement", true),
            //        id = table.Column<Guid>(type: "TEXT", nullable: false),
            //        parent = table.Column<Guid>(type: "TEXT", nullable: true),
            //        key = table.Column<string>(type: "TEXT", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_cmsDictionary", x => x.pk);
            //        table.UniqueConstraint("AK_cmsDictionary_id", x => x.id);
            //        table.ForeignKey(
            //            name: "FK_cmsDictionary_cmsDictionary_id",
            //            column: x => x.parent,
            //            principalTable: "cmsDictionary",
            //            principalColumn: "id");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "cmsMacro",
            //    columns: table => new
            //    {
            //        id = table.Column<int>(type: "INTEGER", nullable: false)
            //            .Annotation("Sqlite:Autoincrement", true),
            //        uniqueId = table.Column<Guid>(type: "TEXT", nullable: false),
            //        macroUseInEditor = table.Column<bool>(type: "INTEGER", nullable: false, defaultValueSql: "('0')"),
            //        macroRefreshRate = table.Column<int>(type: "INTEGER", nullable: false, defaultValueSql: "('0')"),
            //        macroAlias = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
            //        macroName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
            //        macroCacheByPage = table.Column<bool>(type: "INTEGER", nullable: false, defaultValueSql: "('1')"),
            //        macroCachePersonalized = table.Column<bool>(type: "INTEGER", nullable: false, defaultValueSql: "('0')"),
            //        macroDontRender = table.Column<bool>(type: "INTEGER", nullable: false, defaultValueSql: "('0')"),
            //        macroSource = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
            //        macroType = table.Column<int>(type: "INTEGER", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_cmsMacro", x => x.id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "umbracoAudit",
            //    columns: table => new
            //    {
            //        id = table.Column<int>(type: "INTEGER", nullable: false)
            //            .Annotation("Sqlite:Autoincrement", true),
            //        performingUserId = table.Column<int>(type: "INTEGER", nullable: false),
            //        performingDetails = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: true),
            //        performingIp = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
            //        eventDateUtc = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
            //        affectedUserId = table.Column<int>(type: "INTEGER", nullable: false),
            //        affectedDetails = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: true),
            //        eventType = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
            //        eventDetails = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_umbracoAudit", x => x.id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "umbracoCacheInstruction",
            //    columns: table => new
            //    {
            //        id = table.Column<int>(type: "INTEGER", nullable: false)
            //            .Annotation("Sqlite:Autoincrement", true),
            //        utcStamp = table.Column<DateTime>(type: "datetime", nullable: false),
            //        jsonInstruction = table.Column<string>(type: "TEXT", nullable: false),
            //        originated = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
            //        instructionCount = table.Column<int>(type: "INTEGER", nullable: false, defaultValueSql: "('1')")
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_umbracoCacheInstruction", x => x.id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "umbracoConsent",
            //    columns: table => new
            //    {
            //        id = table.Column<int>(type: "INTEGER", nullable: false)
            //            .Annotation("Sqlite:Autoincrement", true),
            //        current = table.Column<bool>(type: "INTEGER", nullable: false),
            //        source = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
            //        context = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
            //        action = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
            //        createDate = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
            //        state = table.Column<int>(type: "INTEGER", nullable: false),
            //        comment = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_umbracoConsent", x => x.id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "umbracoCreatedPackageSchema",
            //    columns: table => new
            //    {
            //        id = table.Column<int>(type: "INTEGER", nullable: false)
            //            .Annotation("Sqlite:Autoincrement", true),
            //        name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
            //        value = table.Column<string>(type: "TEXT", nullable: false),
            //        updateDate = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
            //        packageId = table.Column<Guid>(type: "TEXT", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_umbracoCreatedPackageSchema", x => x.id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "umbracoExternalLogin",
            //    columns: table => new
            //    {
            //        id = table.Column<int>(type: "INTEGER", nullable: false)
            //            .Annotation("Sqlite:Autoincrement", true),
            //        userOrMemberKey = table.Column<Guid>(type: "TEXT", nullable: false),
            //        loginProvider = table.Column<string>(type: "TEXT", maxLength: 400, nullable: false),
            //        providerKey = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
            //        createDate = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
            //        userData = table.Column<string>(type: "TEXT", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_umbracoExternalLogin", x => x.id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "umbracoKeyValue",
            //    columns: table => new
            //    {
            //        key = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
            //        value = table.Column<string>(type: "TEXT", nullable: true),
            //        updated = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_umbracoKeyValue", x => x.key);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "umbracoLanguage",
            //    columns: table => new
            //    {
            //        id = table.Column<int>(type: "INTEGER", nullable: false)
            //            .Annotation("Sqlite:Autoincrement", true),
            //        languageISOCode = table.Column<string>(type: "TEXT", maxLength: 14, nullable: true),
            //        languageCultureName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
            //        isDefaultVariantLang = table.Column<bool>(type: "INTEGER", nullable: false, defaultValueSql: "('0')"),
            //        mandatory = table.Column<bool>(type: "INTEGER", nullable: false, defaultValueSql: "('0')"),
            //        fallbackLanguageId = table.Column<int>(type: "INTEGER", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_umbracoLanguage", x => x.id);
            //        table.ForeignKey(
            //            name: "FK_umbracoLanguage_umbracoLanguage_id",
            //            column: x => x.fallbackLanguageId,
            //            principalTable: "umbracoLanguage",
            //            principalColumn: "id");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "umbracoLock",
            //    columns: table => new
            //    {
            //        id = table.Column<int>(type: "INTEGER", nullable: false),
            //        value = table.Column<int>(type: "INTEGER", nullable: false),
            //        name = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_umbracoLock", x => x.id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "umbracoLogViewerQuery",
            //    columns: table => new
            //    {
            //        id = table.Column<int>(type: "INTEGER", nullable: false)
            //            .Annotation("Sqlite:Autoincrement", true),
            //        name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
            //        query = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_umbracoLogViewerQuery", x => x.id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "umbracoRelationType",
            //    columns: table => new
            //    {
            //        id = table.Column<int>(type: "INTEGER", nullable: false)
            //            .Annotation("Sqlite:Autoincrement", true),
            //        typeUniqueId = table.Column<Guid>(type: "TEXT", nullable: false),
            //        dual = table.Column<bool>(type: "INTEGER", nullable: false),
            //        parentObjectType = table.Column<Guid>(type: "TEXT", nullable: true),
            //        childObjectType = table.Column<Guid>(type: "TEXT", nullable: true),
            //        name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
            //        alias = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
            //        isDependency = table.Column<bool>(type: "INTEGER", nullable: false, defaultValueSql: "('0')")
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_umbracoRelationType", x => x.id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "umbracoServer",
            //    columns: table => new
            //    {
            //        id = table.Column<int>(type: "INTEGER", nullable: false)
            //            .Annotation("Sqlite:Autoincrement", true),
            //        address = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
            //        computerName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
            //        registeredDate = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
            //        lastNotifiedDate = table.Column<DateTime>(type: "datetime", nullable: false),
            //        isActive = table.Column<bool>(type: "INTEGER", nullable: false),
            //        isSchedulingPublisher = table.Column<bool>(type: "INTEGER", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_umbracoServer", x => x.id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "umbracoTwoFactorLogin",
            //    columns: table => new
            //    {
            //        id = table.Column<int>(type: "INTEGER", nullable: false)
            //            .Annotation("Sqlite:Autoincrement", true),
            //        userOrMemberKey = table.Column<Guid>(type: "TEXT", nullable: false),
            //        providerName = table.Column<string>(type: "TEXT", maxLength: 400, nullable: false),
            //        secret = table.Column<string>(type: "TEXT", maxLength: 400, nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_umbracoTwoFactorLogin", x => x.id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "umbracoUser",
            //    columns: table => new
            //    {
            //        id = table.Column<int>(type: "INTEGER", nullable: false)
            //            .Annotation("Sqlite:Autoincrement", true),
            //        userDisabled = table.Column<bool>(type: "INTEGER", nullable: false, defaultValueSql: "('0')"),
            //        userNoConsole = table.Column<bool>(type: "INTEGER", nullable: false, defaultValueSql: "('0')"),
            //        userName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
            //        userLogin = table.Column<string>(type: "TEXT", maxLength: 125, nullable: false),
            //        userPassword = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
            //        passwordConfig = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
            //        userEmail = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
            //        userLanguage = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
            //        securityStampToken = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
            //        failedLoginAttempts = table.Column<int>(type: "INTEGER", nullable: true),
            //        lastLockoutDate = table.Column<DateTime>(type: "datetime", nullable: true),
            //        lastPasswordChangeDate = table.Column<DateTime>(type: "datetime", nullable: true),
            //        lastLoginDate = table.Column<DateTime>(type: "datetime", nullable: true),
            //        emailConfirmedDate = table.Column<DateTime>(type: "datetime", nullable: true),
            //        invitedDate = table.Column<DateTime>(type: "datetime", nullable: true),
            //        createDate = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
            //        updateDate = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
            //        avatar = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
            //        tourData = table.Column<string>(type: "TEXT", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_user", x => x.id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "cmsMacroProperty",
            //    columns: table => new
            //    {
            //        id = table.Column<int>(type: "INTEGER", nullable: false)
            //            .Annotation("Sqlite:Autoincrement", true),
            //        uniquePropertyId = table.Column<Guid>(type: "TEXT", nullable: false),
            //        editorAlias = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
            //        macro = table.Column<int>(type: "INTEGER", nullable: false),
            //        macroPropertySortOrder = table.Column<int>(type: "INTEGER", nullable: false, defaultValueSql: "('0')"),
            //        macroPropertyAlias = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
            //        macroPropertyName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_cmsMacroProperty", x => x.id);
            //        table.ForeignKey(
            //            name: "FK_cmsMacroProperty_cmsMacro_id",
            //            column: x => x.macro,
            //            principalTable: "cmsMacro",
            //            principalColumn: "id");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "umbracoExternalLoginToken",
            //    columns: table => new
            //    {
            //        id = table.Column<int>(type: "INTEGER", nullable: false)
            //            .Annotation("Sqlite:Autoincrement", true),
            //        externalLoginId = table.Column<int>(type: "INTEGER", nullable: false),
            //        name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
            //        value = table.Column<string>(type: "TEXT", nullable: false),
            //        createDate = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_umbracoExternalLoginToken", x => x.id);
            //        table.ForeignKey(
            //            name: "FK_umbracoExternalLoginToken_umbracoExternalLogin_id",
            //            column: x => x.externalLoginId,
            //            principalTable: "umbracoExternalLogin",
            //            principalColumn: "id");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "cmsLanguageText",
            //    columns: table => new
            //    {
            //        pk = table.Column<int>(type: "INTEGER", nullable: false)
            //            .Annotation("Sqlite:Autoincrement", true),
            //        languageId = table.Column<int>(type: "INTEGER", nullable: false),
            //        UniqueId = table.Column<Guid>(type: "TEXT", nullable: false),
            //        value = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_cmsLanguageText", x => x.pk);
            //        table.ForeignKey(
            //            name: "FK_cmsLanguageText_cmsDictionary_id",
            //            column: x => x.UniqueId,
            //            principalTable: "cmsDictionary",
            //            principalColumn: "id");
            //        table.ForeignKey(
            //            name: "FK_cmsLanguageText_umbracoLanguage_id",
            //            column: x => x.languageId,
            //            principalTable: "umbracoLanguage",
            //            principalColumn: "id");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "cmsTags",
            //    columns: table => new
            //    {
            //        id = table.Column<int>(type: "INTEGER", nullable: false)
            //            .Annotation("Sqlite:Autoincrement", true),
            //        group = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
            //        languageId = table.Column<int>(type: "INTEGER", nullable: true),
            //        tag = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_cmsTags", x => x.id);
            //        table.ForeignKey(
            //            name: "FK_cmsTags_umbracoLanguage_id",
            //            column: x => x.languageId,
            //            principalTable: "umbracoLanguage",
            //            principalColumn: "id");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "umbracoLog",
            //    columns: table => new
            //    {
            //        id = table.Column<int>(type: "INTEGER", nullable: false)
            //            .Annotation("Sqlite:Autoincrement", true),
            //        userId = table.Column<int>(type: "INTEGER", nullable: true),
            //        NodeId = table.Column<int>(type: "INTEGER", nullable: false),
            //        entityType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
            //        Datestamp = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
            //        logHeader = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
            //        logComment = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true),
            //        parameters = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_umbracoLog", x => x.id);
            //        table.ForeignKey(
            //            name: "FK_umbracoLog_umbracoUser_id",
            //            column: x => x.userId,
            //            principalTable: "umbracoUser",
            //            principalColumn: "id");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "umbracoNode",
            //    columns: table => new
            //    {
            //        id = table.Column<int>(type: "INTEGER", nullable: false)
            //            .Annotation("Sqlite:Autoincrement", true),
            //        uniqueId = table.Column<Guid>(type: "TEXT", nullable: false, defaultValueSql: "(newid())"),
            //        parentId = table.Column<int>(type: "INTEGER", nullable: false),
            //        level = table.Column<int>(type: "INTEGER", nullable: false),
            //        path = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
            //        sortOrder = table.Column<int>(type: "INTEGER", nullable: false),
            //        trashed = table.Column<bool>(type: "INTEGER", nullable: false, defaultValueSql: "('0')"),
            //        nodeUser = table.Column<int>(type: "INTEGER", nullable: true),
            //        text = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
            //        nodeObjectType = table.Column<Guid>(type: "TEXT", nullable: true),
            //        createDate = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_umbracoNode", x => x.id);
            //        table.UniqueConstraint("AK_umbracoNode_uniqueId", x => x.uniqueId);
            //        table.ForeignKey(
            //            name: "FK_umbracoNode_umbracoNode_id",
            //            column: x => x.parentId,
            //            principalTable: "umbracoNode",
            //            principalColumn: "id");
            //        table.ForeignKey(
            //            name: "FK_umbracoNode_umbracoUser_id",
            //            column: x => x.nodeUser,
            //            principalTable: "umbracoUser",
            //            principalColumn: "id");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "umbracoUserLogin",
            //    columns: table => new
            //    {
            //        sessionId = table.Column<Guid>(type: "TEXT", nullable: false),
            //        userId = table.Column<int>(type: "INTEGER", nullable: false),
            //        loggedInUtc = table.Column<DateTime>(type: "datetime", nullable: false),
            //        lastValidatedUtc = table.Column<DateTime>(type: "datetime", nullable: false),
            //        loggedOutUtc = table.Column<DateTime>(type: "datetime", nullable: true),
            //        ipAddress = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_umbracoUserLogin", x => x.sessionId);
            //        table.ForeignKey(
            //            name: "FK_umbracoUserLogin_umbracoUser_id",
            //            column: x => x.userId,
            //            principalTable: "umbracoUser",
            //            principalColumn: "id");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "cmsContentType",
            //    columns: table => new
            //    {
            //        pk = table.Column<int>(type: "INTEGER", nullable: false)
            //            .Annotation("Sqlite:Autoincrement", true),
            //        nodeId = table.Column<int>(type: "INTEGER", nullable: false),
            //        alias = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
            //        icon = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
            //        thumbnail = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false, defaultValueSql: "('folder.png')"),
            //        description = table.Column<string>(type: "TEXT", maxLength: 1500, nullable: true),
            //        isContainer = table.Column<bool>(type: "INTEGER", nullable: false, defaultValueSql: "('0')"),
            //        isElement = table.Column<bool>(type: "INTEGER", nullable: false, defaultValueSql: "('0')"),
            //        allowAtRoot = table.Column<bool>(type: "INTEGER", nullable: false, defaultValueSql: "('0')"),
            //        variations = table.Column<int>(type: "INTEGER", nullable: false, defaultValueSql: "('1')")
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_cmsContentType", x => x.pk);
            //        table.UniqueConstraint("AK_cmsContentType_nodeId", x => x.nodeId);
            //        table.ForeignKey(
            //            name: "FK_cmsContentType_umbracoNode_id",
            //            column: x => x.nodeId,
            //            principalTable: "umbracoNode",
            //            principalColumn: "id");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "cmsContentType2ContentType",
            //    columns: table => new
            //    {
            //        parentContentTypeId = table.Column<int>(type: "INTEGER", nullable: false),
            //        childContentTypeId = table.Column<int>(type: "INTEGER", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_cmsContentType2ContentType", x => new { x.parentContentTypeId, x.childContentTypeId });
            //        table.ForeignKey(
            //            name: "FK_cmsContentType2ContentType_umbracoNode_child",
            //            column: x => x.childContentTypeId,
            //            principalTable: "umbracoNode",
            //            principalColumn: "id");
            //        table.ForeignKey(
            //            name: "FK_cmsContentType2ContentType_umbracoNode_parent",
            //            column: x => x.parentContentTypeId,
            //            principalTable: "umbracoNode",
            //            principalColumn: "id");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "cmsTemplate",
            //    columns: table => new
            //    {
            //        pk = table.Column<int>(type: "INTEGER", nullable: false)
            //            .Annotation("Sqlite:Autoincrement", true),
            //        nodeId = table.Column<int>(type: "INTEGER", nullable: false),
            //        alias = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_cmsTemplate", x => x.pk);
            //        table.UniqueConstraint("AK_cmsTemplate_nodeId", x => x.nodeId);
            //        table.ForeignKey(
            //            name: "FK_cmsTemplate_umbracoNode",
            //            column: x => x.nodeId,
            //            principalTable: "umbracoNode",
            //            principalColumn: "id");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "umbracoAccess",
            //    columns: table => new
            //    {
            //        id = table.Column<Guid>(type: "TEXT", nullable: false),
            //        nodeId = table.Column<int>(type: "INTEGER", nullable: false),
            //        loginNodeId = table.Column<int>(type: "INTEGER", nullable: false),
            //        noAccessNodeId = table.Column<int>(type: "INTEGER", nullable: false),
            //        createDate = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
            //        updateDate = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_umbracoAccess", x => x.id);
            //        table.ForeignKey(
            //            name: "FK_umbracoAccess_umbracoNode_id",
            //            column: x => x.nodeId,
            //            principalTable: "umbracoNode",
            //            principalColumn: "id");
            //        table.ForeignKey(
            //            name: "FK_umbracoAccess_umbracoNode_id1",
            //            column: x => x.loginNodeId,
            //            principalTable: "umbracoNode",
            //            principalColumn: "id");
            //        table.ForeignKey(
            //            name: "FK_umbracoAccess_umbracoNode_id2",
            //            column: x => x.noAccessNodeId,
            //            principalTable: "umbracoNode",
            //            principalColumn: "id");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "umbracoDataType",
            //    columns: table => new
            //    {
            //        nodeId = table.Column<int>(type: "INTEGER", nullable: false),
            //        propertyEditorAlias = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
            //        dbType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
            //        config = table.Column<string>(type: "TEXT", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_umbracoDataType", x => x.nodeId);
            //        table.ForeignKey(
            //            name: "FK_umbracoDataType_umbracoNode_id",
            //            column: x => x.nodeId,
            //            principalTable: "umbracoNode",
            //            principalColumn: "id");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "umbracoDocumentCultureVariation",
            //    columns: table => new
            //    {
            //        id = table.Column<int>(type: "INTEGER", nullable: false)
            //            .Annotation("Sqlite:Autoincrement", true),
            //        nodeId = table.Column<int>(type: "INTEGER", nullable: false),
            //        languageId = table.Column<int>(type: "INTEGER", nullable: false),
            //        edited = table.Column<bool>(type: "INTEGER", nullable: false),
            //        available = table.Column<bool>(type: "INTEGER", nullable: false),
            //        published = table.Column<bool>(type: "INTEGER", nullable: false),
            //        name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_umbracoDocumentCultureVariation", x => x.id);
            //        table.ForeignKey(
            //            name: "FK_umbracoDocumentCultureVariation_umbracoLanguage_id",
            //            column: x => x.languageId,
            //            principalTable: "umbracoLanguage",
            //            principalColumn: "id");
            //        table.ForeignKey(
            //            name: "FK_umbracoDocumentCultureVariation_umbracoNode_id",
            //            column: x => x.nodeId,
            //            principalTable: "umbracoNode",
            //            principalColumn: "id");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "umbracoDomain",
            //    columns: table => new
            //    {
            //        id = table.Column<int>(type: "INTEGER", nullable: false)
            //            .Annotation("Sqlite:Autoincrement", true),
            //        domainDefaultLanguage = table.Column<int>(type: "INTEGER", nullable: true),
            //        domainRootStructureID = table.Column<int>(type: "INTEGER", nullable: true),
            //        domainName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
            //        sortOrder = table.Column<int>(type: "INTEGER", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_umbracoDomain", x => x.id);
            //        table.ForeignKey(
            //            name: "FK_umbracoDomain_umbracoNode_id",
            //            column: x => x.domainRootStructureID,
            //            principalTable: "umbracoNode",
            //            principalColumn: "id");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "umbracoRedirectUrl",
            //    columns: table => new
            //    {
            //        id = table.Column<Guid>(type: "TEXT", nullable: false),
            //        contentKey = table.Column<Guid>(type: "TEXT", nullable: false),
            //        createDateUtc = table.Column<DateTime>(type: "datetime", nullable: false),
            //        url = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
            //        culture = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
            //        urlHash = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_umbracoRedirectUrl", x => x.id);
            //        table.ForeignKey(
            //            name: "FK_umbracoRedirectUrl_umbracoNode_uniqueID",
            //            column: x => x.contentKey,
            //            principalTable: "umbracoNode",
            //            principalColumn: "uniqueId");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "umbracoRelation",
            //    columns: table => new
            //    {
            //        id = table.Column<int>(type: "INTEGER", nullable: false)
            //            .Annotation("Sqlite:Autoincrement", true),
            //        parentId = table.Column<int>(type: "INTEGER", nullable: false),
            //        childId = table.Column<int>(type: "INTEGER", nullable: false),
            //        relType = table.Column<int>(type: "INTEGER", nullable: false),
            //        datetime = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
            //        comment = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_umbracoRelation", x => x.id);
            //        table.ForeignKey(
            //            name: "FK_umbracoRelation_umbracoNode",
            //            column: x => x.parentId,
            //            principalTable: "umbracoNode",
            //            principalColumn: "id");
            //        table.ForeignKey(
            //            name: "FK_umbracoRelation_umbracoNode1",
            //            column: x => x.childId,
            //            principalTable: "umbracoNode",
            //            principalColumn: "id");
            //        table.ForeignKey(
            //            name: "FK_umbracoRelation_umbracoRelationType_id",
            //            column: x => x.relType,
            //            principalTable: "umbracoRelationType",
            //            principalColumn: "id");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "umbracoUser2NodeNotify",
            //    columns: table => new
            //    {
            //        userId = table.Column<int>(type: "INTEGER", nullable: false),
            //        nodeId = table.Column<int>(type: "INTEGER", nullable: false),
            //        action = table.Column<string>(type: "TEXT", fixedLength: true, maxLength: 1, nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_umbracoUser2NodeNotify", x => new { x.userId, x.nodeId, x.action });
            //        table.ForeignKey(
            //            name: "FK_umbracoUser2NodeNotify_umbracoNode_id",
            //            column: x => x.nodeId,
            //            principalTable: "umbracoNode",
            //            principalColumn: "id");
            //        table.ForeignKey(
            //            name: "FK_umbracoUser2NodeNotify_umbracoUser_id",
            //            column: x => x.userId,
            //            principalTable: "umbracoUser",
            //            principalColumn: "id");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "umbracoUserGroup",
            //    columns: table => new
            //    {
            //        id = table.Column<int>(type: "INTEGER", nullable: false)
            //            .Annotation("Sqlite:Autoincrement", true),
            //        userGroupAlias = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
            //        userGroupName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
            //        userGroupDefaultPermissions = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
            //        createDate = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
            //        updateDate = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
            //        icon = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
            //        hasAccessToAllLanguages = table.Column<bool>(type: "INTEGER", nullable: false),
            //        startContentId = table.Column<int>(type: "INTEGER", nullable: true),
            //        startMediaId = table.Column<int>(type: "INTEGER", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_umbracoUserGroup", x => x.id);
            //        table.ForeignKey(
            //            name: "FK_startContentId_umbracoNode_id",
            //            column: x => x.startContentId,
            //            principalTable: "umbracoNode",
            //            principalColumn: "id");
            //        table.ForeignKey(
            //            name: "FK_startMediaId_umbracoNode_id",
            //            column: x => x.startMediaId,
            //            principalTable: "umbracoNode",
            //            principalColumn: "id");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "umbracoUserStartNode",
            //    columns: table => new
            //    {
            //        id = table.Column<int>(type: "INTEGER", nullable: false)
            //            .Annotation("Sqlite:Autoincrement", true),
            //        userId = table.Column<int>(type: "INTEGER", nullable: false),
            //        startNode = table.Column<int>(type: "INTEGER", nullable: false),
            //        startNodeType = table.Column<int>(type: "INTEGER", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_userStartNode", x => x.id);
            //        table.ForeignKey(
            //            name: "FK_umbracoUserStartNode_umbracoNode_id",
            //            column: x => x.startNode,
            //            principalTable: "umbracoNode",
            //            principalColumn: "id");
            //        table.ForeignKey(
            //            name: "FK_umbracoUserStartNode_umbracoUser_id",
            //            column: x => x.userId,
            //            principalTable: "umbracoUser",
            //            principalColumn: "id");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "cmsContentTypeAllowedContentType",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "INTEGER", nullable: false),
            //        AllowedId = table.Column<int>(type: "INTEGER", nullable: false),
            //        SortOrder = table.Column<int>(type: "INTEGER", nullable: false, defaultValueSql: "('0')")
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_cmsContentTypeAllowedContentType", x => new { x.Id, x.AllowedId });
            //        table.ForeignKey(
            //            name: "FK_cmsContentTypeAllowedContentType_cmsContentType",
            //            column: x => x.Id,
            //            principalTable: "cmsContentType",
            //            principalColumn: "nodeId");
            //        table.ForeignKey(
            //            name: "FK_cmsContentTypeAllowedContentType_cmsContentType1",
            //            column: x => x.AllowedId,
            //            principalTable: "cmsContentType",
            //            principalColumn: "nodeId");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "cmsMemberType",
            //    columns: table => new
            //    {
            //        pk = table.Column<int>(type: "INTEGER", nullable: false)
            //            .Annotation("Sqlite:Autoincrement", true),
            //        NodeId = table.Column<int>(type: "INTEGER", nullable: false),
            //        propertytypeId = table.Column<int>(type: "INTEGER", nullable: false),
            //        memberCanEdit = table.Column<bool>(type: "INTEGER", nullable: false, defaultValueSql: "('0')"),
            //        viewOnProfile = table.Column<bool>(type: "INTEGER", nullable: false, defaultValueSql: "('0')"),
            //        isSensitive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValueSql: "('0')")
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_cmsMemberType", x => x.pk);
            //        table.ForeignKey(
            //            name: "FK_cmsMemberType_cmsContentType_nodeId",
            //            column: x => x.NodeId,
            //            principalTable: "cmsContentType",
            //            principalColumn: "nodeId");
            //        table.ForeignKey(
            //            name: "FK_cmsMemberType_umbracoNode_id",
            //            column: x => x.NodeId,
            //            principalTable: "umbracoNode",
            //            principalColumn: "id");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "cmsPropertyTypeGroup",
            //    columns: table => new
            //    {
            //        id = table.Column<int>(type: "INTEGER", nullable: false)
            //            .Annotation("Sqlite:Autoincrement", true),
            //        uniqueID = table.Column<Guid>(type: "TEXT", nullable: false, defaultValueSql: "(newid())"),
            //        contenttypeNodeId = table.Column<int>(type: "INTEGER", nullable: false),
            //        type = table.Column<int>(type: "INTEGER", nullable: false, defaultValueSql: "('0')"),
            //        text = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
            //        alias = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
            //        sortorder = table.Column<int>(type: "INTEGER", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_cmsPropertyTypeGroup", x => x.id);
            //        table.ForeignKey(
            //            name: "FK_cmsPropertyTypeGroup_cmsContentType_nodeId",
            //            column: x => x.contenttypeNodeId,
            //            principalTable: "cmsContentType",
            //            principalColumn: "nodeId");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "umbracoContent",
            //    columns: table => new
            //    {
            //        nodeId = table.Column<int>(type: "INTEGER", nullable: false),
            //        contentTypeId = table.Column<int>(type: "INTEGER", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_umbracoContent", x => x.nodeId);
            //        table.ForeignKey(
            //            name: "FK_umbracoContent_cmsContentType_NodeId",
            //            column: x => x.contentTypeId,
            //            principalTable: "cmsContentType",
            //            principalColumn: "nodeId");
            //        table.ForeignKey(
            //            name: "FK_umbracoContent_umbracoNode_id",
            //            column: x => x.nodeId,
            //            principalTable: "umbracoNode",
            //            principalColumn: "id");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "umbracoContentVersionCleanupPolicy",
            //    columns: table => new
            //    {
            //        contentTypeId = table.Column<int>(type: "INTEGER", nullable: false),
            //        preventCleanup = table.Column<bool>(type: "INTEGER", nullable: false),
            //        keepAllVersionsNewerThanDays = table.Column<int>(type: "INTEGER", nullable: true),
            //        keepLatestVersionPerDayForDays = table.Column<int>(type: "INTEGER", nullable: true),
            //        updated = table.Column<DateTime>(type: "datetime", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_umbracoContentVersionCleanupPolicy", x => x.contentTypeId);
            //        table.ForeignKey(
            //            name: "FK_umbracoContentVersionCleanupPolicy_cmsContentType_nodeId",
            //            column: x => x.contentTypeId,
            //            principalTable: "cmsContentType",
            //            principalColumn: "nodeId");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "cmsDocumentType",
            //    columns: table => new
            //    {
            //        contentTypeNodeId = table.Column<int>(type: "INTEGER", nullable: false),
            //        templateNodeId = table.Column<int>(type: "INTEGER", nullable: false),
            //        IsDefault = table.Column<bool>(type: "INTEGER", nullable: false, defaultValueSql: "('0')")
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_cmsDocumentType", x => new { x.contentTypeNodeId, x.templateNodeId });
            //        table.ForeignKey(
            //            name: "FK_cmsDocumentType_cmsContentType_nodeId",
            //            column: x => x.contentTypeNodeId,
            //            principalTable: "cmsContentType",
            //            principalColumn: "nodeId");
            //        table.ForeignKey(
            //            name: "FK_cmsDocumentType_cmsTemplate_nodeId",
            //            column: x => x.templateNodeId,
            //            principalTable: "cmsTemplate",
            //            principalColumn: "nodeId");
            //        table.ForeignKey(
            //            name: "FK_cmsDocumentType_umbracoNode_id",
            //            column: x => x.contentTypeNodeId,
            //            principalTable: "umbracoNode",
            //            principalColumn: "id");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "umbracoAccessRule",
            //    columns: table => new
            //    {
            //        id = table.Column<Guid>(type: "TEXT", nullable: false),
            //        accessId = table.Column<Guid>(type: "TEXT", nullable: false),
            //        ruleValue = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
            //        ruleType = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
            //        createDate = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
            //        updateDate = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_umbracoAccessRule", x => x.id);
            //        table.ForeignKey(
            //            name: "FK_umbracoAccessRule_umbracoAccess_id",
            //            column: x => x.accessId,
            //            principalTable: "umbracoAccess",
            //            principalColumn: "id");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "umbracoUser2UserGroup",
            //    columns: table => new
            //    {
            //        userId = table.Column<int>(type: "INTEGER", nullable: false),
            //        userGroupId = table.Column<int>(type: "INTEGER", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_user2userGroup", x => new { x.userId, x.userGroupId });
            //        table.ForeignKey(
            //            name: "FK_umbracoUser2UserGroup_umbracoUserGroup_id",
            //            column: x => x.userGroupId,
            //            principalTable: "umbracoUserGroup",
            //            principalColumn: "id");
            //        table.ForeignKey(
            //            name: "FK_umbracoUser2UserGroup_umbracoUser_id",
            //            column: x => x.userId,
            //            principalTable: "umbracoUser",
            //            principalColumn: "id");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "umbracoUserGroup2App",
            //    columns: table => new
            //    {
            //        userGroupId = table.Column<int>(type: "INTEGER", nullable: false),
            //        app = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_userGroup2App", x => new { x.userGroupId, x.app });
            //        table.ForeignKey(
            //            name: "FK_umbracoUserGroup2App_umbracoUserGroup_id",
            //            column: x => x.userGroupId,
            //            principalTable: "umbracoUserGroup",
            //            principalColumn: "id");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "umbracoUserGroup2Language",
            //    columns: table => new
            //    {
            //        userGroupId = table.Column<int>(type: "INTEGER", nullable: false),
            //        languageId = table.Column<int>(type: "INTEGER", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_userGroup2language", x => new { x.userGroupId, x.languageId });
            //        table.ForeignKey(
            //            name: "FK_umbracoUserGroup2Language_umbracoLanguage_id",
            //            column: x => x.languageId,
            //            principalTable: "umbracoLanguage",
            //            principalColumn: "id",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_umbracoUserGroup2Language_umbracoUserGroup_id",
            //            column: x => x.userGroupId,
            //            principalTable: "umbracoUserGroup",
            //            principalColumn: "id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "umbracoUserGroup2Node",
            //    columns: table => new
            //    {
            //        userGroupId = table.Column<int>(type: "INTEGER", nullable: false),
            //        nodeId = table.Column<int>(type: "INTEGER", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_umbracoUserGroup2Node", x => new { x.userGroupId, x.nodeId });
            //        table.ForeignKey(
            //            name: "FK_umbracoUserGroup2Node_umbracoNode_id",
            //            column: x => x.nodeId,
            //            principalTable: "umbracoNode",
            //            principalColumn: "id");
            //        table.ForeignKey(
            //            name: "FK_umbracoUserGroup2Node_umbracoUserGroup_id",
            //            column: x => x.userGroupId,
            //            principalTable: "umbracoUserGroup",
            //            principalColumn: "id");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "umbracoUserGroup2NodePermission",
            //    columns: table => new
            //    {
            //        userGroupId = table.Column<int>(type: "INTEGER", nullable: false),
            //        nodeId = table.Column<int>(type: "INTEGER", nullable: false),
            //        permission = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_umbracoUserGroup2NodePermission", x => new { x.userGroupId, x.nodeId, x.permission });
            //        table.ForeignKey(
            //            name: "FK_umbracoUserGroup2NodePermission_umbracoNode_id",
            //            column: x => x.nodeId,
            //            principalTable: "umbracoNode",
            //            principalColumn: "id");
            //        table.ForeignKey(
            //            name: "FK_umbracoUserGroup2NodePermission_umbracoUserGroup_id",
            //            column: x => x.userGroupId,
            //            principalTable: "umbracoUserGroup",
            //            principalColumn: "id");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "cmsPropertyType",
            //    columns: table => new
            //    {
            //        id = table.Column<int>(type: "INTEGER", nullable: false)
            //            .Annotation("Sqlite:Autoincrement", true),
            //        dataTypeId = table.Column<int>(type: "INTEGER", nullable: false),
            //        contentTypeId = table.Column<int>(type: "INTEGER", nullable: false),
            //        propertyTypeGroupId = table.Column<int>(type: "INTEGER", nullable: true),
            //        Alias = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
            //        Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
            //        sortOrder = table.Column<int>(type: "INTEGER", nullable: false, defaultValueSql: "('0')"),
            //        mandatory = table.Column<bool>(type: "INTEGER", nullable: false, defaultValueSql: "('0')"),
            //        mandatoryMessage = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
            //        validationRegExp = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
            //        validationRegExpMessage = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
            //        Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
            //        labelOnTop = table.Column<bool>(type: "INTEGER", nullable: false, defaultValueSql: "('0')"),
            //        variations = table.Column<int>(type: "INTEGER", nullable: false, defaultValueSql: "('1')"),
            //        UniqueID = table.Column<Guid>(type: "TEXT", nullable: false, defaultValueSql: "(newid())")
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_cmsPropertyType", x => x.id);
            //        table.ForeignKey(
            //            name: "FK_cmsPropertyType_cmsContentType_nodeId",
            //            column: x => x.contentTypeId,
            //            principalTable: "cmsContentType",
            //            principalColumn: "nodeId");
            //        table.ForeignKey(
            //            name: "FK_cmsPropertyType_cmsPropertyTypeGroup_id",
            //            column: x => x.propertyTypeGroupId,
            //            principalTable: "cmsPropertyTypeGroup",
            //            principalColumn: "id");
            //        table.ForeignKey(
            //            name: "FK_cmsPropertyType_umbracoDataType_nodeId",
            //            column: x => x.dataTypeId,
            //            principalTable: "umbracoDataType",
            //            principalColumn: "nodeId");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "cmsContentNu",
            //    columns: table => new
            //    {
            //        nodeId = table.Column<int>(type: "INTEGER", nullable: false),
            //        published = table.Column<bool>(type: "INTEGER", nullable: false),
            //        data = table.Column<string>(type: "TEXT", nullable: true),
            //        rv = table.Column<long>(type: "INTEGER", nullable: false),
            //        dataRaw = table.Column<byte[]>(type: "BLOB", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_cmsContentNu", x => new { x.nodeId, x.published });
            //        table.ForeignKey(
            //            name: "FK_cmsContentNu_umbracoContent_nodeId",
            //            column: x => x.nodeId,
            //            principalTable: "umbracoContent",
            //            principalColumn: "nodeId",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "cmsMember",
            //    columns: table => new
            //    {
            //        nodeId = table.Column<int>(type: "INTEGER", nullable: false),
            //        Email = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false, defaultValueSql: "('''')"),
            //        LoginName = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false, defaultValueSql: "('''')"),
            //        Password = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false, defaultValueSql: "('''')"),
            //        passwordConfig = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
            //        securityStampToken = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
            //        emailConfirmedDate = table.Column<DateTime>(type: "datetime", nullable: true),
            //        failedPasswordAttempts = table.Column<int>(type: "INTEGER", nullable: true),
            //        isLockedOut = table.Column<bool>(type: "INTEGER", nullable: true, defaultValueSql: "('0')"),
            //        isApproved = table.Column<bool>(type: "INTEGER", nullable: false, defaultValueSql: "('1')"),
            //        lastLoginDate = table.Column<DateTime>(type: "datetime", nullable: true),
            //        lastLockoutDate = table.Column<DateTime>(type: "datetime", nullable: true),
            //        lastPasswordChangeDate = table.Column<DateTime>(type: "datetime", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_cmsMember", x => x.nodeId);
            //        table.ForeignKey(
            //            name: "FK_cmsMember_umbracoContent_nodeId",
            //            column: x => x.nodeId,
            //            principalTable: "umbracoContent",
            //            principalColumn: "nodeId");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "umbracoContentSchedule",
            //    columns: table => new
            //    {
            //        id = table.Column<Guid>(type: "TEXT", nullable: false),
            //        nodeId = table.Column<int>(type: "INTEGER", nullable: false),
            //        languageId = table.Column<int>(type: "INTEGER", nullable: true),
            //        date = table.Column<DateTime>(type: "datetime", nullable: false),
            //        action = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_umbracoContentSchedule", x => x.id);
            //        table.ForeignKey(
            //            name: "FK_umbracoContentSchedule_umbracoContent_nodeId",
            //            column: x => x.nodeId,
            //            principalTable: "umbracoContent",
            //            principalColumn: "nodeId");
            //        table.ForeignKey(
            //            name: "FK_umbracoContentSchedule_umbracoLanguage_id",
            //            column: x => x.languageId,
            //            principalTable: "umbracoLanguage",
            //            principalColumn: "id");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "umbracoContentVersion",
            //    columns: table => new
            //    {
            //        id = table.Column<int>(type: "INTEGER", nullable: false)
            //            .Annotation("Sqlite:Autoincrement", true),
            //        nodeId = table.Column<int>(type: "INTEGER", nullable: false),
            //        versionDate = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
            //        userId = table.Column<int>(type: "INTEGER", nullable: true),
            //        current = table.Column<bool>(type: "INTEGER", nullable: false),
            //        text = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
            //        preventCleanup = table.Column<bool>(type: "INTEGER", nullable: false, defaultValueSql: "('0')")
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_umbracoContentVersion", x => x.id);
            //        table.ForeignKey(
            //            name: "FK_umbracoContentVersion_umbracoContent_nodeId",
            //            column: x => x.nodeId,
            //            principalTable: "umbracoContent",
            //            principalColumn: "nodeId");
            //        table.ForeignKey(
            //            name: "FK_umbracoContentVersion_umbracoUser_id",
            //            column: x => x.userId,
            //            principalTable: "umbracoUser",
            //            principalColumn: "id");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "umbracoDocument",
            //    columns: table => new
            //    {
            //        nodeId = table.Column<int>(type: "INTEGER", nullable: false),
            //        published = table.Column<bool>(type: "INTEGER", nullable: false),
            //        edited = table.Column<bool>(type: "INTEGER", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_umbracoDocument", x => x.nodeId);
            //        table.ForeignKey(
            //            name: "FK_umbracoDocument_umbracoContent_nodeId",
            //            column: x => x.nodeId,
            //            principalTable: "umbracoContent",
            //            principalColumn: "nodeId");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "cmsTagRelationship",
            //    columns: table => new
            //    {
            //        nodeId = table.Column<int>(type: "INTEGER", nullable: false),
            //        tagId = table.Column<int>(type: "INTEGER", nullable: false),
            //        propertyTypeId = table.Column<int>(type: "INTEGER", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_cmsTagRelationship", x => new { x.nodeId, x.propertyTypeId, x.tagId });
            //        table.ForeignKey(
            //            name: "FK_cmsTagRelationship_cmsContent",
            //            column: x => x.nodeId,
            //            principalTable: "umbracoContent",
            //            principalColumn: "nodeId");
            //        table.ForeignKey(
            //            name: "FK_cmsTagRelationship_cmsPropertyType",
            //            column: x => x.propertyTypeId,
            //            principalTable: "cmsPropertyType",
            //            principalColumn: "id");
            //        table.ForeignKey(
            //            name: "FK_cmsTagRelationship_cmsTags_id",
            //            column: x => x.tagId,
            //            principalTable: "cmsTags",
            //            principalColumn: "id");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "cmsMember2MemberGroup",
            //    columns: table => new
            //    {
            //        Member = table.Column<int>(type: "INTEGER", nullable: false),
            //        MemberGroup = table.Column<int>(type: "INTEGER", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_cmsMember2MemberGroup", x => new { x.Member, x.MemberGroup });
            //        table.ForeignKey(
            //            name: "FK_cmsMember2MemberGroup_cmsMember_nodeId",
            //            column: x => x.Member,
            //            principalTable: "cmsMember",
            //            principalColumn: "nodeId");
            //        table.ForeignKey(
            //            name: "FK_cmsMember2MemberGroup_umbracoNode_id",
            //            column: x => x.MemberGroup,
            //            principalTable: "umbracoNode",
            //            principalColumn: "id");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "umbracoContentVersionCultureVariation",
            //    columns: table => new
            //    {
            //        id = table.Column<int>(type: "INTEGER", nullable: false)
            //            .Annotation("Sqlite:Autoincrement", true),
            //        versionId = table.Column<int>(type: "INTEGER", nullable: false),
            //        languageId = table.Column<int>(type: "INTEGER", nullable: false),
            //        name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
            //        date = table.Column<DateTime>(type: "datetime", nullable: false),
            //        availableUserId = table.Column<int>(type: "INTEGER", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_umbracoContentVersionCultureVariation", x => x.id);
            //        table.ForeignKey(
            //            name: "FK_umbracoContentVersionCultureVariation_umbracoContentVersion_id",
            //            column: x => x.versionId,
            //            principalTable: "umbracoContentVersion",
            //            principalColumn: "id");
            //        table.ForeignKey(
            //            name: "FK_umbracoContentVersionCultureVariation_umbracoLanguage_id",
            //            column: x => x.languageId,
            //            principalTable: "umbracoLanguage",
            //            principalColumn: "id");
            //        table.ForeignKey(
            //            name: "FK_umbracoContentVersionCultureVariation_umbracoUser_id",
            //            column: x => x.availableUserId,
            //            principalTable: "umbracoUser",
            //            principalColumn: "id");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "umbracoDocumentVersion",
            //    columns: table => new
            //    {
            //        id = table.Column<int>(type: "INTEGER", nullable: false),
            //        templateId = table.Column<int>(type: "INTEGER", nullable: true),
            //        published = table.Column<bool>(type: "INTEGER", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_umbracoDocumentVersion", x => x.id);
            //        table.ForeignKey(
            //            name: "FK_umbracoDocumentVersion_cmsTemplate_nodeId",
            //            column: x => x.templateId,
            //            principalTable: "cmsTemplate",
            //            principalColumn: "nodeId");
            //        table.ForeignKey(
            //            name: "FK_umbracoDocumentVersion_umbracoContentVersion_id",
            //            column: x => x.id,
            //            principalTable: "umbracoContentVersion",
            //            principalColumn: "id");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "umbracoMediaVersion",
            //    columns: table => new
            //    {
            //        id = table.Column<int>(type: "INTEGER", nullable: false),
            //        path = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_umbracoMediaVersion", x => x.id);
            //        table.ForeignKey(
            //            name: "FK_umbracoMediaVersion_umbracoContentVersion_id",
            //            column: x => x.id,
            //            principalTable: "umbracoContentVersion",
            //            principalColumn: "id");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "umbracoPropertyData",
            //    columns: table => new
            //    {
            //        id = table.Column<int>(type: "INTEGER", nullable: false)
            //            .Annotation("Sqlite:Autoincrement", true),
            //        versionId = table.Column<int>(type: "INTEGER", nullable: false),
            //        propertyTypeId = table.Column<int>(type: "INTEGER", nullable: false),
            //        languageId = table.Column<int>(type: "INTEGER", nullable: true),
            //        segment = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
            //        intValue = table.Column<int>(type: "INTEGER", nullable: true),
            //        decimalValue = table.Column<decimal>(type: "decimal(38, 6)", nullable: true),
            //        dateValue = table.Column<DateTime>(type: "datetime", nullable: true),
            //        varcharValue = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
            //        textValue = table.Column<string>(type: "TEXT", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_umbracoPropertyData", x => x.id);
            //        table.ForeignKey(
            //            name: "FK_umbracoPropertyData_cmsPropertyType_id",
            //            column: x => x.propertyTypeId,
            //            principalTable: "cmsPropertyType",
            //            principalColumn: "id");
            //        table.ForeignKey(
            //            name: "FK_umbracoPropertyData_umbracoContentVersion_id",
            //            column: x => x.versionId,
            //            principalTable: "umbracoContentVersion",
            //            principalColumn: "id");
            //        table.ForeignKey(
            //            name: "FK_umbracoPropertyData_umbracoLanguage_id",
            //            column: x => x.languageId,
            //            principalTable: "umbracoLanguage",
            //            principalColumn: "id");
            //    });

            //migrationBuilder.CreateIndex(
            //    name: "IX_cmsContentNu_published",
            //    table: "cmsContentNu",
            //    columns: new[] { "published", "nodeId", "rv" });

            //migrationBuilder.CreateIndex(
            //    name: "IX_cmsContentType",
            //    table: "cmsContentType",
            //    column: "nodeId",
            //    unique: true);

            //migrationBuilder.CreateIndex(
            //    name: "IX_cmsContentType_icon",
            //    table: "cmsContentType",
            //    column: "icon");

            //migrationBuilder.CreateIndex(
            //    name: "IX_cmsContentType2ContentType_childContentTypeId",
            //    table: "cmsContentType2ContentType",
            //    column: "childContentTypeId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_cmsContentTypeAllowedContentType_AllowedId",
            //    table: "cmsContentTypeAllowedContentType",
            //    column: "AllowedId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_cmsDictionary_id",
            //    table: "cmsDictionary",
            //    column: "id",
            //    unique: true);

            //migrationBuilder.CreateIndex(
            //    name: "IX_cmsDictionary_key",
            //    table: "cmsDictionary",
            //    column: "key",
            //    unique: true);

            //migrationBuilder.CreateIndex(
            //    name: "IX_cmsDictionary_Parent",
            //    table: "cmsDictionary",
            //    column: "parent");

            //migrationBuilder.CreateIndex(
            //    name: "IX_cmsDocumentType_templateNodeId",
            //    table: "cmsDocumentType",
            //    column: "templateNodeId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_cmsLanguageText_languageId",
            //    table: "cmsLanguageText",
            //    columns: new[] { "languageId", "UniqueId" },
            //    unique: true);

            //migrationBuilder.CreateIndex(
            //    name: "IX_cmsLanguageText_UniqueId",
            //    table: "cmsLanguageText",
            //    column: "UniqueId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_cmsMacro_UniqueId",
            //    table: "cmsMacro",
            //    column: "uniqueId",
            //    unique: true);

            //migrationBuilder.CreateIndex(
            //    name: "IX_cmsMacroPropertyAlias",
            //    table: "cmsMacro",
            //    column: "macroAlias",
            //    unique: true);

            //migrationBuilder.CreateIndex(
            //    name: "IX_cmsMacroProperty_Alias",
            //    table: "cmsMacroProperty",
            //    columns: new[] { "macro", "macroPropertyAlias" },
            //    unique: true);

            //migrationBuilder.CreateIndex(
            //    name: "IX_cmsMacroProperty_UniquePropertyId",
            //    table: "cmsMacroProperty",
            //    column: "uniquePropertyId",
            //    unique: true);

            //migrationBuilder.CreateIndex(
            //    name: "IX_cmsMember_LoginName",
            //    table: "cmsMember",
            //    column: "LoginName");

            //migrationBuilder.CreateIndex(
            //    name: "IX_cmsMember2MemberGroup_MemberGroup",
            //    table: "cmsMember2MemberGroup",
            //    column: "MemberGroup");

            //migrationBuilder.CreateIndex(
            //    name: "IX_cmsMemberType_NodeId",
            //    table: "cmsMemberType",
            //    column: "NodeId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_cmsPropertyType_contentTypeId",
            //    table: "cmsPropertyType",
            //    column: "contentTypeId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_cmsPropertyType_dataTypeId",
            //    table: "cmsPropertyType",
            //    column: "dataTypeId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_cmsPropertyType_propertyTypeGroupId",
            //    table: "cmsPropertyType",
            //    column: "propertyTypeGroupId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_cmsPropertyTypeAlias",
            //    table: "cmsPropertyType",
            //    column: "Alias");

            //migrationBuilder.CreateIndex(
            //    name: "IX_cmsPropertyTypeUniqueID",
            //    table: "cmsPropertyType",
            //    column: "UniqueID",
            //    unique: true);

            //migrationBuilder.CreateIndex(
            //    name: "IX_cmsPropertyTypeGroup_contenttypeNodeId",
            //    table: "cmsPropertyTypeGroup",
            //    column: "contenttypeNodeId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_cmsPropertyTypeGroupUniqueID",
            //    table: "cmsPropertyTypeGroup",
            //    column: "uniqueID",
            //    unique: true);

            //migrationBuilder.CreateIndex(
            //    name: "IX_cmsTagRelationship_propertyTypeId",
            //    table: "cmsTagRelationship",
            //    column: "propertyTypeId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_cmsTagRelationship_tagId_nodeId",
            //    table: "cmsTagRelationship",
            //    columns: new[] { "tagId", "nodeId" });

            //migrationBuilder.CreateIndex(
            //    name: "IX_cmsTags",
            //    table: "cmsTags",
            //    columns: new[] { "group", "tag", "languageId" },
            //    unique: true);

            //migrationBuilder.CreateIndex(
            //    name: "IX_cmsTags_LanguageId",
            //    table: "cmsTags",
            //    column: "languageId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_cmsTags_languageId_group",
            //    table: "cmsTags",
            //    columns: new[] { "languageId", "group" });

            //migrationBuilder.CreateIndex(
            //    name: "IX_cmsTemplate_nodeId",
            //    table: "cmsTemplate",
            //    column: "nodeId",
            //    unique: true);

            //migrationBuilder.CreateIndex(
            //    name: "IX_umbracoAccess_loginNodeId",
            //    table: "umbracoAccess",
            //    column: "loginNodeId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_umbracoAccess_noAccessNodeId",
            //    table: "umbracoAccess",
            //    column: "noAccessNodeId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_umbracoAccess_nodeId",
            //    table: "umbracoAccess",
            //    column: "nodeId",
            //    unique: true);

            //migrationBuilder.CreateIndex(
            //    name: "IX_umbracoAccessRule",
            //    table: "umbracoAccessRule",
            //    columns: new[] { "ruleValue", "ruleType", "accessId" },
            //    unique: true);

            //migrationBuilder.CreateIndex(
            //    name: "IX_umbracoAccessRule_accessId",
            //    table: "umbracoAccessRule",
            //    column: "accessId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_umbracoContent_contentTypeId",
            //    table: "umbracoContent",
            //    column: "contentTypeId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_umbracoContentSchedule_languageId",
            //    table: "umbracoContentSchedule",
            //    column: "languageId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_umbracoContentSchedule_nodeId",
            //    table: "umbracoContentSchedule",
            //    column: "nodeId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_umbracoContentVersion_Current",
            //    table: "umbracoContentVersion",
            //    column: "current");

            //migrationBuilder.CreateIndex(
            //    name: "IX_umbracoContentVersion_NodeId",
            //    table: "umbracoContentVersion",
            //    columns: new[] { "nodeId", "current" });

            //migrationBuilder.CreateIndex(
            //    name: "IX_umbracoContentVersion_userId",
            //    table: "umbracoContentVersion",
            //    column: "userId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_umbracoContentVersionCultureVariation_availableUserId",
            //    table: "umbracoContentVersionCultureVariation",
            //    column: "availableUserId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_umbracoContentVersionCultureVariation_LanguageId",
            //    table: "umbracoContentVersionCultureVariation",
            //    column: "languageId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_umbracoContentVersionCultureVariation_VersionId",
            //    table: "umbracoContentVersionCultureVariation",
            //    columns: new[] { "versionId", "languageId" },
            //    unique: true);

            //migrationBuilder.CreateIndex(
            //    name: "IX_umbracoCreatedPackageSchema_Name",
            //    table: "umbracoCreatedPackageSchema",
            //    column: "name",
            //    unique: true);

            //migrationBuilder.CreateIndex(
            //    name: "IX_umbracoDocument_Published",
            //    table: "umbracoDocument",
            //    column: "published");

            //migrationBuilder.CreateIndex(
            //    name: "IX_umbracoDocumentCultureVariation_LanguageId",
            //    table: "umbracoDocumentCultureVariation",
            //    column: "languageId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_umbracoDocumentCultureVariation_NodeId",
            //    table: "umbracoDocumentCultureVariation",
            //    columns: new[] { "nodeId", "languageId" },
            //    unique: true);

            //migrationBuilder.CreateIndex(
            //    name: "IX_umbracoDocumentVersion_id_published",
            //    table: "umbracoDocumentVersion",
            //    columns: new[] { "id", "published" });

            //migrationBuilder.CreateIndex(
            //    name: "IX_umbracoDocumentVersion_published",
            //    table: "umbracoDocumentVersion",
            //    column: "published");

            //migrationBuilder.CreateIndex(
            //    name: "IX_umbracoDocumentVersion_templateId",
            //    table: "umbracoDocumentVersion",
            //    column: "templateId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_umbracoDomain_domainRootStructureID",
            //    table: "umbracoDomain",
            //    column: "domainRootStructureID");

            //migrationBuilder.CreateIndex(
            //    name: "IX_umbracoExternalLogin_LoginProvider",
            //    table: "umbracoExternalLogin",
            //    columns: new[] { "loginProvider", "userOrMemberKey" },
            //    unique: true);

            //migrationBuilder.CreateIndex(
            //    name: "IX_umbracoExternalLogin_ProviderKey",
            //    table: "umbracoExternalLogin",
            //    columns: new[] { "loginProvider", "providerKey" });

            //migrationBuilder.CreateIndex(
            //    name: "IX_umbracoExternalLogin_userOrMemberKey",
            //    table: "umbracoExternalLogin",
            //    column: "userOrMemberKey");

            //migrationBuilder.CreateIndex(
            //    name: "IX_umbracoExternalLoginToken_Name",
            //    table: "umbracoExternalLoginToken",
            //    columns: new[] { "externalLoginId", "name" },
            //    unique: true);

            //migrationBuilder.CreateIndex(
            //    name: "IX_umbracoLanguage_fallbackLanguageId",
            //    table: "umbracoLanguage",
            //    column: "fallbackLanguageId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_umbracoLanguage_languageISOCode",
            //    table: "umbracoLanguage",
            //    column: "languageISOCode",
            //    unique: true);

            //migrationBuilder.CreateIndex(
            //    name: "IX_umbracoLog",
            //    table: "umbracoLog",
            //    column: "NodeId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_umbracoLog_datestamp",
            //    table: "umbracoLog",
            //    columns: new[] { "Datestamp", "userId", "NodeId" });

            //migrationBuilder.CreateIndex(
            //    name: "IX_umbracoLog_datestamp_logheader",
            //    table: "umbracoLog",
            //    columns: new[] { "Datestamp", "logHeader" });

            //migrationBuilder.CreateIndex(
            //    name: "IX_umbracoLog_userId",
            //    table: "umbracoLog",
            //    column: "userId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_LogViewerQuery_name",
            //    table: "umbracoLogViewerQuery",
            //    column: "name",
            //    unique: true);

            //migrationBuilder.CreateIndex(
            //    name: "IX_umbracoMediaVersion",
            //    table: "umbracoMediaVersion",
            //    columns: new[] { "id", "path" },
            //    unique: true);

            //migrationBuilder.CreateIndex(
            //    name: "IX_umbracoNode_Level",
            //    table: "umbracoNode",
            //    columns: new[] { "level", "parentId", "sortOrder", "nodeObjectType", "trashed" });

            //migrationBuilder.CreateIndex(
            //    name: "IX_umbracoNode_nodeUser",
            //    table: "umbracoNode",
            //    column: "nodeUser");

            //migrationBuilder.CreateIndex(
            //    name: "IX_umbracoNode_ObjectType",
            //    table: "umbracoNode",
            //    columns: new[] { "nodeObjectType", "trashed" });

            //migrationBuilder.CreateIndex(
            //    name: "IX_umbracoNode_ObjectType_trashed_sorted",
            //    table: "umbracoNode",
            //    columns: new[] { "nodeObjectType", "trashed", "sortOrder", "id" });

            //migrationBuilder.CreateIndex(
            //    name: "IX_umbracoNode_parentId_nodeObjectType",
            //    table: "umbracoNode",
            //    columns: new[] { "parentId", "nodeObjectType" });

            //migrationBuilder.CreateIndex(
            //    name: "IX_umbracoNode_Path",
            //    table: "umbracoNode",
            //    column: "path");

            //migrationBuilder.CreateIndex(
            //    name: "IX_umbracoNode_Trashed",
            //    table: "umbracoNode",
            //    column: "trashed");

            //migrationBuilder.CreateIndex(
            //    name: "IX_umbracoNode_UniqueId",
            //    table: "umbracoNode",
            //    column: "uniqueId",
            //    unique: true);

            //migrationBuilder.CreateIndex(
            //    name: "IX_umbracoPropertyData_LanguageId",
            //    table: "umbracoPropertyData",
            //    column: "languageId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_umbracoPropertyData_PropertyTypeId",
            //    table: "umbracoPropertyData",
            //    column: "propertyTypeId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_umbracoPropertyData_Segment",
            //    table: "umbracoPropertyData",
            //    column: "segment");

            //migrationBuilder.CreateIndex(
            //    name: "IX_umbracoPropertyData_VersionId",
            //    table: "umbracoPropertyData",
            //    columns: new[] { "versionId", "propertyTypeId", "languageId", "segment" },
            //    unique: true);

            //migrationBuilder.CreateIndex(
            //    name: "IX_umbracoRedirectUrl",
            //    table: "umbracoRedirectUrl",
            //    columns: new[] { "urlHash", "contentKey", "culture", "createDateUtc" },
            //    unique: true);

            //migrationBuilder.CreateIndex(
            //    name: "IX_umbracoRedirectUrl_contentKey",
            //    table: "umbracoRedirectUrl",
            //    column: "contentKey");

            //migrationBuilder.CreateIndex(
            //    name: "IX_umbracoRedirectUrl_culture_hash",
            //    table: "umbracoRedirectUrl",
            //    column: "createDateUtc");

            //migrationBuilder.CreateIndex(
            //    name: "IX_umbracoRelation_childId",
            //    table: "umbracoRelation",
            //    column: "childId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_umbracoRelation_parentChildType",
            //    table: "umbracoRelation",
            //    columns: new[] { "parentId", "childId", "relType" },
            //    unique: true);

            //migrationBuilder.CreateIndex(
            //    name: "IX_umbracoRelation_relType",
            //    table: "umbracoRelation",
            //    column: "relType");

            //migrationBuilder.CreateIndex(
            //    name: "IX_umbracoRelationType_alias",
            //    table: "umbracoRelationType",
            //    column: "alias",
            //    unique: true);

            //migrationBuilder.CreateIndex(
            //    name: "IX_umbracoRelationType_name",
            //    table: "umbracoRelationType",
            //    column: "name",
            //    unique: true);

            //migrationBuilder.CreateIndex(
            //    name: "IX_umbracoRelationType_UniqueId",
            //    table: "umbracoRelationType",
            //    column: "typeUniqueId",
            //    unique: true);

            //migrationBuilder.CreateIndex(
            //    name: "IX_computerName",
            //    table: "umbracoServer",
            //    column: "computerName",
            //    unique: true);

            //migrationBuilder.CreateIndex(
            //    name: "IX_umbracoServer_isActive",
            //    table: "umbracoServer",
            //    column: "isActive");

            //migrationBuilder.CreateIndex(
            //    name: "IX_umbracoTwoFactorLogin_ProviderName",
            //    table: "umbracoTwoFactorLogin",
            //    columns: new[] { "providerName", "userOrMemberKey" },
            //    unique: true);

            //migrationBuilder.CreateIndex(
            //    name: "IX_umbracoTwoFactorLogin_userOrMemberKey",
            //    table: "umbracoTwoFactorLogin",
            //    column: "userOrMemberKey");

            //migrationBuilder.CreateIndex(
            //    name: "IX_umbracoUser_userLogin",
            //    table: "umbracoUser",
            //    column: "userLogin");

            //migrationBuilder.CreateIndex(
            //    name: "IX_umbracoUser2NodeNotify_nodeId",
            //    table: "umbracoUser2NodeNotify",
            //    column: "nodeId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_umbracoUser2UserGroup_userGroupId",
            //    table: "umbracoUser2UserGroup",
            //    column: "userGroupId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_umbracoUserGroup_startContentId",
            //    table: "umbracoUserGroup",
            //    column: "startContentId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_umbracoUserGroup_startMediaId",
            //    table: "umbracoUserGroup",
            //    column: "startMediaId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_umbracoUserGroup_userGroupAlias",
            //    table: "umbracoUserGroup",
            //    column: "userGroupAlias",
            //    unique: true);

            //migrationBuilder.CreateIndex(
            //    name: "IX_umbracoUserGroup_userGroupName",
            //    table: "umbracoUserGroup",
            //    column: "userGroupName",
            //    unique: true);

            //migrationBuilder.CreateIndex(
            //    name: "IX_umbracoUserGroup2Language_languageId",
            //    table: "umbracoUserGroup2Language",
            //    column: "languageId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_umbracoUserGroup2Node_nodeId",
            //    table: "umbracoUserGroup2Node",
            //    column: "nodeId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_umbracoUser2NodePermission_nodeId",
            //    table: "umbracoUserGroup2NodePermission",
            //    column: "nodeId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_umbracoUserLogin_lastValidatedUtc",
            //    table: "umbracoUserLogin",
            //    column: "lastValidatedUtc");

            //migrationBuilder.CreateIndex(
            //    name: "IX_umbracoUserLogin_userId",
            //    table: "umbracoUserLogin",
            //    column: "userId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_umbracoUserStartNode_startNode",
            //    table: "umbracoUserStartNode",
            //    column: "startNode");

            //migrationBuilder.CreateIndex(
            //    name: "IX_umbracoUserStartNode_startNodeType",
            //    table: "umbracoUserStartNode",
            //    columns: new[] { "startNodeType", "startNode", "userId" },
            //    unique: true);

            //migrationBuilder.CreateIndex(
            //    name: "IX_umbracoUserStartNode_userId",
            //    table: "umbracoUserStartNode",
            //    column: "userId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            /*
             * Uncomment the block below when the NPOCO migrations are removed. This will create the tables in the same way as the NPOCO migrations.
             */

            //migrationBuilder.DropTable(
            //    name: "cmsContentNu");

            //migrationBuilder.DropTable(
            //    name: "cmsContentType2ContentType");

            //migrationBuilder.DropTable(
            //    name: "cmsContentTypeAllowedContentType");

            //migrationBuilder.DropTable(
            //    name: "cmsDocumentType");

            //migrationBuilder.DropTable(
            //    name: "cmsLanguageText");

            //migrationBuilder.DropTable(
            //    name: "cmsMacroProperty");

            //migrationBuilder.DropTable(
            //    name: "cmsMember2MemberGroup");

            //migrationBuilder.DropTable(
            //    name: "cmsMemberType");

            //migrationBuilder.DropTable(
            //    name: "cmsTagRelationship");

            //migrationBuilder.DropTable(
            //    name: "umbracoAccessRule");

            //migrationBuilder.DropTable(
            //    name: "umbracoAudit");

            //migrationBuilder.DropTable(
            //    name: "umbracoCacheInstruction");

            //migrationBuilder.DropTable(
            //    name: "umbracoConsent");

            //migrationBuilder.DropTable(
            //    name: "umbracoContentSchedule");

            //migrationBuilder.DropTable(
            //    name: "umbracoContentVersionCleanupPolicy");

            //migrationBuilder.DropTable(
            //    name: "umbracoContentVersionCultureVariation");

            //migrationBuilder.DropTable(
            //    name: "umbracoCreatedPackageSchema");

            //migrationBuilder.DropTable(
            //    name: "umbracoDocument");

            //migrationBuilder.DropTable(
            //    name: "umbracoDocumentCultureVariation");

            //migrationBuilder.DropTable(
            //    name: "umbracoDocumentVersion");

            //migrationBuilder.DropTable(
            //    name: "umbracoDomain");

            //migrationBuilder.DropTable(
            //    name: "umbracoExternalLoginToken");

            //migrationBuilder.DropTable(
            //    name: "umbracoKeyValue");

            //migrationBuilder.DropTable(
            //    name: "umbracoLock");

            //migrationBuilder.DropTable(
            //    name: "umbracoLog");

            //migrationBuilder.DropTable(
            //    name: "umbracoLogViewerQuery");

            //migrationBuilder.DropTable(
            //    name: "umbracoMediaVersion");

            //migrationBuilder.DropTable(
            //    name: "umbracoPropertyData");

            //migrationBuilder.DropTable(
            //    name: "umbracoRedirectUrl");

            //migrationBuilder.DropTable(
            //    name: "umbracoRelation");

            //migrationBuilder.DropTable(
            //    name: "umbracoServer");

            //migrationBuilder.DropTable(
            //    name: "umbracoTwoFactorLogin");

            //migrationBuilder.DropTable(
            //    name: "umbracoUser2NodeNotify");

            //migrationBuilder.DropTable(
            //    name: "umbracoUser2UserGroup");

            //migrationBuilder.DropTable(
            //    name: "umbracoUserGroup2App");

            //migrationBuilder.DropTable(
            //    name: "umbracoUserGroup2Language");

            //migrationBuilder.DropTable(
            //    name: "umbracoUserGroup2Node");

            //migrationBuilder.DropTable(
            //    name: "umbracoUserGroup2NodePermission");

            //migrationBuilder.DropTable(
            //    name: "umbracoUserLogin");

            //migrationBuilder.DropTable(
            //    name: "umbracoUserStartNode");

            //migrationBuilder.DropTable(
            //    name: "cmsDictionary");

            //migrationBuilder.DropTable(
            //    name: "cmsMacro");

            //migrationBuilder.DropTable(
            //    name: "cmsMember");

            //migrationBuilder.DropTable(
            //    name: "cmsTags");

            //migrationBuilder.DropTable(
            //    name: "umbracoAccess");

            //migrationBuilder.DropTable(
            //    name: "cmsTemplate");

            //migrationBuilder.DropTable(
            //    name: "umbracoExternalLogin");

            //migrationBuilder.DropTable(
            //    name: "cmsPropertyType");

            //migrationBuilder.DropTable(
            //    name: "umbracoContentVersion");

            //migrationBuilder.DropTable(
            //    name: "umbracoRelationType");

            //migrationBuilder.DropTable(
            //    name: "umbracoUserGroup");

            //migrationBuilder.DropTable(
            //    name: "umbracoLanguage");

            //migrationBuilder.DropTable(
            //    name: "cmsPropertyTypeGroup");

            //migrationBuilder.DropTable(
            //    name: "umbracoDataType");

            //migrationBuilder.DropTable(
            //    name: "umbracoContent");

            //migrationBuilder.DropTable(
            //    name: "cmsContentType");

            //migrationBuilder.DropTable(
            //    name: "umbracoNode");

            //migrationBuilder.DropTable(
            //    name: "umbracoUser");
        }
    }
}
