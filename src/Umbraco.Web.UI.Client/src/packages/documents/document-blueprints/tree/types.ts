import type { UmbDocumentBlueprintEntityType, UmbDocumentBlueprintFolderEntityType } from '../entity.js';
import type { UmbUniqueTreeItemModel, UmbUniqueTreeRootModel } from '@umbraco-cms/backoffice/tree';

export interface UmbDocumentBlueprintTreeItemModel extends UmbUniqueTreeItemModel {
	entityType: UmbDocumentBlueprintEntityType | UmbDocumentBlueprintFolderEntityType;
}
