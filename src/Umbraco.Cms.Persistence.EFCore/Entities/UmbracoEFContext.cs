using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Umbraco.Cms.Persistence.EFCore.Entities
{
    public partial class UmbracoEFContext : DbContext
    {
        public UmbracoEFContext()
        {
        }

        public UmbracoEFContext(DbContextOptions<UmbracoEFContext> options)
            : base(options)
        {
        }

        public virtual DbSet<CmsContentNu> CmsContentNus { get; set; } = null!;
        public virtual DbSet<CmsContentType> CmsContentTypes { get; set; } = null!;
        public virtual DbSet<CmsContentTypeAllowedContentType> CmsContentTypeAllowedContentTypes { get; set; } = null!;
        public virtual DbSet<CmsDictionary> CmsDictionaries { get; set; } = null!;
        public virtual DbSet<CmsDocumentType> CmsDocumentTypes { get; set; } = null!;
        public virtual DbSet<CmsLanguageText> CmsLanguageTexts { get; set; } = null!;
        public virtual DbSet<CmsMacro> CmsMacros { get; set; } = null!;
        public virtual DbSet<CmsMacroProperty> CmsMacroProperties { get; set; } = null!;
        public virtual DbSet<CmsMember> CmsMembers { get; set; } = null!;
        public virtual DbSet<CmsMemberType> CmsMemberTypes { get; set; } = null!;
        public virtual DbSet<CmsPropertyType> CmsPropertyTypes { get; set; } = null!;
        public virtual DbSet<CmsPropertyTypeGroup> CmsPropertyTypeGroups { get; set; } = null!;
        public virtual DbSet<CmsTag> CmsTags { get; set; } = null!;
        public virtual DbSet<CmsTagRelationship> CmsTagRelationships { get; set; } = null!;
        public virtual DbSet<CmsTemplate> CmsTemplates { get; set; } = null!;
        public virtual DbSet<UmbracoAccess> UmbracoAccesses { get; set; } = null!;
        public virtual DbSet<UmbracoAccessRule> UmbracoAccessRules { get; set; } = null!;
        public virtual DbSet<UmbracoAudit> UmbracoAudits { get; set; } = null!;
        public virtual DbSet<UmbracoCacheInstruction> UmbracoCacheInstructions { get; set; } = null!;
        public virtual DbSet<UmbracoConsent> UmbracoConsents { get; set; } = null!;
        public virtual DbSet<UmbracoContent> UmbracoContents { get; set; } = null!;
        public virtual DbSet<UmbracoContentSchedule> UmbracoContentSchedules { get; set; } = null!;
        public virtual DbSet<UmbracoContentVersion> UmbracoContentVersions { get; set; } = null!;
        public virtual DbSet<UmbracoContentVersionCleanupPolicy> UmbracoContentVersionCleanupPolicies { get; set; } = null!;
        public virtual DbSet<UmbracoContentVersionCultureVariation> UmbracoContentVersionCultureVariations { get; set; } = null!;
        public virtual DbSet<UmbracoCreatedPackageSchema> UmbracoCreatedPackageSchemas { get; set; } = null!;
        public virtual DbSet<UmbracoDataType> UmbracoDataTypes { get; set; } = null!;
        public virtual DbSet<UmbracoDocument> UmbracoDocuments { get; set; } = null!;
        public virtual DbSet<UmbracoDocumentCultureVariation> UmbracoDocumentCultureVariations { get; set; } = null!;
        public virtual DbSet<UmbracoDocumentVersion> UmbracoDocumentVersions { get; set; } = null!;
        public virtual DbSet<UmbracoDomain> UmbracoDomains { get; set; } = null!;
        public virtual DbSet<UmbracoExternalLogin> UmbracoExternalLogins { get; set; } = null!;
        public virtual DbSet<UmbracoExternalLoginToken> UmbracoExternalLoginTokens { get; set; } = null!;
        public virtual DbSet<UmbracoKeyValue> UmbracoKeyValues { get; set; } = null!;
        public virtual DbSet<UmbracoLanguage> UmbracoLanguages { get; set; } = null!;
        public virtual DbSet<UmbracoLock> UmbracoLocks { get; set; } = null!;
        public virtual DbSet<UmbracoLog> UmbracoLogs { get; set; } = null!;
        public virtual DbSet<UmbracoLogViewerQuery> UmbracoLogViewerQueries { get; set; } = null!;
        public virtual DbSet<UmbracoMediaVersion> UmbracoMediaVersions { get; set; } = null!;
        public virtual DbSet<UmbracoNode> UmbracoNodes { get; set; } = null!;
        public virtual DbSet<UmbracoPropertyDatum> UmbracoPropertyData { get; set; } = null!;
        public virtual DbSet<UmbracoRedirectUrl> UmbracoRedirectUrls { get; set; } = null!;
        public virtual DbSet<UmbracoRelation> UmbracoRelations { get; set; } = null!;
        public virtual DbSet<UmbracoRelationType> UmbracoRelationTypes { get; set; } = null!;
        public virtual DbSet<UmbracoServer> UmbracoServers { get; set; } = null!;
        public virtual DbSet<UmbracoTwoFactorLogin> UmbracoTwoFactorLogins { get; set; } = null!;
        public virtual DbSet<UmbracoUser> UmbracoUsers { get; set; } = null!;
        public virtual DbSet<UmbracoUser2NodeNotify> UmbracoUser2NodeNotifies { get; set; } = null!;
        public virtual DbSet<UmbracoUserGroup> UmbracoUserGroups { get; set; } = null!;
        public virtual DbSet<UmbracoUserGroup2App> UmbracoUserGroup2Apps { get; set; } = null!;
        public virtual DbSet<UmbracoUserGroup2NodePermission> UmbracoUserGroup2NodePermissions { get; set; } = null!;
        public virtual DbSet<UmbracoUserLogin> UmbracoUserLogins { get; set; } = null!;
        public virtual DbSet<UmbracoUserStartNode> UmbracoUserStartNodes { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CmsContentNu>(entity =>
            {
                entity.HasKey(e => new { e.NodeId, e.Published });

                entity.ToTable("cmsContentNu");

                entity.Property(e => e.NodeId).HasColumnName("nodeId");

                entity.Property(e => e.Published).HasColumnName("published");

                entity.Property(e => e.Data)
                    .HasColumnType("ntext")
                    .HasColumnName("data");

                entity.Property(e => e.DataRaw).HasColumnName("dataRaw");

                entity.Property(e => e.Rv).HasColumnName("rv");

                entity.HasOne(d => d.Node)
                    .WithMany(p => p.CmsContentNus)
                    .HasForeignKey(d => d.NodeId);
            });

            modelBuilder.Entity<CmsContentType>(entity =>
            {
                entity.HasKey(e => e.Pk);

                entity.ToTable("cmsContentType");

                entity.HasIndex(e => e.NodeId, "IX_cmsContentType")
                    .IsUnique();

                entity.HasIndex(e => e.Icon, "IX_cmsContentType_icon");

                entity.Property(e => e.Pk).HasColumnName("pk");

                entity.Property(e => e.Alias)
                    .HasMaxLength(255)
                    .HasColumnName("alias");

                entity.Property(e => e.AllowAtRoot)
                    .IsRequired()
                    .HasColumnName("allowAtRoot")
                    .HasDefaultValueSql("('0')");

                entity.Property(e => e.Description)
                    .HasMaxLength(1500)
                    .HasColumnName("description");

                entity.Property(e => e.Icon)
                    .HasMaxLength(255)
                    .HasColumnName("icon");

                entity.Property(e => e.IsContainer)
                    .IsRequired()
                    .HasColumnName("isContainer")
                    .HasDefaultValueSql("('0')");

                entity.Property(e => e.IsElement)
                    .IsRequired()
                    .HasColumnName("isElement")
                    .HasDefaultValueSql("('0')");

                entity.Property(e => e.NodeId).HasColumnName("nodeId");

                entity.Property(e => e.Thumbnail)
                    .HasMaxLength(255)
                    .HasColumnName("thumbnail")
                    .HasDefaultValueSql("('folder.png')");

                entity.Property(e => e.Variations)
                    .HasColumnName("variations")
                    .HasDefaultValueSql("('1')");

                entity.HasOne(d => d.Node)
                    .WithOne(p => p.CmsContentType)
                    .HasForeignKey<CmsContentType>(d => d.NodeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_cmsContentType_umbracoNode_id");
            });

            modelBuilder.Entity<CmsContentTypeAllowedContentType>(entity =>
            {
                entity.HasKey(e => new { e.Id, e.AllowedId });

                entity.ToTable("cmsContentTypeAllowedContentType");

                entity.Property(e => e.SortOrder).HasDefaultValueSql("('0')");

                entity.HasOne(d => d.Allowed)
                    .WithMany(p => p.CmsContentTypeAllowedContentTypeAlloweds)
                    .HasPrincipalKey(p => p.NodeId)
                    .HasForeignKey(d => d.AllowedId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_cmsContentTypeAllowedContentType_cmsContentType1");

                entity.HasOne(d => d.IdNavigation)
                    .WithMany(p => p.CmsContentTypeAllowedContentTypeIdNavigations)
                    .HasPrincipalKey(p => p.NodeId)
                    .HasForeignKey(d => d.Id)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_cmsContentTypeAllowedContentType_cmsContentType");
            });

            modelBuilder.Entity<CmsDictionary>(entity =>
            {
                entity.HasKey(e => e.Pk);

                entity.ToTable("cmsDictionary");

                entity.HasIndex(e => e.Parent, "IX_cmsDictionary_Parent");

                entity.HasIndex(e => e.Id, "IX_cmsDictionary_id")
                    .IsUnique();

                entity.HasIndex(e => e.Key, "IX_cmsDictionary_key")
                    .IsUnique();

                entity.Property(e => e.Pk).HasColumnName("pk");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Key).HasColumnName("key");

                entity.Property(e => e.Parent).HasColumnName("parent");

                entity.HasOne(d => d.ParentNavigation)
                    .WithMany(p => p.InverseParentNavigation)
                    .HasPrincipalKey(p => p.Id)
                    .HasForeignKey(d => d.Parent)
                    .HasConstraintName("FK_cmsDictionary_cmsDictionary_id");
            });

            modelBuilder.Entity<CmsDocumentType>(entity =>
            {
                entity.HasKey(e => new { e.ContentTypeNodeId, e.TemplateNodeId });

                entity.ToTable("cmsDocumentType");

                entity.Property(e => e.ContentTypeNodeId).HasColumnName("contentTypeNodeId");

                entity.Property(e => e.TemplateNodeId).HasColumnName("templateNodeId");

                entity.Property(e => e.IsDefault)
                    .IsRequired()
                    .HasDefaultValueSql("('0')");

                entity.HasOne(d => d.ContentTypeNode)
                    .WithMany(p => p.CmsDocumentTypes)
                    .HasForeignKey(d => d.ContentTypeNodeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_cmsDocumentType_umbracoNode_id");

                entity.HasOne(d => d.ContentTypeNodeNavigation)
                    .WithMany(p => p.CmsDocumentTypes)
                    .HasPrincipalKey(p => p.NodeId)
                    .HasForeignKey(d => d.ContentTypeNodeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_cmsDocumentType_cmsContentType_nodeId");

                entity.HasOne(d => d.TemplateNode)
                    .WithMany(p => p.CmsDocumentTypes)
                    .HasPrincipalKey(p => p.NodeId)
                    .HasForeignKey(d => d.TemplateNodeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_cmsDocumentType_cmsTemplate_nodeId");
            });

            modelBuilder.Entity<CmsLanguageText>(entity =>
            {
                entity.HasKey(e => e.Pk);

                entity.ToTable("cmsLanguageText");

                entity.HasIndex(e => new { e.LanguageId, e.UniqueId }, "IX_cmsLanguageText_languageId")
                    .IsUnique();

                entity.Property(e => e.Pk).HasColumnName("pk");

                entity.Property(e => e.LanguageId).HasColumnName("languageId");

                entity.Property(e => e.Value)
                    .HasMaxLength(1000)
                    .HasColumnName("value");

                entity.HasOne(d => d.Language)
                    .WithMany(p => p.CmsLanguageTexts)
                    .HasForeignKey(d => d.LanguageId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_cmsLanguageText_umbracoLanguage_id");

                entity.HasOne(d => d.Unique)
                    .WithMany(p => p.CmsLanguageTexts)
                    .HasPrincipalKey(p => p.Id)
                    .HasForeignKey(d => d.UniqueId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_cmsLanguageText_cmsDictionary_id");
            });

            modelBuilder.Entity<CmsMacro>(entity =>
            {
                entity.ToTable("cmsMacro");

                entity.HasIndex(e => e.MacroAlias, "IX_cmsMacroPropertyAlias")
                    .IsUnique();

                entity.HasIndex(e => e.UniqueId, "IX_cmsMacro_UniqueId")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.MacroAlias)
                    .HasMaxLength(255)
                    .HasColumnName("macroAlias");

                entity.Property(e => e.MacroCacheByPage)
                    .IsRequired()
                    .HasColumnName("macroCacheByPage")
                    .HasDefaultValueSql("('1')");

                entity.Property(e => e.MacroCachePersonalized)
                    .IsRequired()
                    .HasColumnName("macroCachePersonalized")
                    .HasDefaultValueSql("('0')");

                entity.Property(e => e.MacroDontRender)
                    .IsRequired()
                    .HasColumnName("macroDontRender")
                    .HasDefaultValueSql("('0')");

                entity.Property(e => e.MacroName)
                    .HasMaxLength(255)
                    .HasColumnName("macroName");

                entity.Property(e => e.MacroRefreshRate)
                    .HasColumnName("macroRefreshRate")
                    .HasDefaultValueSql("('0')");

                entity.Property(e => e.MacroSource)
                    .HasMaxLength(255)
                    .HasColumnName("macroSource");

                entity.Property(e => e.MacroType).HasColumnName("macroType");

                entity.Property(e => e.MacroUseInEditor)
                    .IsRequired()
                    .HasColumnName("macroUseInEditor")
                    .HasDefaultValueSql("('0')");

                entity.Property(e => e.UniqueId).HasColumnName("uniqueId");
            });

            modelBuilder.Entity<CmsMacroProperty>(entity =>
            {
                entity.ToTable("cmsMacroProperty");

                entity.HasIndex(e => new { e.Macro, e.MacroPropertyAlias }, "IX_cmsMacroProperty_Alias")
                    .IsUnique();

                entity.HasIndex(e => e.UniquePropertyId, "IX_cmsMacroProperty_UniquePropertyId")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.EditorAlias)
                    .HasMaxLength(255)
                    .HasColumnName("editorAlias");

                entity.Property(e => e.Macro).HasColumnName("macro");

                entity.Property(e => e.MacroPropertyAlias)
                    .HasMaxLength(50)
                    .HasColumnName("macroPropertyAlias");

                entity.Property(e => e.MacroPropertyName)
                    .HasMaxLength(255)
                    .HasColumnName("macroPropertyName");

                entity.Property(e => e.MacroPropertySortOrder)
                    .HasColumnName("macroPropertySortOrder")
                    .HasDefaultValueSql("('0')");

                entity.Property(e => e.UniquePropertyId).HasColumnName("uniquePropertyId");

                entity.HasOne(d => d.MacroNavigation)
                    .WithMany(p => p.CmsMacroProperties)
                    .HasForeignKey(d => d.Macro)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_cmsMacroProperty_cmsMacro_id");
            });

            modelBuilder.Entity<CmsMember>(entity =>
            {
                entity.HasKey(e => e.NodeId);

                entity.ToTable("cmsMember");

                entity.HasIndex(e => e.LoginName, "IX_cmsMember_LoginName");

                entity.Property(e => e.NodeId)
                    .ValueGeneratedNever()
                    .HasColumnName("nodeId");

                entity.Property(e => e.Email)
                    .HasMaxLength(1000)
                    .HasDefaultValueSql("('''')");

                entity.Property(e => e.EmailConfirmedDate)
                    .HasColumnType("datetime")
                    .HasColumnName("emailConfirmedDate");

                entity.Property(e => e.FailedPasswordAttempts).HasColumnName("failedPasswordAttempts");

                entity.Property(e => e.IsApproved)
                    .IsRequired()
                    .HasColumnName("isApproved")
                    .HasDefaultValueSql("('1')");

                entity.Property(e => e.IsLockedOut)
                    .HasColumnName("isLockedOut")
                    .HasDefaultValueSql("('0')");

                entity.Property(e => e.LastLockoutDate)
                    .HasColumnType("datetime")
                    .HasColumnName("lastLockoutDate");

                entity.Property(e => e.LastLoginDate)
                    .HasColumnType("datetime")
                    .HasColumnName("lastLoginDate");

                entity.Property(e => e.LastPasswordChangeDate)
                    .HasColumnType("datetime")
                    .HasColumnName("lastPasswordChangeDate");

                entity.Property(e => e.LoginName)
                    .HasMaxLength(1000)
                    .HasDefaultValueSql("('''')");

                entity.Property(e => e.Password)
                    .HasMaxLength(1000)
                    .HasDefaultValueSql("('''')");

                entity.Property(e => e.PasswordConfig)
                    .HasMaxLength(500)
                    .HasColumnName("passwordConfig");

                entity.Property(e => e.SecurityStampToken)
                    .HasMaxLength(255)
                    .HasColumnName("securityStampToken");

                entity.HasOne(d => d.Node)
                    .WithOne(p => p.CmsMember)
                    .HasForeignKey<CmsMember>(d => d.NodeId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasMany(d => d.MemberGroups)
                    .WithMany(p => p.Members)
                    .UsingEntity<Dictionary<string, object>>(
                        "CmsMember2MemberGroup",
                        l => l.HasOne<UmbracoNode>().WithMany().HasForeignKey("MemberGroup").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_cmsMember2MemberGroup_umbracoNode_id"),
                        r => r.HasOne<CmsMember>().WithMany().HasForeignKey("Member").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_cmsMember2MemberGroup_cmsMember_nodeId"),
                        j =>
                        {
                            j.HasKey("Member", "MemberGroup");

                            j.ToTable("cmsMember2MemberGroup");
                        });
            });

            modelBuilder.Entity<CmsMemberType>(entity =>
            {
                entity.HasKey(e => e.Pk);

                entity.ToTable("cmsMemberType");

                entity.Property(e => e.Pk).HasColumnName("pk");

                entity.Property(e => e.IsSensitive)
                    .IsRequired()
                    .HasColumnName("isSensitive")
                    .HasDefaultValueSql("('0')");

                entity.Property(e => e.MemberCanEdit)
                    .IsRequired()
                    .HasColumnName("memberCanEdit")
                    .HasDefaultValueSql("('0')");

                entity.Property(e => e.PropertytypeId).HasColumnName("propertytypeId");

                entity.Property(e => e.ViewOnProfile)
                    .IsRequired()
                    .HasColumnName("viewOnProfile")
                    .HasDefaultValueSql("('0')");

                entity.HasOne(d => d.Node)
                    .WithMany(p => p.CmsMemberTypes)
                    .HasForeignKey(d => d.NodeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_cmsMemberType_umbracoNode_id");

                entity.HasOne(d => d.NodeNavigation)
                    .WithMany(p => p.CmsMemberTypes)
                    .HasPrincipalKey(p => p.NodeId)
                    .HasForeignKey(d => d.NodeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_cmsMemberType_cmsContentType_nodeId");
            });

            modelBuilder.Entity<CmsPropertyType>(entity =>
            {
                entity.ToTable("cmsPropertyType");

                entity.HasIndex(e => e.Alias, "IX_cmsPropertyTypeAlias");

                entity.HasIndex(e => e.UniqueId, "IX_cmsPropertyTypeUniqueID")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Alias).HasMaxLength(255);

                entity.Property(e => e.ContentTypeId).HasColumnName("contentTypeId");

                entity.Property(e => e.DataTypeId).HasColumnName("dataTypeId");

                entity.Property(e => e.Description).HasMaxLength(2000);

                entity.Property(e => e.LabelOnTop)
                    .IsRequired()
                    .HasColumnName("labelOnTop")
                    .HasDefaultValueSql("('0')");

                entity.Property(e => e.Mandatory)
                    .IsRequired()
                    .HasColumnName("mandatory")
                    .HasDefaultValueSql("('0')");

                entity.Property(e => e.MandatoryMessage)
                    .HasMaxLength(500)
                    .HasColumnName("mandatoryMessage");

                entity.Property(e => e.Name).HasMaxLength(255);

                entity.Property(e => e.PropertyTypeGroupId).HasColumnName("propertyTypeGroupId");

                entity.Property(e => e.SortOrder)
                    .HasColumnName("sortOrder")
                    .HasDefaultValueSql("('0')");

                entity.Property(e => e.UniqueId)
                    .HasColumnName("UniqueID")
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.ValidationRegExp)
                    .HasMaxLength(255)
                    .HasColumnName("validationRegExp");

                entity.Property(e => e.ValidationRegExpMessage)
                    .HasMaxLength(500)
                    .HasColumnName("validationRegExpMessage");

                entity.Property(e => e.Variations)
                    .HasColumnName("variations")
                    .HasDefaultValueSql("('1')");

                entity.HasOne(d => d.ContentType)
                    .WithMany(p => p.CmsPropertyTypes)
                    .HasPrincipalKey(p => p.NodeId)
                    .HasForeignKey(d => d.ContentTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_cmsPropertyType_cmsContentType_nodeId");

                entity.HasOne(d => d.DataType)
                    .WithMany(p => p.CmsPropertyTypes)
                    .HasForeignKey(d => d.DataTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_cmsPropertyType_umbracoDataType_nodeId");

                entity.HasOne(d => d.PropertyTypeGroup)
                    .WithMany(p => p.CmsPropertyTypes)
                    .HasForeignKey(d => d.PropertyTypeGroupId)
                    .HasConstraintName("FK_cmsPropertyType_cmsPropertyTypeGroup_id");
            });

            modelBuilder.Entity<CmsPropertyTypeGroup>(entity =>
            {
                entity.ToTable("cmsPropertyTypeGroup");

                entity.HasIndex(e => e.UniqueId, "IX_cmsPropertyTypeGroupUniqueID")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Alias)
                    .HasMaxLength(255)
                    .HasColumnName("alias");

                entity.Property(e => e.ContenttypeNodeId).HasColumnName("contenttypeNodeId");

                entity.Property(e => e.Sortorder).HasColumnName("sortorder");

                entity.Property(e => e.Text)
                    .HasMaxLength(255)
                    .HasColumnName("text");

                entity.Property(e => e.Type)
                    .HasColumnName("type")
                    .HasDefaultValueSql("('0')");

                entity.Property(e => e.UniqueId)
                    .HasColumnName("uniqueID")
                    .HasDefaultValueSql("(newid())");

                entity.HasOne(d => d.ContenttypeNode)
                    .WithMany(p => p.CmsPropertyTypeGroups)
                    .HasPrincipalKey(p => p.NodeId)
                    .HasForeignKey(d => d.ContenttypeNodeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_cmsPropertyTypeGroup_cmsContentType_nodeId");
            });

            modelBuilder.Entity<CmsTag>(entity =>
            {
                entity.ToTable("cmsTags");

                entity.HasIndex(e => new { e.Group, e.Tag, e.LanguageId }, "IX_cmsTags")
                    .IsUnique();

                entity.HasIndex(e => e.LanguageId, "IX_cmsTags_LanguageId");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Group)
                    .HasMaxLength(100)
                    .HasColumnName("group");

                entity.Property(e => e.LanguageId).HasColumnName("languageId");

                entity.Property(e => e.Tag)
                    .HasMaxLength(200)
                    .HasColumnName("tag");

                entity.HasOne(d => d.Language)
                    .WithMany(p => p.CmsTags)
                    .HasForeignKey(d => d.LanguageId)
                    .HasConstraintName("FK_cmsTags_umbracoLanguage_id");
            });

            modelBuilder.Entity<CmsTagRelationship>(entity =>
            {
                entity.HasKey(e => new { e.NodeId, e.PropertyTypeId, e.TagId });

                entity.ToTable("cmsTagRelationship");

                entity.Property(e => e.NodeId).HasColumnName("nodeId");

                entity.Property(e => e.PropertyTypeId).HasColumnName("propertyTypeId");

                entity.Property(e => e.TagId).HasColumnName("tagId");

                entity.HasOne(d => d.Node)
                    .WithMany(p => p.CmsTagRelationships)
                    .HasForeignKey(d => d.NodeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_cmsTagRelationship_cmsContent");

                entity.HasOne(d => d.PropertyType)
                    .WithMany(p => p.CmsTagRelationships)
                    .HasForeignKey(d => d.PropertyTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_cmsTagRelationship_cmsPropertyType");

                entity.HasOne(d => d.Tag)
                    .WithMany(p => p.CmsTagRelationships)
                    .HasForeignKey(d => d.TagId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_cmsTagRelationship_cmsTags_id");
            });

            modelBuilder.Entity<CmsTemplate>(entity =>
            {
                entity.HasKey(e => e.Pk);

                entity.ToTable("cmsTemplate");

                entity.HasIndex(e => e.NodeId, "IX_cmsTemplate_nodeId")
                    .IsUnique();

                entity.Property(e => e.Pk).HasColumnName("pk");

                entity.Property(e => e.Alias)
                    .HasMaxLength(100)
                    .HasColumnName("alias");

                entity.Property(e => e.NodeId).HasColumnName("nodeId");

                entity.HasOne(d => d.Node)
                    .WithOne(p => p.CmsTemplate)
                    .HasForeignKey<CmsTemplate>(d => d.NodeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_cmsTemplate_umbracoNode");
            });

            modelBuilder.Entity<UmbracoAccess>(entity =>
            {
                entity.ToTable("umbracoAccess");

                entity.HasIndex(e => e.NodeId, "IX_umbracoAccess_nodeId")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.CreateDate)
                    .HasColumnType("datetime")
                    .HasColumnName("createDate")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.LoginNodeId).HasColumnName("loginNodeId");

                entity.Property(e => e.NoAccessNodeId).HasColumnName("noAccessNodeId");

                entity.Property(e => e.NodeId).HasColumnName("nodeId");

                entity.Property(e => e.UpdateDate)
                    .HasColumnType("datetime")
                    .HasColumnName("updateDate")
                    .HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.LoginNode)
                    .WithMany(p => p.UmbracoAccessLoginNodes)
                    .HasForeignKey(d => d.LoginNodeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_umbracoAccess_umbracoNode_id1");

                entity.HasOne(d => d.NoAccessNode)
                    .WithMany(p => p.UmbracoAccessNoAccessNodes)
                    .HasForeignKey(d => d.NoAccessNodeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_umbracoAccess_umbracoNode_id2");

                entity.HasOne(d => d.Node)
                    .WithOne(p => p.UmbracoAccessNode)
                    .HasForeignKey<UmbracoAccess>(d => d.NodeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_umbracoAccess_umbracoNode_id");
            });

            modelBuilder.Entity<UmbracoAccessRule>(entity =>
            {
                entity.ToTable("umbracoAccessRule");

                entity.HasIndex(e => new { e.RuleValue, e.RuleType, e.AccessId }, "IX_umbracoAccessRule")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.AccessId).HasColumnName("accessId");

                entity.Property(e => e.CreateDate)
                    .HasColumnType("datetime")
                    .HasColumnName("createDate")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.RuleType)
                    .HasMaxLength(255)
                    .HasColumnName("ruleType");

                entity.Property(e => e.RuleValue)
                    .HasMaxLength(255)
                    .HasColumnName("ruleValue");

                entity.Property(e => e.UpdateDate)
                    .HasColumnType("datetime")
                    .HasColumnName("updateDate")
                    .HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.Access)
                    .WithMany(p => p.UmbracoAccessRules)
                    .HasForeignKey(d => d.AccessId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_umbracoAccessRule_umbracoAccess_id");
            });

            modelBuilder.Entity<UmbracoAudit>(entity =>
            {
                entity.ToTable("umbracoAudit");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.AffectedDetails)
                    .HasMaxLength(1024)
                    .HasColumnName("affectedDetails");

                entity.Property(e => e.AffectedUserId).HasColumnName("affectedUserId");

                entity.Property(e => e.EventDateUtc)
                    .HasColumnType("datetime")
                    .HasColumnName("eventDateUtc")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.EventDetails)
                    .HasMaxLength(1024)
                    .HasColumnName("eventDetails");

                entity.Property(e => e.EventType)
                    .HasMaxLength(256)
                    .HasColumnName("eventType");

                entity.Property(e => e.PerformingDetails)
                    .HasMaxLength(1024)
                    .HasColumnName("performingDetails");

                entity.Property(e => e.PerformingIp)
                    .HasMaxLength(64)
                    .HasColumnName("performingIp");

                entity.Property(e => e.PerformingUserId).HasColumnName("performingUserId");
            });

            modelBuilder.Entity<UmbracoCacheInstruction>(entity =>
            {
                entity.ToTable("umbracoCacheInstruction");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.InstructionCount)
                    .HasColumnName("instructionCount")
                    .HasDefaultValueSql("('1')");

                entity.Property(e => e.JsonInstruction)
                    .HasColumnType("ntext")
                    .HasColumnName("jsonInstruction");

                entity.Property(e => e.Originated)
                    .HasMaxLength(500)
                    .HasColumnName("originated");

                entity.Property(e => e.UtcStamp)
                    .HasColumnType("datetime")
                    .HasColumnName("utcStamp");
            });

            modelBuilder.Entity<UmbracoConsent>(entity =>
            {
                entity.ToTable("umbracoConsent");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Action)
                    .HasMaxLength(512)
                    .HasColumnName("action");

                entity.Property(e => e.Comment)
                    .HasMaxLength(255)
                    .HasColumnName("comment");

                entity.Property(e => e.Context)
                    .HasMaxLength(128)
                    .HasColumnName("context");

                entity.Property(e => e.CreateDate)
                    .HasColumnType("datetime")
                    .HasColumnName("createDate")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Current).HasColumnName("current");

                entity.Property(e => e.Source)
                    .HasMaxLength(512)
                    .HasColumnName("source");

                entity.Property(e => e.State).HasColumnName("state");
            });

            modelBuilder.Entity<UmbracoContent>(entity =>
            {
                entity.HasKey(e => e.NodeId);

                entity.ToTable("umbracoContent");

                entity.Property(e => e.NodeId)
                    .ValueGeneratedNever()
                    .HasColumnName("nodeId");

                entity.Property(e => e.ContentTypeId).HasColumnName("contentTypeId");

                entity.HasOne(d => d.ContentType)
                    .WithMany(p => p.UmbracoContents)
                    .HasPrincipalKey(p => p.NodeId)
                    .HasForeignKey(d => d.ContentTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_umbracoContent_cmsContentType_NodeId");

                entity.HasOne(d => d.Node)
                    .WithOne(p => p.UmbracoContent)
                    .HasForeignKey<UmbracoContent>(d => d.NodeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_umbracoContent_umbracoNode_id");
            });

            modelBuilder.Entity<UmbracoContentSchedule>(entity =>
            {
                entity.ToTable("umbracoContentSchedule");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.Action)
                    .HasMaxLength(255)
                    .HasColumnName("action");

                entity.Property(e => e.Date)
                    .HasColumnType("datetime")
                    .HasColumnName("date");

                entity.Property(e => e.LanguageId).HasColumnName("languageId");

                entity.Property(e => e.NodeId).HasColumnName("nodeId");

                entity.HasOne(d => d.Language)
                    .WithMany(p => p.UmbracoContentSchedules)
                    .HasForeignKey(d => d.LanguageId)
                    .HasConstraintName("FK_umbracoContentSchedule_umbracoLanguage_id");

                entity.HasOne(d => d.Node)
                    .WithMany(p => p.UmbracoContentSchedules)
                    .HasForeignKey(d => d.NodeId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<UmbracoContentVersion>(entity =>
            {
                entity.ToTable("umbracoContentVersion");

                entity.HasIndex(e => e.Current, "IX_umbracoContentVersion_Current");

                entity.HasIndex(e => new { e.NodeId, e.Current }, "IX_umbracoContentVersion_NodeId");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Current).HasColumnName("current");

                entity.Property(e => e.NodeId).HasColumnName("nodeId");

                entity.Property(e => e.PreventCleanup)
                    .IsRequired()
                    .HasColumnName("preventCleanup")
                    .HasDefaultValueSql("('0')");

                entity.Property(e => e.Text)
                    .HasMaxLength(255)
                    .HasColumnName("text");

                entity.Property(e => e.UserId).HasColumnName("userId");

                entity.Property(e => e.VersionDate)
                    .HasColumnType("datetime")
                    .HasColumnName("versionDate")
                    .HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.Node)
                    .WithMany(p => p.UmbracoContentVersions)
                    .HasForeignKey(d => d.NodeId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UmbracoContentVersions)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_umbracoContentVersion_umbracoUser_id");
            });

            modelBuilder.Entity<UmbracoContentVersionCleanupPolicy>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("umbracoContentVersionCleanupPolicy");

                entity.Property(e => e.ContentTypeId).HasColumnName("contentTypeId");

                entity.Property(e => e.KeepAllVersionsNewerThanDays).HasColumnName("keepAllVersionsNewerThanDays");

                entity.Property(e => e.KeepLatestVersionPerDayForDays).HasColumnName("keepLatestVersionPerDayForDays");

                entity.Property(e => e.PreventCleanup).HasColumnName("preventCleanup");

                entity.Property(e => e.Updated)
                    .HasColumnType("datetime")
                    .HasColumnName("updated");

                entity.HasOne(d => d.ContentType)
                    .WithMany()
                    .HasPrincipalKey(p => p.NodeId)
                    .HasForeignKey(d => d.ContentTypeId)
                    .HasConstraintName("FK_umbracoContentVersionCleanupPolicy_cmsContentType_nodeId");
            });

            modelBuilder.Entity<UmbracoContentVersionCultureVariation>(entity =>
            {
                entity.ToTable("umbracoContentVersionCultureVariation");

                entity.HasIndex(e => e.LanguageId, "IX_umbracoContentVersionCultureVariation_LanguageId");

                entity.HasIndex(e => new { e.VersionId, e.LanguageId }, "IX_umbracoContentVersionCultureVariation_VersionId")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.AvailableUserId).HasColumnName("availableUserId");

                entity.Property(e => e.Date)
                    .HasColumnType("datetime")
                    .HasColumnName("date");

                entity.Property(e => e.LanguageId).HasColumnName("languageId");

                entity.Property(e => e.Name)
                    .HasMaxLength(255)
                    .HasColumnName("name");

                entity.Property(e => e.VersionId).HasColumnName("versionId");

                entity.HasOne(d => d.AvailableUser)
                    .WithMany(p => p.UmbracoContentVersionCultureVariations)
                    .HasForeignKey(d => d.AvailableUserId)
                    .HasConstraintName("FK_umbracoContentVersionCultureVariation_umbracoUser_id");

                entity.HasOne(d => d.Language)
                    .WithMany(p => p.UmbracoContentVersionCultureVariations)
                    .HasForeignKey(d => d.LanguageId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_umbracoContentVersionCultureVariation_umbracoLanguage_id");

                entity.HasOne(d => d.Version)
                    .WithMany(p => p.UmbracoContentVersionCultureVariations)
                    .HasForeignKey(d => d.VersionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_umbracoContentVersionCultureVariation_umbracoContentVersion_id");
            });

            modelBuilder.Entity<UmbracoCreatedPackageSchema>(entity =>
            {
                entity.ToTable("umbracoCreatedPackageSchema");

                entity.HasIndex(e => e.Name, "IX_umbracoCreatedPackageSchema_Name")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Name)
                    .HasMaxLength(255)
                    .HasColumnName("name");

                entity.Property(e => e.PackageId).HasColumnName("packageId");

                entity.Property(e => e.UpdateDate)
                    .HasColumnType("datetime")
                    .HasColumnName("updateDate")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<UmbracoDataType>(entity =>
            {
                entity.HasKey(e => e.NodeId);

                entity.ToTable("umbracoDataType");

                entity.Property(e => e.NodeId)
                    .ValueGeneratedNever()
                    .HasColumnName("nodeId");

                entity.Property(e => e.Config)
                    .HasColumnType("ntext")
                    .HasColumnName("config");

                entity.Property(e => e.DbType)
                    .HasMaxLength(50)
                    .HasColumnName("dbType");

                entity.Property(e => e.PropertyEditorAlias)
                    .HasMaxLength(255)
                    .HasColumnName("propertyEditorAlias");

                entity.HasOne(d => d.Node)
                    .WithOne(p => p.UmbracoDataType)
                    .HasForeignKey<UmbracoDataType>(d => d.NodeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_umbracoDataType_umbracoNode_id");
            });

            modelBuilder.Entity<UmbracoDocument>(entity =>
            {
                entity.HasKey(e => e.NodeId);

                entity.ToTable("umbracoDocument");

                entity.HasIndex(e => e.Published, "IX_umbracoDocument_Published");

                entity.Property(e => e.NodeId)
                    .ValueGeneratedNever()
                    .HasColumnName("nodeId");

                entity.Property(e => e.Edited).HasColumnName("edited");

                entity.Property(e => e.Published).HasColumnName("published");

                entity.HasOne(d => d.Node)
                    .WithOne(p => p.UmbracoDocument)
                    .HasForeignKey<UmbracoDocument>(d => d.NodeId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<UmbracoDocumentCultureVariation>(entity =>
            {
                entity.ToTable("umbracoDocumentCultureVariation");

                entity.HasIndex(e => e.LanguageId, "IX_umbracoDocumentCultureVariation_LanguageId");

                entity.HasIndex(e => new { e.NodeId, e.LanguageId }, "IX_umbracoDocumentCultureVariation_NodeId")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Available).HasColumnName("available");

                entity.Property(e => e.Edited).HasColumnName("edited");

                entity.Property(e => e.LanguageId).HasColumnName("languageId");

                entity.Property(e => e.Name)
                    .HasMaxLength(255)
                    .HasColumnName("name");

                entity.Property(e => e.NodeId).HasColumnName("nodeId");

                entity.Property(e => e.Published).HasColumnName("published");

                entity.HasOne(d => d.Language)
                    .WithMany(p => p.UmbracoDocumentCultureVariations)
                    .HasForeignKey(d => d.LanguageId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_umbracoDocumentCultureVariation_umbracoLanguage_id");

                entity.HasOne(d => d.Node)
                    .WithMany(p => p.UmbracoDocumentCultureVariations)
                    .HasForeignKey(d => d.NodeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_umbracoDocumentCultureVariation_umbracoNode_id");
            });

            modelBuilder.Entity<UmbracoDocumentVersion>(entity =>
            {
                entity.ToTable("umbracoDocumentVersion");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.Published).HasColumnName("published");

                entity.Property(e => e.TemplateId).HasColumnName("templateId");

                entity.HasOne(d => d.IdNavigation)
                    .WithOne(p => p.UmbracoDocumentVersion)
                    .HasForeignKey<UmbracoDocumentVersion>(d => d.Id)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.Template)
                    .WithMany(p => p.UmbracoDocumentVersions)
                    .HasPrincipalKey(p => p.NodeId)
                    .HasForeignKey(d => d.TemplateId)
                    .HasConstraintName("FK_umbracoDocumentVersion_cmsTemplate_nodeId");
            });

            modelBuilder.Entity<UmbracoDomain>(entity =>
            {
                entity.ToTable("umbracoDomain");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.DomainDefaultLanguage).HasColumnName("domainDefaultLanguage");

                entity.Property(e => e.DomainName)
                    .HasMaxLength(255)
                    .HasColumnName("domainName");

                entity.Property(e => e.DomainRootStructureId).HasColumnName("domainRootStructureID");

                entity.HasOne(d => d.DomainRootStructure)
                    .WithMany(p => p.UmbracoDomains)
                    .HasForeignKey(d => d.DomainRootStructureId)
                    .HasConstraintName("FK_umbracoDomain_umbracoNode_id");
            });

            modelBuilder.Entity<UmbracoExternalLogin>(entity =>
            {
                entity.ToTable("umbracoExternalLogin");

                entity.HasIndex(e => new { e.LoginProvider, e.UserOrMemberKey }, "IX_umbracoExternalLogin_LoginProvider")
                    .IsUnique();

                entity.HasIndex(e => new { e.LoginProvider, e.ProviderKey }, "IX_umbracoExternalLogin_ProviderKey");

                entity.HasIndex(e => e.UserOrMemberKey, "IX_umbracoExternalLogin_userOrMemberKey");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreateDate)
                    .HasColumnType("datetime")
                    .HasColumnName("createDate")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.LoginProvider)
                    .HasMaxLength(400)
                    .HasColumnName("loginProvider");

                entity.Property(e => e.ProviderKey)
                    .HasMaxLength(4000)
                    .HasColumnName("providerKey");

                entity.Property(e => e.UserData)
                    .HasColumnType("ntext")
                    .HasColumnName("userData");

                entity.Property(e => e.UserOrMemberKey).HasColumnName("userOrMemberKey");
            });

            modelBuilder.Entity<UmbracoExternalLoginToken>(entity =>
            {
                entity.ToTable("umbracoExternalLoginToken");

                entity.HasIndex(e => new { e.ExternalLoginId, e.Name }, "IX_umbracoExternalLoginToken_Name")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreateDate)
                    .HasColumnType("datetime")
                    .HasColumnName("createDate")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ExternalLoginId).HasColumnName("externalLoginId");

                entity.Property(e => e.Name)
                    .HasMaxLength(255)
                    .HasColumnName("name");

                entity.Property(e => e.Value).HasColumnName("value");

                entity.HasOne(d => d.ExternalLogin)
                    .WithMany(p => p.UmbracoExternalLoginTokens)
                    .HasForeignKey(d => d.ExternalLoginId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_umbracoExternalLoginToken_umbracoExternalLogin_id");
            });

            modelBuilder.Entity<UmbracoKeyValue>(entity =>
            {
                entity.HasKey(e => e.Key);

                entity.ToTable("umbracoKeyValue");

                entity.Property(e => e.Key)
                    .HasMaxLength(256)
                    .HasColumnName("key");

                entity.Property(e => e.Updated)
                    .HasColumnType("datetime")
                    .HasColumnName("updated")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Value)
                    .HasMaxLength(255)
                    .HasColumnName("value");
            });

            modelBuilder.Entity<UmbracoLanguage>(entity =>
            {
                entity.ToTable("umbracoLanguage");

                entity.HasIndex(e => e.FallbackLanguageId, "IX_umbracoLanguage_fallbackLanguageId");

                entity.HasIndex(e => e.LanguageIsocode, "IX_umbracoLanguage_languageISOCode")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.FallbackLanguageId).HasColumnName("fallbackLanguageId");

                entity.Property(e => e.IsDefaultVariantLang)
                    .IsRequired()
                    .HasColumnName("isDefaultVariantLang")
                    .HasDefaultValueSql("('0')");

                entity.Property(e => e.LanguageCultureName)
                    .HasMaxLength(100)
                    .HasColumnName("languageCultureName");

                entity.Property(e => e.LanguageIsocode)
                    .HasMaxLength(14)
                    .HasColumnName("languageISOCode");

                entity.Property(e => e.Mandatory)
                    .IsRequired()
                    .HasColumnName("mandatory")
                    .HasDefaultValueSql("('0')");

                entity.HasOne(d => d.FallbackLanguage)
                    .WithMany(p => p.InverseFallbackLanguage)
                    .HasForeignKey(d => d.FallbackLanguageId)
                    .HasConstraintName("FK_umbracoLanguage_umbracoLanguage_id");
            });

            modelBuilder.Entity<UmbracoLock>(entity =>
            {
                entity.ToTable("umbracoLock");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.Name)
                    .HasMaxLength(64)
                    .HasColumnName("name");

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<UmbracoLog>(entity =>
            {
                entity.ToTable("umbracoLog");

                entity.HasIndex(e => e.NodeId, "IX_umbracoLog");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Datestamp)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.EntityType)
                    .HasMaxLength(50)
                    .HasColumnName("entityType");

                entity.Property(e => e.LogComment)
                    .HasMaxLength(4000)
                    .HasColumnName("logComment");

                entity.Property(e => e.LogHeader)
                    .HasMaxLength(50)
                    .HasColumnName("logHeader");

                entity.Property(e => e.Parameters)
                    .HasMaxLength(500)
                    .HasColumnName("parameters");

                entity.Property(e => e.UserId).HasColumnName("userId");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UmbracoLogs)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_umbracoLog_umbracoUser_id");
            });

            modelBuilder.Entity<UmbracoLogViewerQuery>(entity =>
            {
                entity.ToTable("umbracoLogViewerQuery");

                entity.HasIndex(e => e.Name, "IX_LogViewerQuery_name")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Name)
                    .HasMaxLength(255)
                    .HasColumnName("name");

                entity.Property(e => e.Query)
                    .HasMaxLength(255)
                    .HasColumnName("query");
            });

            modelBuilder.Entity<UmbracoMediaVersion>(entity =>
            {
                entity.ToTable("umbracoMediaVersion");

                entity.HasIndex(e => new { e.Id, e.Path }, "IX_umbracoMediaVersion")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.Path)
                    .HasMaxLength(255)
                    .HasColumnName("path");

                entity.HasOne(d => d.IdNavigation)
                    .WithOne(p => p.UmbracoMediaVersion)
                    .HasForeignKey<UmbracoMediaVersion>(d => d.Id)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<UmbracoNode>(entity =>
            {
                entity.ToTable("umbracoNode");

                entity.HasIndex(e => new { e.Level, e.ParentId, e.SortOrder, e.NodeObjectType, e.Trashed }, "IX_umbracoNode_Level");

                entity.HasIndex(e => new { e.NodeObjectType, e.Trashed }, "IX_umbracoNode_ObjectType");

                entity.HasIndex(e => e.ParentId, "IX_umbracoNode_ParentId");

                entity.HasIndex(e => e.Path, "IX_umbracoNode_Path");

                entity.HasIndex(e => e.Trashed, "IX_umbracoNode_Trashed");

                entity.HasIndex(e => e.UniqueId, "IX_umbracoNode_UniqueId")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreateDate)
                    .HasColumnType("datetime")
                    .HasColumnName("createDate")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Level).HasColumnName("level");

                entity.Property(e => e.NodeObjectType).HasColumnName("nodeObjectType");

                entity.Property(e => e.NodeUser).HasColumnName("nodeUser");

                entity.Property(e => e.ParentId).HasColumnName("parentId");

                entity.Property(e => e.Path)
                    .HasMaxLength(150)
                    .HasColumnName("path");

                entity.Property(e => e.SortOrder).HasColumnName("sortOrder");

                entity.Property(e => e.Text)
                    .HasMaxLength(255)
                    .HasColumnName("text");

                entity.Property(e => e.Trashed)
                    .IsRequired()
                    .HasColumnName("trashed")
                    .HasDefaultValueSql("('0')");

                entity.Property(e => e.UniqueId)
                    .HasColumnName("uniqueId")
                    .HasDefaultValueSql("(newid())");

                entity.HasOne(d => d.NodeUserNavigation)
                    .WithMany(p => p.UmbracoNodes)
                    .HasForeignKey(d => d.NodeUser)
                    .HasConstraintName("FK_umbracoNode_umbracoUser_id");

                entity.HasOne(d => d.Parent)
                    .WithMany(p => p.InverseParent)
                    .HasForeignKey(d => d.ParentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_umbracoNode_umbracoNode_id");

                entity.HasMany(d => d.ChildContentTypes)
                    .WithMany(p => p.ParentContentTypes)
                    .UsingEntity<Dictionary<string, object>>(
                        "CmsContentType2ContentType",
                        l => l.HasOne<UmbracoNode>().WithMany().HasForeignKey("ChildContentTypeId").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_cmsContentType2ContentType_umbracoNode_child"),
                        r => r.HasOne<UmbracoNode>().WithMany().HasForeignKey("ParentContentTypeId").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_cmsContentType2ContentType_umbracoNode_parent"),
                        j =>
                        {
                            j.HasKey("ParentContentTypeId", "ChildContentTypeId");

                            j.ToTable("cmsContentType2ContentType");

                            j.IndexerProperty<int>("ParentContentTypeId").HasColumnName("parentContentTypeId");

                            j.IndexerProperty<int>("ChildContentTypeId").HasColumnName("childContentTypeId");
                        });

                entity.HasMany(d => d.ParentContentTypes)
                    .WithMany(p => p.ChildContentTypes)
                    .UsingEntity<Dictionary<string, object>>(
                        "CmsContentType2ContentType",
                        l => l.HasOne<UmbracoNode>().WithMany().HasForeignKey("ParentContentTypeId").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_cmsContentType2ContentType_umbracoNode_parent"),
                        r => r.HasOne<UmbracoNode>().WithMany().HasForeignKey("ChildContentTypeId").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_cmsContentType2ContentType_umbracoNode_child"),
                        j =>
                        {
                            j.HasKey("ParentContentTypeId", "ChildContentTypeId");

                            j.ToTable("cmsContentType2ContentType");

                            j.IndexerProperty<int>("ParentContentTypeId").HasColumnName("parentContentTypeId");

                            j.IndexerProperty<int>("ChildContentTypeId").HasColumnName("childContentTypeId");
                        });
            });

            modelBuilder.Entity<UmbracoPropertyDatum>(entity =>
            {
                entity.ToTable("umbracoPropertyData");

                entity.HasIndex(e => e.LanguageId, "IX_umbracoPropertyData_LanguageId");

                entity.HasIndex(e => e.PropertyTypeId, "IX_umbracoPropertyData_PropertyTypeId");

                entity.HasIndex(e => e.Segment, "IX_umbracoPropertyData_Segment");

                entity.HasIndex(e => new { e.VersionId, e.PropertyTypeId, e.LanguageId, e.Segment }, "IX_umbracoPropertyData_VersionId")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.DateValue)
                    .HasColumnType("datetime")
                    .HasColumnName("dateValue");

                entity.Property(e => e.DecimalValue)
                    .HasColumnType("decimal(38, 6)")
                    .HasColumnName("decimalValue");

                entity.Property(e => e.IntValue).HasColumnName("intValue");

                entity.Property(e => e.LanguageId).HasColumnName("languageId");

                entity.Property(e => e.PropertyTypeId).HasColumnName("propertyTypeId");

                entity.Property(e => e.Segment)
                    .HasMaxLength(256)
                    .HasColumnName("segment");

                entity.Property(e => e.TextValue)
                    .HasColumnType("ntext")
                    .HasColumnName("textValue");

                entity.Property(e => e.VarcharValue)
                    .HasMaxLength(512)
                    .HasColumnName("varcharValue");

                entity.Property(e => e.VersionId).HasColumnName("versionId");

                entity.HasOne(d => d.Language)
                    .WithMany(p => p.UmbracoPropertyData)
                    .HasForeignKey(d => d.LanguageId)
                    .HasConstraintName("FK_umbracoPropertyData_umbracoLanguage_id");

                entity.HasOne(d => d.PropertyType)
                    .WithMany(p => p.UmbracoPropertyData)
                    .HasForeignKey(d => d.PropertyTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_umbracoPropertyData_cmsPropertyType_id");

                entity.HasOne(d => d.Version)
                    .WithMany(p => p.UmbracoPropertyData)
                    .HasForeignKey(d => d.VersionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_umbracoPropertyData_umbracoContentVersion_id");
            });

            modelBuilder.Entity<UmbracoRedirectUrl>(entity =>
            {
                entity.ToTable("umbracoRedirectUrl");

                entity.HasIndex(e => new { e.UrlHash, e.ContentKey, e.Culture, e.CreateDateUtc }, "IX_umbracoRedirectUrl")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.ContentKey).HasColumnName("contentKey");

                entity.Property(e => e.CreateDateUtc)
                    .HasColumnType("datetime")
                    .HasColumnName("createDateUtc");

                entity.Property(e => e.Culture)
                    .HasMaxLength(255)
                    .HasColumnName("culture");

                entity.Property(e => e.Url)
                    .HasMaxLength(255)
                    .HasColumnName("url");

                entity.Property(e => e.UrlHash)
                    .HasMaxLength(40)
                    .HasColumnName("urlHash");

                entity.HasOne(d => d.ContentKeyNavigation)
                    .WithMany(p => p.UmbracoRedirectUrls)
                    .HasPrincipalKey(p => p.UniqueId)
                    .HasForeignKey(d => d.ContentKey)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_umbracoRedirectUrl_umbracoNode_uniqueID");
            });

            modelBuilder.Entity<UmbracoRelation>(entity =>
            {
                entity.ToTable("umbracoRelation");

                entity.HasIndex(e => new { e.ParentId, e.ChildId, e.RelType }, "IX_umbracoRelation_parentChildType")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.ChildId).HasColumnName("childId");

                entity.Property(e => e.Comment)
                    .HasMaxLength(1000)
                    .HasColumnName("comment");

                entity.Property(e => e.Datetime)
                    .HasColumnType("datetime")
                    .HasColumnName("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ParentId).HasColumnName("parentId");

                entity.Property(e => e.RelType).HasColumnName("relType");

                entity.HasOne(d => d.Child)
                    .WithMany(p => p.UmbracoRelationChildren)
                    .HasForeignKey(d => d.ChildId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_umbracoRelation_umbracoNode1");

                entity.HasOne(d => d.Parent)
                    .WithMany(p => p.UmbracoRelationParents)
                    .HasForeignKey(d => d.ParentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_umbracoRelation_umbracoNode");

                entity.HasOne(d => d.RelTypeNavigation)
                    .WithMany(p => p.UmbracoRelations)
                    .HasForeignKey(d => d.RelType)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_umbracoRelation_umbracoRelationType_id");
            });

            modelBuilder.Entity<UmbracoRelationType>(entity =>
            {
                entity.ToTable("umbracoRelationType");

                entity.HasIndex(e => e.TypeUniqueId, "IX_umbracoRelationType_UniqueId")
                    .IsUnique();

                entity.HasIndex(e => e.Alias, "IX_umbracoRelationType_alias")
                    .IsUnique();

                entity.HasIndex(e => e.Name, "IX_umbracoRelationType_name")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Alias)
                    .HasMaxLength(100)
                    .HasColumnName("alias");

                entity.Property(e => e.ChildObjectType).HasColumnName("childObjectType");

                entity.Property(e => e.Dual).HasColumnName("dual");

                entity.Property(e => e.IsDependency)
                    .IsRequired()
                    .HasColumnName("isDependency")
                    .HasDefaultValueSql("('0')");

                entity.Property(e => e.Name)
                    .HasMaxLength(255)
                    .HasColumnName("name");

                entity.Property(e => e.ParentObjectType).HasColumnName("parentObjectType");

                entity.Property(e => e.TypeUniqueId).HasColumnName("typeUniqueId");
            });

            modelBuilder.Entity<UmbracoServer>(entity =>
            {
                entity.ToTable("umbracoServer");

                entity.HasIndex(e => e.ComputerName, "IX_computerName")
                    .IsUnique();

                entity.HasIndex(e => e.IsActive, "IX_umbracoServer_isActive");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Address)
                    .HasMaxLength(500)
                    .HasColumnName("address");

                entity.Property(e => e.ComputerName)
                    .HasMaxLength(255)
                    .HasColumnName("computerName");

                entity.Property(e => e.IsActive).HasColumnName("isActive");

                entity.Property(e => e.IsSchedulingPublisher).HasColumnName("isSchedulingPublisher");

                entity.Property(e => e.LastNotifiedDate)
                    .HasColumnType("datetime")
                    .HasColumnName("lastNotifiedDate");

                entity.Property(e => e.RegisteredDate)
                    .HasColumnType("datetime")
                    .HasColumnName("registeredDate")
                    .HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<UmbracoTwoFactorLogin>(entity =>
            {
                entity.ToTable("umbracoTwoFactorLogin");

                entity.HasIndex(e => new { e.ProviderName, e.UserOrMemberKey }, "IX_umbracoTwoFactorLogin_ProviderName")
                    .IsUnique();

                entity.HasIndex(e => e.UserOrMemberKey, "IX_umbracoTwoFactorLogin_userOrMemberKey");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.ProviderName)
                    .HasMaxLength(400)
                    .HasColumnName("providerName");

                entity.Property(e => e.Secret)
                    .HasMaxLength(400)
                    .HasColumnName("secret");

                entity.Property(e => e.UserOrMemberKey).HasColumnName("userOrMemberKey");
            });

            modelBuilder.Entity<UmbracoUser>(entity =>
            {
                entity.ToTable("umbracoUser");

                entity.HasIndex(e => e.UserLogin, "IX_umbracoUser_userLogin");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Avatar)
                    .HasMaxLength(500)
                    .HasColumnName("avatar");

                entity.Property(e => e.CreateDate)
                    .HasColumnType("datetime")
                    .HasColumnName("createDate")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.EmailConfirmedDate)
                    .HasColumnType("datetime")
                    .HasColumnName("emailConfirmedDate");

                entity.Property(e => e.FailedLoginAttempts).HasColumnName("failedLoginAttempts");

                entity.Property(e => e.InvitedDate)
                    .HasColumnType("datetime")
                    .HasColumnName("invitedDate");

                entity.Property(e => e.LastLockoutDate)
                    .HasColumnType("datetime")
                    .HasColumnName("lastLockoutDate");

                entity.Property(e => e.LastLoginDate)
                    .HasColumnType("datetime")
                    .HasColumnName("lastLoginDate");

                entity.Property(e => e.LastPasswordChangeDate)
                    .HasColumnType("datetime")
                    .HasColumnName("lastPasswordChangeDate");

                entity.Property(e => e.PasswordConfig)
                    .HasMaxLength(500)
                    .HasColumnName("passwordConfig");

                entity.Property(e => e.SecurityStampToken)
                    .HasMaxLength(255)
                    .HasColumnName("securityStampToken");

                entity.Property(e => e.TourData)
                    .HasColumnType("ntext")
                    .HasColumnName("tourData");

                entity.Property(e => e.UpdateDate)
                    .HasColumnType("datetime")
                    .HasColumnName("updateDate")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.UserDisabled)
                    .IsRequired()
                    .HasColumnName("userDisabled")
                    .HasDefaultValueSql("('0')");

                entity.Property(e => e.UserEmail)
                    .HasMaxLength(255)
                    .HasColumnName("userEmail");

                entity.Property(e => e.UserLanguage)
                    .HasMaxLength(10)
                    .HasColumnName("userLanguage");

                entity.Property(e => e.UserLogin)
                    .HasMaxLength(125)
                    .HasColumnName("userLogin");

                entity.Property(e => e.UserName)
                    .HasMaxLength(255)
                    .HasColumnName("userName");

                entity.Property(e => e.UserNoConsole)
                    .IsRequired()
                    .HasColumnName("userNoConsole")
                    .HasDefaultValueSql("('0')");

                entity.Property(e => e.UserPassword)
                    .HasMaxLength(500)
                    .HasColumnName("userPassword");

                entity.HasMany(d => d.UserGroups)
                    .WithMany(p => p.Users)
                    .UsingEntity<Dictionary<string, object>>(
                        "UmbracoUser2UserGroup",
                        l => l.HasOne<UmbracoUserGroup>().WithMany().HasForeignKey("UserGroupId").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_umbracoUser2UserGroup_umbracoUserGroup_id"),
                        r => r.HasOne<UmbracoUser>().WithMany().HasForeignKey("UserId").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_umbracoUser2UserGroup_umbracoUser_id"),
                        j =>
                        {
                            j.HasKey("UserId", "UserGroupId").HasName("PK_user2userGroup");

                            j.ToTable("umbracoUser2UserGroup");

                            j.IndexerProperty<int>("UserId").HasColumnName("userId");

                            j.IndexerProperty<int>("UserGroupId").HasColumnName("userGroupId");
                        });
            });

            modelBuilder.Entity<UmbracoUser2NodeNotify>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.NodeId, e.Action });

                entity.ToTable("umbracoUser2NodeNotify");

                entity.Property(e => e.UserId).HasColumnName("userId");

                entity.Property(e => e.NodeId).HasColumnName("nodeId");

                entity.Property(e => e.Action)
                    .HasMaxLength(1)
                    .HasColumnName("action")
                    .IsFixedLength();

                entity.HasOne(d => d.Node)
                    .WithMany(p => p.UmbracoUser2NodeNotifies)
                    .HasForeignKey(d => d.NodeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_umbracoUser2NodeNotify_umbracoNode_id");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UmbracoUser2NodeNotifies)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_umbracoUser2NodeNotify_umbracoUser_id");
            });

            modelBuilder.Entity<UmbracoUserGroup>(entity =>
            {
                entity.ToTable("umbracoUserGroup");

                entity.HasIndex(e => e.UserGroupAlias, "IX_umbracoUserGroup_userGroupAlias")
                    .IsUnique();

                entity.HasIndex(e => e.UserGroupName, "IX_umbracoUserGroup_userGroupName")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreateDate)
                    .HasColumnType("datetime")
                    .HasColumnName("createDate")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.HasAccessToAllLanguages).HasColumnName("hasAccessToAllLanguages");

                entity.Property(e => e.Icon)
                    .HasMaxLength(255)
                    .HasColumnName("icon");

                entity.Property(e => e.StartContentId).HasColumnName("startContentId");

                entity.Property(e => e.StartMediaId).HasColumnName("startMediaId");

                entity.Property(e => e.UpdateDate)
                    .HasColumnType("datetime")
                    .HasColumnName("updateDate")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.UserGroupAlias)
                    .HasMaxLength(200)
                    .HasColumnName("userGroupAlias");

                entity.Property(e => e.UserGroupDefaultPermissions)
                    .HasMaxLength(50)
                    .HasColumnName("userGroupDefaultPermissions");

                entity.Property(e => e.UserGroupName)
                    .HasMaxLength(200)
                    .HasColumnName("userGroupName");

                entity.HasOne(d => d.StartContent)
                    .WithMany(p => p.UmbracoUserGroupStartContents)
                    .HasForeignKey(d => d.StartContentId)
                    .HasConstraintName("FK_startContentId_umbracoNode_id");

                entity.HasOne(d => d.StartMedia)
                    .WithMany(p => p.UmbracoUserGroupStartMedia)
                    .HasForeignKey(d => d.StartMediaId)
                    .HasConstraintName("FK_startMediaId_umbracoNode_id");

                entity.HasMany(d => d.Languages)
                    .WithMany(p => p.UserGroups)
                    .UsingEntity<Dictionary<string, object>>(
                        "UmbracoUserGroup2Language",
                        l => l.HasOne<UmbracoLanguage>().WithMany().HasForeignKey("LanguageId").HasConstraintName("FK_umbracoUserGroup2Language_umbracoLanguage_id"),
                        r => r.HasOne<UmbracoUserGroup>().WithMany().HasForeignKey("UserGroupId").HasConstraintName("FK_umbracoUserGroup2Language_umbracoUserGroup_id"),
                        j =>
                        {
                            j.HasKey("UserGroupId", "LanguageId").HasName("PK_userGroup2language");

                            j.ToTable("umbracoUserGroup2Language");

                            j.IndexerProperty<int>("UserGroupId").HasColumnName("userGroupId");

                            j.IndexerProperty<int>("LanguageId").HasColumnName("languageId");
                        });

                entity.HasMany(d => d.Nodes)
                    .WithMany(p => p.UserGroups)
                    .UsingEntity<Dictionary<string, object>>(
                        "UmbracoUserGroup2Node",
                        l => l.HasOne<UmbracoNode>().WithMany().HasForeignKey("NodeId").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_umbracoUserGroup2Node_umbracoNode_id"),
                        r => r.HasOne<UmbracoUserGroup>().WithMany().HasForeignKey("UserGroupId").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_umbracoUserGroup2Node_umbracoUserGroup_id"),
                        j =>
                        {
                            j.HasKey("UserGroupId", "NodeId");

                            j.ToTable("umbracoUserGroup2Node");

                            j.HasIndex(new[] { "NodeId" }, "IX_umbracoUserGroup2Node_nodeId");

                            j.IndexerProperty<int>("UserGroupId").HasColumnName("userGroupId");

                            j.IndexerProperty<int>("NodeId").HasColumnName("nodeId");
                        });
            });

            modelBuilder.Entity<UmbracoUserGroup2App>(entity =>
            {
                entity.HasKey(e => new { e.UserGroupId, e.App })
                    .HasName("PK_userGroup2App");

                entity.ToTable("umbracoUserGroup2App");

                entity.Property(e => e.UserGroupId).HasColumnName("userGroupId");

                entity.Property(e => e.App)
                    .HasMaxLength(50)
                    .HasColumnName("app");

                entity.HasOne(d => d.UserGroup)
                    .WithMany(p => p.UmbracoUserGroup2Apps)
                    .HasForeignKey(d => d.UserGroupId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_umbracoUserGroup2App_umbracoUserGroup_id");
            });

            modelBuilder.Entity<UmbracoUserGroup2NodePermission>(entity =>
            {
                entity.HasKey(e => new { e.UserGroupId, e.NodeId, e.Permission });

                entity.ToTable("umbracoUserGroup2NodePermission");

                entity.HasIndex(e => e.NodeId, "IX_umbracoUser2NodePermission_nodeId");

                entity.Property(e => e.UserGroupId).HasColumnName("userGroupId");

                entity.Property(e => e.NodeId).HasColumnName("nodeId");

                entity.Property(e => e.Permission)
                    .HasMaxLength(255)
                    .HasColumnName("permission");

                entity.HasOne(d => d.Node)
                    .WithMany(p => p.UmbracoUserGroup2NodePermissions)
                    .HasForeignKey(d => d.NodeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_umbracoUserGroup2NodePermission_umbracoNode_id");

                entity.HasOne(d => d.UserGroup)
                    .WithMany(p => p.UmbracoUserGroup2NodePermissions)
                    .HasForeignKey(d => d.UserGroupId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_umbracoUserGroup2NodePermission_umbracoUserGroup_id");
            });

            modelBuilder.Entity<UmbracoUserLogin>(entity =>
            {
                entity.HasKey(e => e.SessionId);

                entity.ToTable("umbracoUserLogin");

                entity.HasIndex(e => e.LastValidatedUtc, "IX_umbracoUserLogin_lastValidatedUtc");

                entity.Property(e => e.SessionId)
                    .ValueGeneratedNever()
                    .HasColumnName("sessionId");

                entity.Property(e => e.IpAddress)
                    .HasMaxLength(255)
                    .HasColumnName("ipAddress");

                entity.Property(e => e.LastValidatedUtc)
                    .HasColumnType("datetime")
                    .HasColumnName("lastValidatedUtc");

                entity.Property(e => e.LoggedInUtc)
                    .HasColumnType("datetime")
                    .HasColumnName("loggedInUtc");

                entity.Property(e => e.LoggedOutUtc)
                    .HasColumnType("datetime")
                    .HasColumnName("loggedOutUtc");

                entity.Property(e => e.UserId).HasColumnName("userId");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UmbracoUserLogins)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_umbracoUserLogin_umbracoUser_id");
            });

            modelBuilder.Entity<UmbracoUserStartNode>(entity =>
            {
                entity.ToTable("umbracoUserStartNode");

                entity.HasIndex(e => new { e.StartNodeType, e.StartNode, e.UserId }, "IX_umbracoUserStartNode_startNodeType")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.StartNode).HasColumnName("startNode");

                entity.Property(e => e.StartNodeType).HasColumnName("startNodeType");

                entity.Property(e => e.UserId).HasColumnName("userId");

                entity.HasOne(d => d.StartNodeNavigation)
                    .WithMany(p => p.UmbracoUserStartNodes)
                    .HasForeignKey(d => d.StartNode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_umbracoUserStartNode_umbracoNode_id");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UmbracoUserStartNodes)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_umbracoUserStartNode_umbracoUser_id");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
