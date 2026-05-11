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
      .addSection('Umb.Section.Content')
      .addFallbackPermission()
        .withReadPermission(true)
        .done()
      .build();

    return await this.create(userGroup);
  }

  async createSimpleUserGroupWithMediaSection(name: string) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection('Umb.Section.Media')
      .addFallbackPermission()
        .withReadPermission(true)
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
      .addSection('Umb.Section.Content')
      .withDocumentRootAccess(false)
      .withDocumentStartNodeId(startNodeId)
      .addFallbackPermission()
        .withReadPermission(true)
        .done()
      .build();

    return await this.create(userGroup);
  }
  
  async createUserGroupWithMediaStartNode(name: string, startNodeId: string) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection('Umb.Section.Media')
      .withMediaRootAccess(false)
      .withMediaStartNodeId(startNodeId)
      .addFallbackPermission()
        .withReadPermission(true)
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
      .addSection('Umb.Section.Content')
      .addLanguage(languageName)
      .withDocumentRootAccess(true)
      .addFallbackPermission()
        .withReadPermission(true)
        .withUpdatePermission(true)
        .withReadPropertyValuePermission(true)
        .withWritePropertyValuePermission(true)
        .done()
      .build();

    return await this.create(userGroup);
  }

  async createUserGroupWithMemberSection(name: string) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection('Umb.Section.Members')
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
            .withReadPermission(true)
            .done()
          .done()
        .done()
      .build();

    return await this.create(userGroup);
  }
  
  async createUserGroupWithReadPermission(name: string, enabled: boolean = true) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection('Umb.Section.Content')
      .withDocumentRootAccess(true)
      .addFallbackPermission()
        .withReadPermission(enabled)
        .done()
      .build();

    return await this.create(userGroup);
  }
  
  async createUserGroupWithCreateDocumentBlueprintPermission(name: string, enabled: boolean = true) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection('Umb.Section.Content')
      .withDocumentRootAccess(true)
      .addFallbackPermission()
        .withCreateDocumentBlueprintPermission(enabled)
        .withReadPermission(true)
        .done()
      .build();

    return await this.create(userGroup);
  }
  
  async createUserGroupWithDeleteDocumentPermission(name: string, enabled: boolean = true) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection('Umb.Section.Content')
      .withDocumentRootAccess(true)
      .addFallbackPermission()
        .withDeletePermission(enabled)
        .withReadPermission(true)
        .done()
      .build();

    return await this.create(userGroup);
  }
  
  async createUserGroupWithCreateDocumentPermission(name: string, enabled: boolean = true) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection('Umb.Section.Content')
      .withDocumentRootAccess(true)
      .addFallbackPermission()
        .withCreatePermission(enabled)
        .withReadPermission(true)
        .done()
      .build();

    return await this.create(userGroup);
  }
  
  async createUserGroupWithNotificationsPermission(name: string, enabled: boolean = true) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection('Umb.Section.Content')
      .withDocumentRootAccess(true)
      .addFallbackPermission()
        .withNotificationsPermission(enabled)
        .withReadPermission(true)
        .done()
      .build();

    return await this.create(userGroup);
  }
  
  async createUserGroupWithPublishPermission(name: string, enabled: boolean = true) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection('Umb.Section.Content')
      .withDocumentRootAccess(true)
      .addFallbackPermission()
        .withPublishPermission(enabled)
        .withReadPermission(true)
        .done()
      .build();

    return await this.create(userGroup);
  }
  
  async createUserGroupWithSetPermissionsPermission(name: string, enabled: boolean = true) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection('Umb.Section.Content')
      .withDocumentRootAccess(true)
      .addFallbackPermission()
        .withSetPermissionsPermission(enabled)
        .withReadPermission(true)
        .done()
      .build();

    return await this.create(userGroup);
  }
  
  async createUserGroupWithUnpublishPermission(name: string, enabled: boolean = true) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection('Umb.Section.Content')
      .withDocumentRootAccess(true)
      .addFallbackPermission()
        .withUnpublishPermission(enabled)
        .withReadPermission(true)
        .done()
      .build();

    return await this.create(userGroup);
  }
  
  async createUserGroupWithUpdatePermission(name: string, enabled: boolean = true) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection('Umb.Section.Content')
      .withDocumentRootAccess(true)
      .addFallbackPermission()
        .withUpdatePermission(enabled)
        .withReadPermission(true)
        .done()
      .build();

    return await this.create(userGroup);
  }
  
  async createUserGroupWithDuplicatePermission(name: string, enabled: boolean = true) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection('Umb.Section.Content')
      .withDocumentRootAccess(true)
      .addFallbackPermission()
        .withDuplicatePermission(enabled)
        .withCreatePermission(enabled)
        .withReadPermission(true)
        .done()
      .build();

    return await this.create(userGroup);
  }
  
  async createUserGroupWithMoveToPermission(name: string, enabled: boolean = true) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection('Umb.Section.Content')
      .withDocumentRootAccess(true)
      .addFallbackPermission()
        .withMoveToPermission(enabled)
        .withCreatePermission(enabled)
        .withReadPermission(true)
        .done()
      .build();

    return await this.create(userGroup);
  }
  
  async createUserGroupWithSortChildrenPermission(name: string, enabled: boolean = true) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection('Umb.Section.Content')
      .withDocumentRootAccess(true)
      .addFallbackPermission()
        .withSortChildrenPermission(enabled)
        .withReadPermission(true)
        .done()
      .build();

    return await this.create(userGroup);
  }
  
  async createUserGroupWithCultureAndHostnamesPermission(name: string, enabled: boolean = true) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection('Umb.Section.Content')
      .withDocumentRootAccess(true)
      .addFallbackPermission()
        .withCultureAndHostnamesPermission(enabled)
        .withReadPermission(true)
        .done()
      .build();

    return await this.create(userGroup);
  }
  
  async createUserGroupWithPublicAccessPermission(name: string, enabled: boolean = true) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection('Umb.Section.Content')
      .addSection('Umb.Section.Members')
      .withDocumentRootAccess(true)
      .addFallbackPermission()
        .withPublicAccessPermission(enabled)
        .withReadPermission(true)
        .done()
      .build();

    return await this.create(userGroup);
  }
  
  async createUserGroupWithRollbackPermission(name: string, enabled: boolean = true) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection('Umb.Section.Content')
      .withDocumentRootAccess(true)
      .addFallbackPermission()
        .withRollbackPermission(enabled)
        .withReadPermission(true)
        .withReadPropertyValuePermission(true)
        .done()
      .build();

    return await this.create(userGroup);
  }
  
  async createUserGroupWithDeletePermissionAndCreatePermission(name: string, deleteEnabled: boolean = true, createEnabled: boolean = true) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection('Umb.Section.Content')
      .withDocumentRootAccess(true)
      .addFallbackPermission()
        .withDeletePermission(deleteEnabled)
        .withCreatePermission(createEnabled)
        .withReadPermission(true)
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

  async convertApiPermissionsToUiPermissions(apiPermissions: string[]) {
    return apiPermissions.map(permission => {
      for (const key in ConstantHelper.userGroupPermissionsSettings) {
          if (ConstantHelper.userGroupPermissionsSettings[key][2].toLowerCase() === permission.toLowerCase()) {
              return ConstantHelper.userGroupPermissionsSettings[key][0];
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

  async createUserGroupWithReadPermissionAndReadPropertyValuePermission(name: string, readEnabled: boolean = true, readPropertyValueEnabled: boolean = true) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection('Umb.Section.Content')
      .withDocumentRootAccess(true)
      .addFallbackPermission()
        .withReadPermission(readEnabled)
        .withReadPropertyValuePermission(readPropertyValueEnabled)
        .done()
      .build();

    return await this.create(userGroup);
  }

  async createUserGroupWithUpdatePermissionAndWritePropertyValuePermission(name: string, updateEnabled: boolean = true, writePropertyValueEnabled: boolean = true, readPropertyValueEnabled: boolean = true) {
    await this.ensureNameNotExists(name);

    const userGroup = new UserGroupBuilder()
      .withName(name)
      .addSection('Umb.Section.Content')
      .withDocumentRootAccess(true)
      .addFallbackPermission()
        .withUpdatePermission(updateEnabled)
        .withReadPermission(true)
        .withWritePropertyValuePermission(writePropertyValueEnabled)
        .withReadPropertyValuePermission(readPropertyValueEnabled)
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
      .addSection('Umb.Section.Content')
      .withDocumentRootAccess(true)
      .addPermissions()
        .addDocumentPermission()
          .withDocumentId(documentId)
          .addVerbs()
            .withReadPermission(true)
            .withUpdatePermission(true)
            .done()
          .done()
        .addPropertyValuePermission()
          .withDocumentTypeId(documentTypeId)
          .withPropertyTypeId(firstPropertyValueId)
          .addVerbs()
            .withReadPropertyValuePermission(readFirstPropertyValueEnabled)
            .withWritePropertyValuePermission(writeFirstPropertyValueEnabled)
            .done()
          .done()
        .addPropertyValuePermission()
          .withDocumentTypeId(documentTypeId)
          .withPropertyTypeId(secondPropertyValueId)
          .addVerbs()
            .withReadPropertyValuePermission(readSecondPropertyValueEnabled)
            .withWritePropertyValuePermission(writeSecondPropertyValueEnabled)
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
      .addSection('Umb.Section.Content')
      .withDocumentRootAccess(true)
      .addPermissions()
        .addDocumentPermission()
          .withDocumentId(documentId)
          .addVerbs()
            .withReadPermission(enabled)
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
      .addSection('Umb.Section.Content')
      .withDocumentRootAccess(true)
      .addPermissions()
        .addDocumentPermission()
          .withDocumentId(documentId)
          .addVerbs()
            .withCreateDocumentBlueprintPermission(enabled)
            .withReadPermission(true)
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
      .addSection('Umb.Section.Content')
      .withDocumentRootAccess(true)
      .addPermissions()
        .addDocumentPermission()
          .withDocumentId(documentId)
          .addVerbs()
            .withDeletePermission(enabled)
            .withReadPermission(true)
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
      .addSection('Umb.Section.Content')
      .withDocumentRootAccess(true)
      .addPermissions()
        .addDocumentPermission()
          .withDocumentId(documentId)
          .addVerbs()
            .withCreatePermission(enabled)
            .withReadPermission(true)
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
      .addSection('Umb.Section.Content')
      .withDocumentRootAccess(true)
      .addPermissions()
        .addDocumentPermission()
          .withDocumentId(documentId)
          .addVerbs()
            .withNotificationsPermission(enabled)
            .withReadPermission(true)
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
      .addSection('Umb.Section.Content')
      .withDocumentRootAccess(true)
      .addPermissions()
        .addDocumentPermission()
          .withDocumentId(documentId)
          .addVerbs()
            .withPublishPermission(enabled)
            .withReadPermission(true)
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
      .addSection('Umb.Section.Content')
      .withDocumentRootAccess(true)
      .addPermissions()
        .addDocumentPermission()
          .withDocumentId(documentId)
          .addVerbs()
            .withSetPermissionsPermission(enabled)
            .withReadPermission(true)
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
      .addSection('Umb.Section.Content')
      .withDocumentRootAccess(true)
      .addPermissions()
        .addDocumentPermission()
          .withDocumentId(documentId)
          .addVerbs()
            .withUnpublishPermission(enabled)
            .withReadPermission(true)
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
      .addSection('Umb.Section.Content')
      .withDocumentRootAccess(true)
      .addFallbackPermission()
        .withReadPermission(true)
        .done()
      .addPermissions()
        .addDocumentPermission()
          .withDocumentId(documentId)
          .addVerbs()
            .withUpdatePermission(enabled)
            .withReadPermission(true)
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
      .addSection('Umb.Section.Content')
      .withDocumentRootAccess(true)
      .addFallbackPermission()
        .withCreatePermission(true) // need to have the 'create' permission - refer this PR: https://github.com/umbraco/Umbraco-CMS/pull/19303
        .done()
      .addPermissions()
        .addDocumentPermission()
          .withDocumentId(documentId)
          .addVerbs()
            .withDuplicatePermission(enabled)
            .withReadPermission(true)
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
      .addSection('Umb.Section.Content')
      .withDocumentRootAccess(true)
      .addFallbackPermission()
        .withCreatePermission(true) // need to have the 'create' permission - refer this PR: https://github.com/umbraco/Umbraco-CMS/pull/19303
        .done()
      .addPermissions()
        .addDocumentPermission()
          .withDocumentId(documentId)
          .addVerbs()
            .withMoveToPermission(enabled)
            .withReadPermission(true)
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
      .addSection('Umb.Section.Content')
      .withDocumentRootAccess(true)
      .addPermissions()
        .addDocumentPermission()
          .withDocumentId(documentId)
          .addVerbs()
            .withSortChildrenPermission(enabled)
            .withReadPermission(true)
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
      .addSection('Umb.Section.Content')
      .withDocumentRootAccess(true)
      .addPermissions()
        .addDocumentPermission()
          .withDocumentId(documentId)
          .addVerbs()
            .withCultureAndHostnamesPermission(enabled)
            .withReadPermission(true)
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
      .addSection('Umb.Section.Content')
      .addSection('Umb.Section.Members')
      .withDocumentRootAccess(true)
      .addPermissions()
        .addDocumentPermission()
          .withDocumentId(documentId)
          .addVerbs()
            .withPublicAccessPermission(enabled)
            .withReadPermission(true)
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
      .addSection('Umb.Section.Content')
      .withDocumentRootAccess(true)
      .addFallbackPermission()
        .withReadPropertyValuePermission(true)
        .withWritePropertyValuePermission(true)
        .done()
      .addPermissions()
        .addDocumentPermission()
          .withDocumentId(documentId)
          .addVerbs()
            .withRollbackPermission(enabled)
            .withReadPermission(true)
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
      .addSection('Umb.Section.Content')
      .withDocumentRootAccess(true)
      .addFallbackPermission()
        .withCreatePermission(enabled)
        .withUpdatePermission(enabled)
        .withReadPermission(true)
        .done()
      .build();

    return await this.create(userGroup);
  }
}