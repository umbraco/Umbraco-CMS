import {UserGroupPermissionBuilder} from './userGroupPermissionBuilder';
import {UserGroupsPermissionsBaseBuilder} from './userGroupsPermissionsBaseBuilder';

export class UserGroupPropertyValuePermissionBuilder {
  parentBuilder: UserGroupPermissionBuilder;
  documentTypeId: string;
  propertyTypeId: string;
  userGroupsPermissionsBaseBuilder: UserGroupsPermissionsBaseBuilder;

  constructor(parentBuilder: UserGroupPermissionBuilder) {
    this.parentBuilder = parentBuilder;
  }

  withDocumentTypeId(documentTypeId: string) {
    this.documentTypeId = documentTypeId;
    return this;
  }

  withPropertyTypeId(propertyTypeId: string) {
    this.propertyTypeId = propertyTypeId;
    return this;
  }

  addVerbs() {
    const builder = new UserGroupsPermissionsBaseBuilder(this);
    this.userGroupsPermissionsBaseBuilder = builder;
    return builder;
  }

  done() {
    return this.parentBuilder;
  }

  build() {
    return {
      '$type': 'DocumentPropertyValuePermissionPresentationModel',
      documentType: this.documentTypeId ? {id: this.documentTypeId} : null,
      propertyType: this.propertyTypeId ? {id: this.propertyTypeId} : null,
      verbs: this.userGroupsPermissionsBaseBuilder ? this.userGroupsPermissionsBaseBuilder.build() : [],
    };
  }
}