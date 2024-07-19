import type {
	UmbDocumentTypeEntityType,
	UmbDocumentTypeFolderEntityType,
	UmbDocumentTypeRootEntityType,
} from '../entity.js';
import type { UmbTreeItemModel, UmbTreeRootModel } from '@umbraco-cms/backoffice/tree';

export interface UmbDocumentTypeTreeItemModel extends UmbTreeItemModel {
	entityType: UmbDocumentTypeEntityType | UmbDocumentTypeFolderEntityType;
	isElement: boolean;
}

export interface UmbDocumentTypeTreeRootModel extends UmbTreeRootModel {
	entityType: UmbDocumentTypeRootEntityType;
}
