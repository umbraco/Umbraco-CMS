import {ApiHelpers} from "./ApiHelpers";
import {UserGroupBuilder} from "../builders";
import {ConstantHelper} from "./ConstantHelper";

export class UserGroupApiHelper {
  api: ApiHelpers

  constructor(api: ApiHelpers) {
    this.api = api;
  }

  async ensureNameNotExists(name: string) {
    const json = await this.getAll();

    for (const sb of json.items) {
      if (sb.name === name) {
        if (sb.id !== null) {
          return await this.api.delete(this.api.baseUrl + '/umbraco/management/api/v1/user-group/' + sb.id);
        }
      }
    }
    return null;
  }

  async doesExist(id: string) {
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/user-group/' + id);
    return response.status() === 200;
  }

  async create(userGroupData) {
    const response = await this.api.post(this.api.baseUrl + '/umbraco/management/api/v1/user-group', userGroupData);
    // Returns the id of the userGroup
    return response.headers().location.split("/").pop();
  }

  async getByName(name: string) {
    const json = await this.getAll();

    for (const sb of json.items) {
      if (sb.name === name) {
        if (sb.id !== null) {
          const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/user-group/' + sb.id);
          return await response.json();
        }
      }
    }
    return null;
  }

  async get(id: string) {
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/user-group/' + id);
    const json = await response.json();

    if (json !== null) {
      return json;
    }
    return null;
  }

  async getAll() {
    const response = await this.api.get(this.api.baseUrl + '/umbraco/management/api/v1/user-group?skip=0&take=10000');
    const json = await response.json();

    if (json !== null) {
      return json;
    }
    return null;
  }

  async update(id: string, userGroup) {
    const response = await this.api.put(this.api.baseUrl + '/umbraco/management/api/v1/user-group/' + id, userGroup);
    return response.text();
  }

  async doesNameExist(name: string) {
    const json = await this.getAll();

    for (const sb of json.items) {
      if (sb.name === name) {
        return true;
      }
    }
    return false;
  }

  async doesUserGroupContainLanguage(userGroupName: string, languageName: string) {
    const userGroup = await this.getByName(userGroupName);
    return userGroup.languages.includes(languageName);
  }

  async doesUserGroupContainAccessToAllLanguages(userGroupName: string) {
    const userGroup = await this.getByName(userGroupName);
    return userGroup.hasAccessToAllLanguages;
  }

  async doesUserGroupContainDocumentRootAccess(userGroupName: string) {
    const userGroup = await this.getByName(userGroupName);
    return userGroup.documentRootAccess;
  }

  async doesUserGroupContainMediaRootAccess(userGroupName: string) {
    const userGroup = await this.getByName(userGroupName);
    return userGroup.mediaRootAccess;
  }

  async delete(id: string) {
    return await this.api.delete(this.api.baseUrl + '/umbraco/management/api/v1/user-group/' + id);
  }

  async createEmptyUserGroup(name: string, description: string = '') {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .withDescription(description)
      .build();

    return await this.create(userGroup);
  }

  async createSimpleUserGroupWithContentSection(name: string, description: string = '') {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .withDescription(description)
      .addSection(ConstantHelper.sectionAliases.content)
      .addFallbackPermission()
        .withReadDocumentPermission(true)
        .done()
      .build();

    return await this.create(userGroup);
  }

  async createSimpleUserGroupWithMediaSection(name: string) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection(ConstantHelper.sectionAliases.media)
      .addFallbackPermission()
        .withReadDocumentPermission(true)
        .done()
      .build();

    return await this.create(userGroup);
  }

  async createUserGroupWithDocumentAccess(name: string) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .withDocumentRootAccess(true)
      .build();

    return await this.create(userGroup);
  }

  async createUserGroupWithDocumentStartNode(name: string, startNodeId: string) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection(ConstantHelper.sectionAliases.content)
      .withDocumentRootAccess(false)
      .withDocumentStartNodeId(startNodeId)
      .addFallbackPermission()
        .withReadDocumentPermission(true)
        .done()
      .build();

    return await this.create(userGroup);
  }

  async createUserGroupWithMediaStartNode(name: string, startNodeId: string) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection(ConstantHelper.sectionAliases.media)
      .withMediaRootAccess(false)
      .withMediaStartNodeId(startNodeId)
      .addFallbackPermission()
        .withReadDocumentPermission(true)
        .done()
      .build();

    return await this.create(userGroup);
  }

  async createUserGroupWithLanguage(name: string, languageName: string) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addLanguage(languageName)
      .build();

    return await this.create(userGroup);
  }

  async createUserGroupWithLanguageAndContentSection(name: string, languageName: string) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection(ConstantHelper.sectionAliases.content)
      .addLanguage(languageName)
      .withDocumentRootAccess(true)
      .addFallbackPermission()
        .withReadDocumentPermission(true)
        .withUpdateDocumentPermission(true)
        .withReadPropertyValueDocumentPermission(true)
        .withWritePropertyValueDocumentPermission(true)
        .done()
      .build();

    return await this.create(userGroup);
  }

  async createUserGroupWithMemberSection(name: string) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection(ConstantHelper.sectionAliases.members)
      .build();

    return await this.create(userGroup);
  }

  async createUserGroupWithPermissionsForSpecificDocumentWithRead(name: string, documentId: string) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addPermissions()
        .addDocumentPermission()
          .withDocumentId(documentId)
          .addVerbs()
            .withReadDocumentPermission(true)
            .done()
          .done()
        .done()
      .build();

    return await this.create(userGroup);
  }

  async createUserGroupWithReadDocumentPermission(name: string, enabled: boolean = true) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection(ConstantHelper.sectionAliases.content)
      .withDocumentRootAccess(true)
      .addFallbackPermission()
        .withReadDocumentPermission(enabled)
        .done()
      .build();

    return await this.create(userGroup);
  }

  async createUserGroupWithCreateDocumentBlueprintPermission(name: string, enabled: boolean = true) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection(ConstantHelper.sectionAliases.content)
      .withDocumentRootAccess(true)
      .addFallbackPermission()
        .withCreateDocumentBlueprintPermission(enabled)
        .withReadDocumentPermission(true)
        .done()
      .build();

    return await this.create(userGroup);
  }

  async createUserGroupWithDeleteDocumentPermission(name: string, enabled: boolean = true) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection(ConstantHelper.sectionAliases.content)
      .withDocumentRootAccess(true)
      .addFallbackPermission()
        .withDeleteDocumentPermission(enabled)
        .withReadDocumentPermission(true)
        .done()
      .build();

    return await this.create(userGroup);
  }

  async createUserGroupWithCreateDocumentPermission(name: string, enabled: boolean = true) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection(ConstantHelper.sectionAliases.content)
      .withDocumentRootAccess(true)
      .addFallbackPermission()
        .withCreateDocumentPermission(enabled)
        .withReadDocumentPermission(true)
        .done()
      .build();

    return await this.create(userGroup);
  }

  async createUserGroupWithNotificationsDocumentPermission(name: string, enabled: boolean = true) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection(ConstantHelper.sectionAliases.content)
      .withDocumentRootAccess(true)
      .addFallbackPermission()
        .withNotificationsDocumentPermission(enabled)
        .withReadDocumentPermission(true)
        .done()
      .build();

    return await this.create(userGroup);
  }

  async createUserGroupWithPublishDocumentPermission(name: string, enabled: boolean = true) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection(ConstantHelper.sectionAliases.content)
      .withDocumentRootAccess(true)
      .addFallbackPermission()
        .withPublishDocumentPermission(enabled)
        .withReadDocumentPermission(true)
        .done()
      .build();

    return await this.create(userGroup);
  }

  async createUserGroupWithSetPermissionsDocumentPermission(name: string, enabled: boolean = true) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection(ConstantHelper.sectionAliases.content)
      .withDocumentRootAccess(true)
      .addFallbackPermission()
        .withSetPermissionsDocumentPermission(enabled)
        .withReadDocumentPermission(true)
        .done()
      .build();

    return await this.create(userGroup);
  }

  async createUserGroupWithUnpublishDocumentPermission(name: string, enabled: boolean = true) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection(ConstantHelper.sectionAliases.content)
      .withDocumentRootAccess(true)
      .addFallbackPermission()
        .withUnpublishDocumentPermission(enabled)
        .withReadDocumentPermission(true)
        .done()
      .build();

    return await this.create(userGroup);
  }

  async createUserGroupWithUpdateDocumentPermission(name: string, enabled: boolean = true) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection(ConstantHelper.sectionAliases.content)
      .withDocumentRootAccess(true)
      .addFallbackPermission()
        .withUpdateDocumentPermission(enabled)
        .withReadDocumentPermission(true)
        .done()
      .build();

    return await this.create(userGroup);
  }

  async createUserGroupWithDuplicateDocumentPermission(name: string, enabled: boolean = true) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection(ConstantHelper.sectionAliases.content)
      .withDocumentRootAccess(true)
      .addFallbackPermission()
        .withDuplicateDocumentPermission(enabled)
        .withCreateDocumentPermission(enabled)
        .withReadDocumentPermission(true)
        .done()
      .build();

    return await this.create(userGroup);
  }

  async createUserGroupWithMoveToDocumentPermission(name: string, enabled: boolean = true) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection(ConstantHelper.sectionAliases.content)
      .withDocumentRootAccess(true)
      .addFallbackPermission()
        .withMoveToDocumentPermission(enabled)
        .withCreateDocumentPermission(enabled)
        .withReadDocumentPermission(true)
        .done()
      .build();

    return await this.create(userGroup);
  }

  async createUserGroupWithSortChildrenDocumentPermission(name: string, enabled: boolean = true) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection(ConstantHelper.sectionAliases.content)
      .withDocumentRootAccess(true)
      .addFallbackPermission()
        .withSortChildrenDocumentPermission(enabled)
        .withReadDocumentPermission(true)
        .done()
      .build();

    return await this.create(userGroup);
  }

  async createUserGroupWithCultureAndHostnamesDocumentPermission(name: string, enabled: boolean = true) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection(ConstantHelper.sectionAliases.content)
      .withDocumentRootAccess(true)
      .addFallbackPermission()
        .withCultureAndHostnamesDocumentPermission(enabled)
        .withReadDocumentPermission(true)
        .done()
      .build();

    return await this.create(userGroup);
  }

  async createUserGroupWithPublicAccessDocumentPermission(name: string, enabled: boolean = true) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection(ConstantHelper.sectionAliases.content)
      .addSection(ConstantHelper.sectionAliases.members)
      .withDocumentRootAccess(true)
      .addFallbackPermission()
        .withPublicAccessDocumentPermission(enabled)
        .withReadDocumentPermission(true)
        .done()
      .build();

    return await this.create(userGroup);
  }

  async createUserGroupWithRollbackDocumentPermission(name: string, enabled: boolean = true) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection(ConstantHelper.sectionAliases.content)
      .withDocumentRootAccess(true)
      .addFallbackPermission()
        .withRollbackDocumentPermission(enabled)
        .withReadDocumentPermission(true)
        .withReadPropertyValueDocumentPermission(true)
        .done()
      .build();

    return await this.create(userGroup);
  }

  async createUserGroupWithDeleteDocumentPermissionAndCreateDocumentPermission(name: string, deleteEnabled: boolean = true, createEnabled: boolean = true) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection(ConstantHelper.sectionAliases.content)
      .withDocumentRootAccess(true)
      .addFallbackPermission()
        .withDeleteDocumentPermission(deleteEnabled)
        .withCreateDocumentPermission(createEnabled)
        .withReadDocumentPermission(true)
        .done()
      .build();

    return await this.create(userGroup);
  }

  async doesUserGroupContainContentStartNodeId(userGroupName: string, documentStartNodeId: string) {
    const userGroup = await this.getByName(userGroupName);
    if (userGroup.documentStartNode === null) {
      return false;
    }
    return userGroup.documentStartNode.id.includes(documentStartNodeId);
  }

  async doesUserGroupContainMediaStartNodeId(userGroupName: string, mediaStartNodeId: string) {
    const userGroup = await this.getByName(userGroupName);
    if (userGroup.mediaStartNode === null) {
      return false;
    }
    return userGroup.mediaStartNode.id.includes(mediaStartNodeId);
  }

  async doesUserGroupContainGranularPermissionsForDocument(userGroupName: string, documentId: string, granularPermissions : string[]) {
    const userGroup = await this.getByName(userGroupName);
    for (const permission of userGroup.permissions) {
      if (permission.document.id === documentId) {
        for (const verb of permission.verbs) {
          if (!granularPermissions.includes(verb)) {
            return false;
          }
        }
        return true;
      }
    }
    return false;
  }

  async doesUserGroupHaveFallbackPermissions(userGroupName: string, permissions: string[]) {
    const userGroup = await this.getByName(userGroupName);
    const fallbackPermissions = userGroup.fallbackPermissions;
    if (permissions.length !== fallbackPermissions.length) {
      return false;
    }
    return permissions.every(item => fallbackPermissions.includes(item));
  }

  async convertApiDocumentPermissionsToUiDocumentPermissions(apiPermissions: string[]) {
    return apiPermissions.map(permission => {
      for (const key in ConstantHelper.userGroupDocumentPermissionsSettings) {
          if (ConstantHelper.userGroupDocumentPermissionsSettings[key][2].toLowerCase() === permission.toLowerCase()) {
              return ConstantHelper.userGroupDocumentPermissionsSettings[key][0];
          }
      }
      return null;
    });
  }

  async convertApiSectionsToUiSections(apiSections: string[]) {
    return apiSections.map(permission => {
      for (const key in ConstantHelper.userGroupSectionsSettings) {
          if (ConstantHelper.userGroupSectionsSettings[key][1].toLowerCase() === permission.toLowerCase()) {
              return ConstantHelper.userGroupSectionsSettings[key][0];
          }
      }
      return null;
    });
  }

  async doesUserGroupHaveSections(userGroupName: string, sections: string[]) {
    const userGroup = await this.getByName(userGroupName);
    const sectionsData = userGroup.sections;
    if (sectionsData.length !== sections.length) {
      return false;
    }
    return sections.every(item => sectionsData.includes(item));
  }

  async createUserGroupWithReadDocumentPermissionAndReadPropertyValueDocumentPermission(name: string, readEnabled: boolean = true, readPropertyValueEnabled: boolean = true) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection(ConstantHelper.sectionAliases.content)
      .withDocumentRootAccess(true)
      .addFallbackPermission()
        .withReadDocumentPermission(readEnabled)
        .withReadPropertyValueDocumentPermission(readPropertyValueEnabled)
        .done()
      .build();

    return await this.create(userGroup);
  }

  async createUserGroupWithUpdateDocumentPermissionAndWritePropertyValueDocumentPermission(name: string, updateEnabled: boolean = true, writePropertyValueEnabled: boolean = true, readPropertyValueEnabled: boolean = true) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection(ConstantHelper.sectionAliases.content)
      .withDocumentRootAccess(true)
      .addFallbackPermission()
        .withUpdateDocumentPermission(updateEnabled)
        .withReadDocumentPermission(true)
        .withWritePropertyValueDocumentPermission(writePropertyValueEnabled)
        .withReadPropertyValueDocumentPermission(readPropertyValueEnabled)
        .done()
      .build();

    return await this.create(userGroup);
  }

  async createUserGroupWithPermissionsForSpecificDocumentAndTwoPropertyValues(name: string, documentId: string, documentTypeId: string, firstPropertyValueName: string, readFirstPropertyValueEnabled: boolean = true, writeFirstPropertyValueEnabled: boolean = true, secondPropertyValueName: string, readSecondPropertyValueEnabled: boolean = true, writeSecondPropertyValueEnabled: boolean = true) {
    await this.ensureNameNotExists(name);
    const firstPropertyValueId = await this.api.documentType.getPropertyIdWithName(documentTypeId, firstPropertyValueName);
    const secondPropertyValueId = await this.api.documentType.getPropertyIdWithName(documentTypeId, secondPropertyValueName);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection(ConstantHelper.sectionAliases.content)
      .withDocumentRootAccess(true)
      .addPermissions()
        .addDocumentPermission()
          .withDocumentId(documentId)
          .addVerbs()
            .withReadDocumentPermission(true)
            .withUpdateDocumentPermission(true)
            .done()
          .done()
        .addPropertyValuePermission()
          .withDocumentTypeId(documentTypeId)
          .withPropertyTypeId(firstPropertyValueId)
          .addVerbs()
            .withReadPropertyValueDocumentPermission(readFirstPropertyValueEnabled)
            .withWritePropertyValueDocumentPermission(writeFirstPropertyValueEnabled)
            .done()
          .done()
        .addPropertyValuePermission()
          .withDocumentTypeId(documentTypeId)
          .withPropertyTypeId(secondPropertyValueId)
          .addVerbs()
            .withReadPropertyValueDocumentPermission(readSecondPropertyValueEnabled)
            .withWritePropertyValueDocumentPermission(writeSecondPropertyValueEnabled)
            .done()
          .done()
        .done()
      .build();

    return await this.create(userGroup);
  }

  async createUserGroupWithReadPermissionForSpecificDocument(name: string, documentId: string, enabled: boolean = true) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection(ConstantHelper.sectionAliases.content)
      .withDocumentRootAccess(true)
      .addPermissions()
        .addDocumentPermission()
          .withDocumentId(documentId)
          .addVerbs()
            .withReadDocumentPermission(enabled)
            .done()
          .done()
        .done()
      .build();

    return await this.create(userGroup);
  }

  async createUserGroupWithCreateDocumentBlueprintPermissionForSpecificDocument(name: string, documentId: string, enabled: boolean = true) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection(ConstantHelper.sectionAliases.content)
      .withDocumentRootAccess(true)
      .addPermissions()
        .addDocumentPermission()
          .withDocumentId(documentId)
          .addVerbs()
            .withCreateDocumentBlueprintPermission(enabled)
            .withReadDocumentPermission(true)
            .done()
          .done()
        .done()
      .build();

    return await this.create(userGroup);
  }

  async createUserGroupWithDeletePermissionForSpecificDocument(name: string, documentId: string, enabled: boolean = true) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection(ConstantHelper.sectionAliases.content)
      .withDocumentRootAccess(true)
      .addPermissions()
        .addDocumentPermission()
          .withDocumentId(documentId)
          .addVerbs()
            .withDeleteDocumentPermission(enabled)
            .withReadDocumentPermission(true)
            .done()
          .done()
        .done()
      .build();

    return await this.create(userGroup);
  }

  async createUserGroupWithCreatePermissionForSpecificDocument(name: string, documentId: string, enabled: boolean = true) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection(ConstantHelper.sectionAliases.content)
      .withDocumentRootAccess(true)
      .addPermissions()
        .addDocumentPermission()
          .withDocumentId(documentId)
          .addVerbs()
            .withCreateDocumentPermission(enabled)
            .withReadDocumentPermission(true)
            .done()
          .done()
        .done()
      .build();

    return await this.create(userGroup);
  }

  async createUserGroupWithNotificationsPermissionForSpecificDocument(name: string, documentId: string, enabled: boolean = true) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection(ConstantHelper.sectionAliases.content)
      .withDocumentRootAccess(true)
      .addPermissions()
        .addDocumentPermission()
          .withDocumentId(documentId)
          .addVerbs()
            .withNotificationsDocumentPermission(enabled)
            .withReadDocumentPermission(true)
            .done()
          .done()
        .done()
      .build();

    return await this.create(userGroup);
  }

  async createUserGroupWithPublishPermissionForSpecificDocument(name: string, documentId: string, enabled: boolean = true) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection(ConstantHelper.sectionAliases.content)
      .withDocumentRootAccess(true)
      .addPermissions()
        .addDocumentPermission()
          .withDocumentId(documentId)
          .addVerbs()
            .withPublishDocumentPermission(enabled)
            .withReadDocumentPermission(true)
            .done()
          .done()
        .done()
      .build();

    return await this.create(userGroup);
  }

  async createUserGroupWithSetPermissionsPermissionForSpecificDocument(name: string, documentId: string, enabled: boolean = true) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection(ConstantHelper.sectionAliases.content)
      .withDocumentRootAccess(true)
      .addPermissions()
        .addDocumentPermission()
          .withDocumentId(documentId)
          .addVerbs()
            .withSetPermissionsDocumentPermission(enabled)
            .withReadDocumentPermission(true)
            .done()
          .done()
        .done()
      .build();

    return await this.create(userGroup);
  }

  async createUserGroupWithUnpublishPermissionForSpecificDocument(name: string, documentId: string, enabled: boolean = true) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection(ConstantHelper.sectionAliases.content)
      .withDocumentRootAccess(true)
      .addPermissions()
        .addDocumentPermission()
          .withDocumentId(documentId)
          .addVerbs()
            .withUnpublishDocumentPermission(enabled)
            .withReadDocumentPermission(true)
            .done()
          .done()
        .done()
      .build();

    return await this.create(userGroup);
  }

  async createUserGroupWithUpdatePermissionForSpecificDocument(name: string, documentId: string, enabled: boolean = true) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection(ConstantHelper.sectionAliases.content)
      .withDocumentRootAccess(true)
      .addFallbackPermission()
        .withReadDocumentPermission(true)
        .done()
      .addPermissions()
        .addDocumentPermission()
          .withDocumentId(documentId)
          .addVerbs()
            .withUpdateDocumentPermission(enabled)
            .withReadDocumentPermission(true)
            .done()
          .done()
        .done()
      .build();

    return await this.create(userGroup);
  }

  async createUserGroupWithDuplicatePermissionForSpecificDocument(name: string, documentId: string, enabled: boolean = true) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection(ConstantHelper.sectionAliases.content)
      .withDocumentRootAccess(true)
      .addFallbackPermission()
        .withCreateDocumentPermission(true) // need to have the 'create' permission - refer this PR: https://github.com/umbraco/Umbraco-CMS/pull/19303
        .done()
      .addPermissions()
        .addDocumentPermission()
          .withDocumentId(documentId)
          .addVerbs()
            .withDuplicateDocumentPermission(enabled)
            .withReadDocumentPermission(true)
            .done()
          .done()
        .done()
      .build();

    return await this.create(userGroup);
  }

  async createUserGroupWithMoveToPermissionForSpecificDocument(name: string, documentId: string, enabled: boolean = true) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection(ConstantHelper.sectionAliases.content)
      .withDocumentRootAccess(true)
      .addFallbackPermission()
        .withCreateDocumentPermission(true) // need to have the 'create' permission - refer this PR: https://github.com/umbraco/Umbraco-CMS/pull/19303
        .done()
      .addPermissions()
        .addDocumentPermission()
          .withDocumentId(documentId)
          .addVerbs()
            .withMoveToDocumentPermission(enabled)
            .withReadDocumentPermission(true)
            .done()
          .done()
        .done()
      .build();

    return await this.create(userGroup);
  }

  async createUserGroupWithSortChildrenPermissionForSpecificDocument(name: string, documentId: string, enabled: boolean = true) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection(ConstantHelper.sectionAliases.content)
      .withDocumentRootAccess(true)
      .addPermissions()
        .addDocumentPermission()
          .withDocumentId(documentId)
          .addVerbs()
            .withSortChildrenDocumentPermission(enabled)
            .withReadDocumentPermission(true)
            .done()
          .done()
        .done()
      .build();

    return await this.create(userGroup);
  }

  async createUserGroupWithCultureAndHostnamesPermissionForSpecificDocument(name: string, documentId: string, enabled: boolean = true) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection(ConstantHelper.sectionAliases.content)
      .withDocumentRootAccess(true)
      .addPermissions()
        .addDocumentPermission()
          .withDocumentId(documentId)
          .addVerbs()
            .withCultureAndHostnamesDocumentPermission(enabled)
            .withReadDocumentPermission(true)
            .done()
          .done()
        .done()
      .build();

    return await this.create(userGroup);
  }

  async createUserGroupWithPublicAccessPermissionForSpecificDocument(name: string, documentId: string, enabled: boolean = true) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection(ConstantHelper.sectionAliases.content)
      .addSection(ConstantHelper.sectionAliases.members)
      .withDocumentRootAccess(true)
      .addPermissions()
        .addDocumentPermission()
          .withDocumentId(documentId)
          .addVerbs()
            .withPublicAccessDocumentPermission(enabled)
            .withReadDocumentPermission(true)
            .done()
          .done()
        .done()
      .build();

    return await this.create(userGroup);
  }

  async createUserGroupWithRollbackPermissionForSpecificDocument(name: string, documentId: string, enabled: boolean = true) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection(ConstantHelper.sectionAliases.content)
      .withDocumentRootAccess(true)
      .addFallbackPermission()
        .withReadPropertyValueDocumentPermission(true)
        .withWritePropertyValueDocumentPermission(true)
        .done()
      .addPermissions()
        .addDocumentPermission()
          .withDocumentId(documentId)
          .addVerbs()
            .withRollbackDocumentPermission(enabled)
            .withReadDocumentPermission(true)
            .done()
          .done()
        .done()
      .build();

    return await this.create(userGroup);
  }

  async createUserGroupWithCreateAndUpdateDocumentPermission(name: string, enabled: boolean = true) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection(ConstantHelper.sectionAliases.content)
      .withDocumentRootAccess(true)
      .addFallbackPermission()
        .withCreateDocumentPermission(enabled)
        .withUpdateDocumentPermission(enabled)
        .withReadDocumentPermission(true)
        .done()
      .build();

    return await this.create(userGroup);
  }

  // Element permission methods
  async createUserGroupWithReadElementPermission(name: string, enabled: boolean = true) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection(ConstantHelper.sectionAliases.library)
      .withElementRootAccess(true)
      .addFallbackPermission()
        .withReadElementPermission(enabled)
        .done()
      .build();

    return await this.create(userGroup);
  }

  async createUserGroupWithCreateElementPermission(name: string, enabled: boolean = true) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection(ConstantHelper.sectionAliases.library)
      .withElementRootAccess(true)
      .addFallbackPermission()
        .withCreateElementPermission(enabled)
        .withReadElementPermission(true)
        .done()
      .build();

    return await this.create(userGroup);
  }

  async createUserGroupWithDeleteElementPermission(name: string, enabled: boolean = true) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection(ConstantHelper.sectionAliases.library)
      .withElementRootAccess(true)
      .addFallbackPermission()
        .withDeleteElementPermission(enabled)
        .withReadElementPermission(true)
        .done()
      .build();

    return await this.create(userGroup);
  }

  async createUserGroupWithPublishElementPermission(name: string, enabled: boolean = true) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection(ConstantHelper.sectionAliases.library)
      .withElementRootAccess(true)
      .addFallbackPermission()
        .withPublishElementPermission(enabled)
        .withReadElementPermission(true)
        .done()
      .build();

    return await this.create(userGroup);
  }

  async createUserGroupWithUnpublishElementPermission(name: string, enabled: boolean = true) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection(ConstantHelper.sectionAliases.library)
      .withElementRootAccess(true)
      .addFallbackPermission()
        .withUnpublishElementPermission(enabled)
        .withReadElementPermission(true)
        .done()
      .build();

    return await this.create(userGroup);
  }

  async createUserGroupWithUpdateElementPermission(name: string, enabled: boolean = true) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection(ConstantHelper.sectionAliases.library)
      .withElementRootAccess(true)
      .addFallbackPermission()
        .withUpdateElementPermission(enabled)
        .withReadElementPermission(true)
        .done()
      .build();

    return await this.create(userGroup);
  }

  async createUserGroupWithDuplicateElementPermission(name: string, enabled: boolean = true) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection(ConstantHelper.sectionAliases.library)
      .withElementRootAccess(true)
      .addFallbackPermission()
        .withDuplicateElementPermission(enabled)
        .withCreateElementPermission(enabled)
        .withReadElementPermission(true)
        .done()
      .build();

    return await this.create(userGroup);
  }

  async createUserGroupWithMoveElementPermission(name: string, enabled: boolean = true) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection(ConstantHelper.sectionAliases.library)
      .withElementRootAccess(true)
      .addFallbackPermission()
        .withMoveElementPermission(enabled)
        .withCreateElementPermission(enabled)
        .withReadElementPermission(true)
        .done()
      .build();

    return await this.create(userGroup);
  }

  async createUserGroupWithRollbackElementPermission(name: string, enabled: boolean = true) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection(ConstantHelper.sectionAliases.library)
      .withElementRootAccess(true)
      .addFallbackPermission()
        .withRollbackElementPermission(enabled)
        .withReadElementPermission(true)
        .done()
      .build();

    return await this.create(userGroup);
  }

  async createUserGroupWithDeleteElementPermissionAndCreateElementPermission(name: string, deleteEnabled: boolean = true, createEnabled: boolean = true) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection(ConstantHelper.sectionAliases.library)
      .withElementRootAccess(true)
      .addFallbackPermission()
        .withDeleteElementPermission(deleteEnabled)
        .withCreateElementPermission(createEnabled)
        .withReadElementPermission(true)
        .done()
      .build();

    return await this.create(userGroup);
  }

  async createUserGroupWithCreateAndUpdateElementPermission(name: string, enabled: boolean = true) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection(ConstantHelper.sectionAliases.library)
      .withElementRootAccess(true)
      .addFallbackPermission()
        .withCreateElementPermission(enabled)
        .withUpdateElementPermission(enabled)
        .withReadElementPermission(true)
        .done()
      .build();

    return await this.create(userGroup);
  }

  async doesUserGroupContainElementStartNodeId(userGroupName: string, elementStartNodeId: string) {
    const userGroup = await this.getByName(userGroupName);
    if (userGroup.elementStartNode === null) {
      return false;
    }
    return userGroup.elementStartNode.id.includes(elementStartNodeId);
  }

  async doesUserGroupContainElementRootAccess(userGroupName: string) {
    const userGroup = await this.getByName(userGroupName);
    return userGroup.elementRootAccess;
  }

  async createUserGroupWithElementStartNode(name: string, startNodeId: string) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection(ConstantHelper.sectionAliases.library)
      .withElementRootAccess(false)
      .withElementStartNodeId(startNodeId)
      .addFallbackPermission()
        .withReadDocumentPermission(true)
        .done()
      .build();

    return await this.create(userGroup);
  }

  async createUserGroupWithReadPermissionForSpecificElement(name: string, elementId: string, enabled: boolean = true) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection(ConstantHelper.sectionAliases.library)
      .withElementRootAccess(true)
      .addPermissions()
        .addElementPermission()
          .withElementId(elementId)
          .addVerbs()
            .withReadElementPermission(enabled)
            .done()
          .done()
        .done()
      .build();

    return await this.create(userGroup);
  }

  async createUserGroupWithDeletePermissionForSpecificElement(name: string, elementId: string, enabled: boolean = true) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection(ConstantHelper.sectionAliases.library)
      .withElementRootAccess(true)
      .addPermissions()
        .addElementPermission()
          .withElementId(elementId)
          .addVerbs()
            .withDeleteElementPermission(enabled)
            .withReadElementPermission(true)
            .done()
          .done()
        .done()
      .build();

    return await this.create(userGroup);
  }

  async createUserGroupWithPublishPermissionForSpecificElement(name: string, elementId: string, enabled: boolean = true) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection(ConstantHelper.sectionAliases.library)
      .withElementRootAccess(true)
      .addPermissions()
        .addElementPermission()
          .withElementId(elementId)
          .addVerbs()
            .withPublishElementPermission(enabled)
            .withReadElementPermission(true)
            .done()
          .done()
        .done()
      .build();

    return await this.create(userGroup);
  }

  async createUserGroupWithUnpublishPermissionForSpecificElement(name: string, elementId: string, enabled: boolean = true) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection(ConstantHelper.sectionAliases.library)
      .withElementRootAccess(true)
      .addPermissions()
        .addElementPermission()
          .withElementId(elementId)
          .addVerbs()
            .withUnpublishElementPermission(enabled)
            .withReadElementPermission(true)
            .done()
          .done()
        .done()
      .build();

    return await this.create(userGroup);
  }

  async createUserGroupWithUpdatePermissionForSpecificElement(name: string, elementId: string, enabled: boolean = true) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection(ConstantHelper.sectionAliases.library)
      .withElementRootAccess(true)
      .addFallbackPermission()
        .withReadElementPermission(true)
        .done()
      .addPermissions()
        .addElementPermission()
          .withElementId(elementId)
          .addVerbs()
            .withUpdateElementPermission(enabled)
            .withReadElementPermission(true)
            .done()
          .done()
        .done()
      .build();

    return await this.create(userGroup);
  }

  async createUserGroupWithDuplicatePermissionForSpecificElement(name: string, elementId: string, enabled: boolean = true) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection(ConstantHelper.sectionAliases.library)
      .withElementRootAccess(true)
      .addFallbackPermission()
        .withCreateElementPermission(true)
        .done()
      .addPermissions()
        .addElementPermission()
          .withElementId(elementId)
          .addVerbs()
            .withDuplicateElementPermission(enabled)
            .withReadElementPermission(true)
            .done()
          .done()
        .done()
      .build();

    return await this.create(userGroup);
  }

  async createUserGroupWithMovePermissionForSpecificElement(name: string, elementId: string, enabled: boolean = true) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection(ConstantHelper.sectionAliases.library)
      .withElementRootAccess(true)
      .addFallbackPermission()
        .withCreateElementPermission(true)
        .done()
      .addPermissions()
        .addElementPermission()
          .withElementId(elementId)
          .addVerbs()
            .withMoveElementPermission(enabled)
            .withReadElementPermission(true)
            .done()
          .done()
        .done()
      .build();

    return await this.create(userGroup);
  }

  async createUserGroupWithRollbackPermissionForSpecificElement(name: string, elementId: string, enabled: boolean = true) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection(ConstantHelper.sectionAliases.library)
      .withElementRootAccess(true)
      .addPermissions()
        .addElementPermission()
          .withElementId(elementId)
          .addVerbs()
            .withRollbackElementPermission(enabled)
            .withReadElementPermission(true)
            .done()
          .done()
        .done()
      .build();

    return await this.create(userGroup);
  }

  async createUserGroupWithCreatePermissionForSpecificElement(name: string, elementId: string, enabled: boolean = true) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection(ConstantHelper.sectionAliases.library)
      .withElementRootAccess(true)
      .addPermissions()
        .addElementPermission()
          .withElementId(elementId)
          .addVerbs()
            .withCreateElementPermission(enabled)
            .withReadElementPermission(true)
            .done()
          .done()
        .done()
      .build();

    return await this.create(userGroup);
  }

  async createSimpleUserGroupWithLibrarySection(name: string) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection(ConstantHelper.sectionAliases.library)
      .addFallbackPermission()
        .withReadDocumentPermission(true)
        .done()
      .build();

    return await this.create(userGroup);
  }

  async convertApiElementPermissionsToUiElementPermissions(apiPermissions: string[]) {
    return apiPermissions.map(permission => {
      for (const key in ConstantHelper.userGroupElementPermissionsSettings) {
          if (ConstantHelper.userGroupElementPermissionsSettings[key][2].toLowerCase() === permission.toLowerCase()) {
              return ConstantHelper.userGroupElementPermissionsSettings[key][0];
          }
      }
      return null;
    });
  }
}
