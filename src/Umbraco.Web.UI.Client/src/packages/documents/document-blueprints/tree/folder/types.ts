import type { UMB_DOCUMENT_BLUEPRINT_FOLDER_ENTITY_TYPE } from '../../entity.js';
import type { UmbDocumentBlueprintTreeItemModel } from '../types.js';

export interface UmbDocumentBlueprintFolderTreeItemModel extends UmbDocumentBlueprintTreeItemModel {
	entityType: typeof UMB_DOCUMENT_BLUEPRINT_FOLDER_ENTITY_TYPE;
}
