import type {
	UmbDocumentTypeEntityType,
	UmbDocumentTypeFolderEntityType,
	UmbDocumentTypeRootEntityType,
} from '../entity.js';
import type { UmbUniqueTreeItemModel, UmbUniqueTreeRootModel } from '@umbraco-cms/backoffice/tree';

export interface UmbDocumentTypeTreeItemModel extends UmbUniqueTreeItemModel {
	entityType: UmbDocumentTypeEntityType | UmbDocumentTypeFolderEntityType;
	isElement: boolean;
}

export interface UmbDocumentTypeTreeRootModel extends UmbUniqueTreeRootModel {
	entityType: UmbDocumentTypeRootEntityType;
}
