import type {
	UmbDocumentBlueprintEntityType,
	UmbDocumentBlueprintFolderEntityType,
	UmbDocumentBlueprintRootEntityType,
} from '../entity.js';
import type { UmbUniqueTreeItemModel, UmbUniqueTreeRootModel } from '@umbraco-cms/backoffice/tree';

export interface UmbDocumentBlueprintTreeItemModel extends UmbUniqueTreeItemModel {
	entityType: UmbDocumentBlueprintEntityType | UmbDocumentBlueprintFolderEntityType;
}

export interface UmbDocumentBlueprintTreeRootModel extends UmbUniqueTreeRootModel {
	entityType: UmbDocumentBlueprintRootEntityType;
}
