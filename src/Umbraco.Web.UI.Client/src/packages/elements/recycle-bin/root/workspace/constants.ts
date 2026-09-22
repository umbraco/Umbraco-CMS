import { UMB_ELEMENT_RECYCLE_BIN_ROOT_ENTITY_TYPE } from '../entity.js';
import { UMB_LIBRARY_SECTION_PATHNAME } from '@umbraco-cms/backoffice/library';
import { UMB_WORKSPACE_PATH_PATTERN } from '@umbraco-cms/backoffice/workspace';

export const UMB_ELEMENT_RECYCLE_BIN_ROOT_WORKSPACE_ALIAS = 'Umb.Workspace.Element.RecycleBin.Root';

export const UMB_ELEMENT_RECYCLE_BIN_ROOT_WORKSPACE_PATH = UMB_WORKSPACE_PATH_PATTERN.generateAbsolute({
	sectionName: UMB_LIBRARY_SECTION_PATHNAME,
	entityType: UMB_ELEMENT_RECYCLE_BIN_ROOT_ENTITY_TYPE,
});
