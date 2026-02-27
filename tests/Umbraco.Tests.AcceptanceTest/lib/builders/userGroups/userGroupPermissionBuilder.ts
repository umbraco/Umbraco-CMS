import {UserGroupBuilder} from './userGroupBuilder';
import {UserGroupDocumentPermissionBuilder} from './userGroupDocumentPermissionBuilder';
import {UserGroupPropertyValuePermissionBuilder} from './userGroupPropertyValuePermissionBuilder';

export class UserGroupPermissionBuilder {
  parentBuilder: UserGroupBuilder;
  permissionBuilders: (UserGroupDocumentPermissionBuilder | UserGroupPropertyValuePermissionBuilder)[];

  constructor(parentBuilder: UserGroupBuilder) {
    this.parentBuilder = parentBuilder;
    this.permissionBuilders = [];
  }

  addDocumentPermission() {
    const builder = new UserGroupDocumentPermissionBuilder(this);
    this.permissionBuilders.push(builder);
    return builder;
  }

  addPropertyValuePermission() {
    const builder = new UserGroupPropertyValuePermissionBuilder(this);
    this.permissionBuilders.push(builder);
    return builder;
  }

  done() {
    return this.parentBuilder;
  }

  build() {
    return this.permissionBuilders.map(builder => builder.build());
  }
}