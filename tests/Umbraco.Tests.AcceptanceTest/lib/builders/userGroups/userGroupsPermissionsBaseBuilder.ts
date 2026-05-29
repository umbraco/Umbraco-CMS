export class UserGroupsPermissionsBaseBuilder {
  parentBuilder;

  // Document permissions
  readDocument: boolean = false;
  createDocumentBlueprint: boolean = false;
  deleteDocument: boolean = false;
  createDocument: boolean = false;
  notificationsDocument: boolean = false;
  publishDocument: boolean = false;
  setPermissionsDocument: boolean = false;
  unpublishDocument: boolean = false;
  updateDocument: boolean = false;
  duplicateDocument: boolean = false;
  moveToDocument: boolean = false;
  sortChildrenDocument: boolean = false;
  cultureAndHostnamesDocument: boolean = false;
  publicAccessDocument: boolean = false;
  rollbackDocument: boolean = false;
  readPropertyValueDocument: boolean = false;
  writePropertyValueDocument: boolean = false;

  // Element permissions
  readElement: boolean = false;
  createElement: boolean = false;
  deleteElement: boolean = false;
  publishElement: boolean = false;
  unpublishElement: boolean = false;
  updateElement: boolean = false;
  duplicateElement: boolean = false;
  moveElement: boolean = false;
  rollbackElement: boolean = false;

  // Element permissions
  readElementContainer: boolean = false;
  createElementContainer: boolean = false;
  deleteElementContainer: boolean = false;
  updateElementContainer: boolean = false;
  moveElementContainer: boolean = false;
  rollbackElementContainer: boolean = false;

  // Element folder (container) permissions
  readElementFolder: boolean = false;
  deleteElementFolder: boolean = false;

  constructor(parentBuilder) {
    this.parentBuilder = parentBuilder;
  }

  // Document permission methods
  withReadDocumentPermission(read: boolean) {
    this.readDocument = read;
    return this;
  }

  withCreateDocumentBlueprintPermission(createDocumentBlueprint: boolean) {
    this.createDocumentBlueprint = createDocumentBlueprint;
    return this;
  }

  withDeleteDocumentPermission(deletePermission: boolean) {
    this.deleteDocument = deletePermission;
    return this;
  }

  withCreateDocumentPermission(createPermission: boolean) {
    this.createDocument = createPermission;
    return this;
  }

  withNotificationsDocumentPermission(notifications: boolean) {
    this.notificationsDocument = notifications;
    return this;
  }

  withPublishDocumentPermission(publish: boolean) {
    this.publishDocument = publish;
    return this;
  }

  withSetPermissionsDocumentPermission(setPermissions: boolean) {
    this.setPermissionsDocument = setPermissions;
    return this;
  }

  withUnpublishDocumentPermission(unpublish: boolean) {
    this.unpublishDocument = unpublish;
    return this;
  }

  withUpdateDocumentPermission(update: boolean) {
    this.updateDocument = update;
    return this;
  }

  withDuplicateDocumentPermission(duplicate: boolean) {
    this.duplicateDocument = duplicate;
    return this;
  }

  withMoveToDocumentPermission(moveTo: boolean) {
    this.moveToDocument = moveTo;
    return this;
  }

  withSortChildrenDocumentPermission(sortChildren: boolean) {
    this.sortChildrenDocument = sortChildren;
    return this;
  }

  withCultureAndHostnamesDocumentPermission(cultureAndHostnames: boolean) {
    this.cultureAndHostnamesDocument = cultureAndHostnames;
    return this;
  }

  withPublicAccessDocumentPermission(publicAccess: boolean) {
    this.publicAccessDocument = publicAccess;
    return this;
  }

  withRollbackDocumentPermission(rollback: boolean) {
    this.rollbackDocument = rollback;
    return this;
  }

  withReadPropertyValueDocumentPermission(readPropertyValue: boolean) {
    this.readPropertyValueDocument = readPropertyValue;
    return this;
  }

  withWritePropertyValueDocumentPermission(writePropertyValue: boolean) {
    this.writePropertyValueDocument = writePropertyValue;
    return this;
  }

  // Element permission methods
  withReadElementPermission(read: boolean) {
    this.readElement = read;
    return this;
  }

  withCreateElementPermission(create: boolean) {
    this.createElement = create;
    return this;
  }

  withDeleteElementPermission(deletePermission: boolean) {
    this.deleteElement = deletePermission;
    return this;
  }

  withPublishElementPermission(publish: boolean) {
    this.publishElement = publish;
    return this;
  }

  withUnpublishElementPermission(unpublish: boolean) {
    this.unpublishElement = unpublish;
    return this;
  }

  withUpdateElementPermission(update: boolean) {
    this.updateElement = update;
    return this;
  }

  withDuplicateElementPermission(duplicate: boolean) {
    this.duplicateElement = duplicate;
    return this;
  }

  withMoveElementPermission(move: boolean) {
    this.moveElement = move;
    return this;
  }

  withRollbackElementPermission(rollback: boolean) {
    this.rollbackElement = rollback;
    return this;
  }

  // Element Container permission methods
  withReadElementContainerPermission(read: boolean) {
    this.readElementContainer = read;
    return this;
  }

  withCreateElementContainerPermission(create: boolean) {
    this.createElementContainer = create;
    return this;
  }

  withDeleteElementContainerPermission(deletePermission: boolean) {
    this.deleteElementContainer = deletePermission;
    return this;
  }

  withUpdateElementContainerPermission(update: boolean) {
    this.updateElementContainer = update;
    return this;
  }

  withMoveElementContainerPermission(move: boolean) {
    this.moveElementContainer = move;
    return this;
  }

  // Element folder (container) permission methods
  withReadElementFolderPermission(read: boolean) {
    this.readElementFolder = read;
    return this;
  }

  withDeleteElementFolderPermission(deletePermission: boolean) {
    this.deleteElementFolder = deletePermission;
    return this;
  }

  done() {
    return this.parentBuilder;
  }

  build() {
    let values: any[] = [];

    // Document permissions
    if (this.readDocument) {
      values.push('Umb.Document.Read');
    }
    if (this.createDocumentBlueprint) {
      values.push('Umb.Document.CreateBlueprint');
    }
    if (this.deleteDocument) {
      values.push('Umb.Document.Delete');
    }
    if (this.createDocument) {
      values.push('Umb.Document.Create');
    }
    if (this.notificationsDocument) {
      values.push('Umb.Document.Notifications');
    }
    if (this.publishDocument) {
      values.push('Umb.Document.Publish');
    }
    if (this.setPermissionsDocument) {
      values.push('Umb.Document.Permissions');
    }
    if (this.unpublishDocument) {
      values.push('Umb.Document.Unpublish');
    }
    if (this.updateDocument) {
      values.push('Umb.Document.Update');
    }
    if (this.duplicateDocument) {
      values.push('Umb.Document.Duplicate');
    }
    if (this.moveToDocument) {
      values.push('Umb.Document.Move');
    }
    if (this.sortChildrenDocument) {
      values.push('Umb.Document.Sort');
    }
    if (this.cultureAndHostnamesDocument) {
      values.push('Umb.Document.CultureAndHostnames');
    }
    if (this.publicAccessDocument) {
      values.push('Umb.Document.PublicAccess');
    }
    if (this.rollbackDocument) {
      values.push('Umb.Document.Rollback');
    }
    if (this.readPropertyValueDocument) {
      values.push('Umb.Document.PropertyValue.Read');
    }
    if (this.writePropertyValueDocument) {
      values.push('Umb.Document.PropertyValue.Write');
    }

    // Element permissions
    if (this.readElement) {
      values.push('Umb.Element.Read');
    }
    if (this.createElement) {
      values.push('Umb.Element.Create');
    }
    if (this.deleteElement) {
      values.push('Umb.Element.Delete');
    }
    if (this.publishElement) {
      values.push('Umb.Element.Publish');
    }
    if (this.unpublishElement) {
      values.push('Umb.Element.Unpublish');
    }
    if (this.updateElement) {
      values.push('Umb.Element.Update');
    }
    if (this.duplicateElement) {
      values.push('Umb.Element.Duplicate');
    }
    if (this.moveElement) {
      values.push('Umb.Element.Move');
    }
    if (this.rollbackElement) {
      values.push('Umb.Element.Rollback');
    }

    // Element Container permissions
    if (this.readElementContainer) {
      values.push('Umb.ElementContainer.Read');
    }
    if (this.createElementContainer) {
      values.push('Umb.ElementContainer.Create');
    }
    if (this.deleteElementContainer) {
      values.push('Umb.ElementContainer.Delete');
    }
    if (this.updateElementContainer) {
      values.push('Umb.ElementContainer.Update');
    }
    if (this.moveElementContainer) {
      values.push('Umb.ElementContainer.Move');
    }

    // Element folder (container) permissions
    if (this.readElementFolder) {
      values.push('Umb.ElementContainer.Read');
    }
    if (this.deleteElementFolder) {
      values.push('Umb.ElementContainer.Delete');
    }

    return values;
  }
}
