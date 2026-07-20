export class UserGroupsPermissionsBaseBuilder {
  parentBuilder;
  read: boolean = false;
  createDocumentBlueprint: boolean = false;
  delete: boolean = false;
  create: boolean;
  notifications: boolean = false;
  publish: boolean = false;
  setPermissions: boolean = false;
  unpublish: boolean = false;
  update: boolean = false;
  duplicate: boolean = false;
  moveTo: boolean = false;
  sortChildren: boolean = false;
  cultureAndHostnames: boolean = false;
  publicAccess: boolean = false;
  rollback: boolean = false;
  readPropertyValue: boolean = false;
  writePropertyValue: boolean = false;

  constructor(parentBuilder) {
    this.parentBuilder = parentBuilder;
  }

  withReadPermission(read: boolean) {
    this.read = read;
    return this;
  }

  withCreateDocumentBlueprintPermission(createDocumentBlueprint: boolean) {
    this.createDocumentBlueprint = createDocumentBlueprint;
    return this;
  }

  withDeletePermission(deletePermission: boolean) {
    this.delete = deletePermission;
    return this;
  }

  withCreatePermission(createPermission: boolean) {
    this.create = createPermission;
    return this;
  }

  withNotificationsPermission(notifications: boolean) {
    this.notifications = notifications;
    return this;
  }

  withPublishPermission(publish: boolean) {
    this.publish = publish;
    return this;
  }

  withSetPermissionsPermission(setPermissions: boolean) {
    this.setPermissions = setPermissions;
    return this;
  }

  withUnpublishPermission(unpublish: boolean) {
    this.unpublish = unpublish;
    return this;
  }

  withUpdatePermission(update: boolean) {
    this.update = update;
    return this;
  }

  withDuplicatePermission(duplicate: boolean) {
    this.duplicate = duplicate;
    return this;
  }

  withMoveToPermission(moveTo: boolean) {
    this.moveTo = moveTo;
    return this;
  }

  withSortChildrenPermission(sortChildren: boolean) {
    this.sortChildren = sortChildren;
    return this;
  }

  withCultureAndHostnamesPermission(cultureAndHostnames: boolean) {
    this.cultureAndHostnames = cultureAndHostnames;
    return this;
  }

  withPublicAccessPermission(publicAccess: boolean) {
    this.publicAccess = publicAccess;
    return this;
  }

  withRollbackPermission(rollback: boolean) {
    this.rollback = rollback;
    return this;
  }

  withReadPropertyValuePermission(readPropertyValue: boolean) {
    this.readPropertyValue = readPropertyValue;
    return this;
  }

  withWritePropertyValuePermission(writePropertyValue: boolean) {
    this.writePropertyValue = writePropertyValue;
    return this;
  }

  done() {
    return this.parentBuilder;
  }

  build() {
    let values: any[] = [];
    if (this.read) {
      values.push(
        'Umb.Document.Read'
      );
    }
    if (this.createDocumentBlueprint) {
      values.push(
        'Umb.Document.CreateBlueprint'
      );
    }

    if (this.delete) {
      values.push(
        'Umb.Document.Delete'
      );
    }

    if (this.create) {
      values.push(
        'Umb.Document.Create'
      );
    }

    if (this.notifications) {
      values.push(
        'Umb.Document.Notifications'
      );
    }

    if (this.publish) {
      values.push(
        'Umb.Document.Publish'
      );
    }

    if (this.setPermissions) {
      values.push(
        'Umb.Document.Permissions'
      );
    }

    if (this.unpublish) {
      values.push(
        'Umb.Document.Unpublish'
      );
    }

    if (this.update) {
      values.push(
        'Umb.Document.Update'
      );
    }

    if (this.duplicate) {
      values.push(
        'Umb.Document.Duplicate'
      );
    }

    if (this.moveTo) {
      values.push(
        'Umb.Document.Move'
      );
    }

    if (this.sortChildren) {
      values.push(
        'Umb.Document.Sort'
      );
    }

    if (this.cultureAndHostnames) {
      values.push(
        'Umb.Document.CultureAndHostnames'
      );
    }

    if (this.publicAccess) {
      values.push(
        'Umb.Document.PublicAccess'
      );
    }

    if (this.rollback) {
      values.push(
        'Umb.Document.Rollback'
      );
    }

    if (this.readPropertyValue) {
      values.push(
        'Umb.Document.PropertyValue.Read'
      );
    }

    if (this.writePropertyValue) {
      values.push(
        'Umb.Document.PropertyValue.Write'
      );
    }
    return values;
  }
}