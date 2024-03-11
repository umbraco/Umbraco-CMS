import type {
	UmbDocumentBlueprintEntityType,
	UmbDocumentBlueprintRootEntityType,
	UmbDocumentBlueprintItemEntityType,
} from '../entity.js';
import type { UmbUniqueTreeItemModel, UmbUniqueTreeRootModel } from '@umbraco-cms/backoffice/tree';

export interface UmbDocumentBlueprintTreeItemModel extends UmbUniqueTreeItemModel {
	entityType: UmbDocumentBlueprintEntityType;
}

export interface UmbDocumentBlueprintItemTreeItemModel extends UmbUniqueTreeItemModel {
	entityType: UmbDocumentBlueprintItemEntityType;
}

export interface UmbDocumentBlueprintTreeRootModel extends UmbUniqueTreeRootModel {
	entityType: UmbDocumentBlueprintRootEntityType;
}
