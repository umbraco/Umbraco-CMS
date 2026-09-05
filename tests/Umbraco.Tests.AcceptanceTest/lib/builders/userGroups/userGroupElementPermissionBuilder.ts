import {UserGroupPermissionBuilder} from './userGroupPermissionBuilder';
import {UserGroupsPermissionsBaseBuilder} from './userGroupsPermissionsBaseBuilder';

export class UserGroupElementPermissionBuilder {
  parentBuilder: UserGroupPermissionBuilder;
  elementId: string;
  userGroupsPermissionsBaseBuilder: UserGroupsPermissionsBaseBuilder;

  constructor(parentBuilder: UserGroupPermissionBuilder) {
    this.parentBuilder = parentBuilder;
  }

  withElementId(elementId: string) {
    this.elementId = elementId;
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
      '$type': 'ElementPermissionPresentationModel',
      element: this.elementId ? {id: this.elementId} : null,
      verbs: this.userGroupsPermissionsBaseBuilder ? this.userGroupsPermissionsBaseBuilder.build() : [],
    };
  }
}
