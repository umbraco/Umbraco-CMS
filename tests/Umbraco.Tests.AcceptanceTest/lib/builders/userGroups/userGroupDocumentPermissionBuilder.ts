import {UserGroupPermissionBuilder} from './userGroupPermissionBuilder';
import {UserGroupsPermissionsBaseBuilder} from './userGroupsPermissionsBaseBuilder';

export class UserGroupDocumentPermissionBuilder {
  parentBuilder: UserGroupPermissionBuilder;
  documentId: string;
  userGroupsPermissionsBaseBuilder: UserGroupsPermissionsBaseBuilder;

  constructor(parentBuilder: UserGroupPermissionBuilder) {
    this.parentBuilder = parentBuilder;
  }

  withDocumentId(documentId: string) {
    this.documentId = documentId;
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
      '$type': 'DocumentPermissionPresentationModel',
      document: this.documentId ? {id: this.documentId} : null,
      verbs: this.userGroupsPermissionsBaseBuilder ? this.userGroupsPermissionsBaseBuilder.build() : [],
    };
  }
}