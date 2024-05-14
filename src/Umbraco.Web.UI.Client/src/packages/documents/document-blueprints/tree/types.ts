import type { UmbDocumentBlueprintEntityType, UmbDocumentBlueprintFolderEntityType } from '../entity.js';
import type { UmbTreeItemModel, UmbTreeRootModel } from '@umbraco-cms/backoffice/tree';

export interface UmbDocumentBlueprintTreeRootModel extends UmbTreeRootModel {}

export interface UmbDocumentBlueprintTreeItemModel extends UmbTreeItemModel {
	entityType: UmbDocumentBlueprintEntityType | UmbDocumentBlueprintFolderEntityType;
}
