import { UMB_DOCUMENT_RECYCLE_BIN_ROOT_ENTITY_TYPE } from '../entity.js';
import { UMB_DOCUMENTS_SECTION_PATHNAME } from '../../../../section/paths.js';
import { UMB_WORKSPACE_PATH_PATTERN } from '@umbraco-cms/backoffice/workspace';

export const UMB_DOCUMENT_RECYCLE_BIN_ROOT_WORKSPACE_ALIAS = 'Umb.Workspace.Document.RecycleBin.Root';

export const UMB_DOCUMENT_RECYCLE_BIN_ROOT_WORKSPACE_PATH = UMB_WORKSPACE_PATH_PATTERN.generateAbsolute({
	sectionName: UMB_DOCUMENTS_SECTION_PATHNAME,
	entityType: UMB_DOCUMENT_RECYCLE_BIN_ROOT_ENTITY_TYPE,
});
