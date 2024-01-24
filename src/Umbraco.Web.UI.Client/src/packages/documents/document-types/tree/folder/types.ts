import { UMB_DOCUMENT_TYPE_FOLDER_ENTITY_TYPE } from '../../entity.js';
import { UmbDocumentTypeTreeItemModel } from '../types.js';

export interface UmbDocumentTypeFolderTreeItemModel extends UmbDocumentTypeTreeItemModel {
	entityType: typeof UMB_DOCUMENT_TYPE_FOLDER_ENTITY_TYPE;
}
