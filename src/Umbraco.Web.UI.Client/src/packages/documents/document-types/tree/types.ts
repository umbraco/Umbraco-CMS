import {
	UmbDocumentTypeEntityType,
	UmbDocumentTypeFolderEntityType,
	UmbDocumentTypeRootEntityType,
} from '../entity.js';
import { UmbUniqueTreeItemModel, UmbUniqueTreeRootModel } from '@umbraco-cms/backoffice/tree';

export interface UmbDocumentTypeTreeItemModel extends UmbUniqueTreeItemModel {
	entityType: UmbDocumentTypeEntityType | UmbDocumentTypeFolderEntityType;
	isElement: boolean;
}

export interface UmbDocumentTypeTreeRootModel extends UmbUniqueTreeRootModel {
	entityType: UmbDocumentTypeRootEntityType;
}
