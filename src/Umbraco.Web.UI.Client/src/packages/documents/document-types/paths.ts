import { UMB_DOCUMENT_TYPE_ENTITY_TYPE, type UmbDocumentTypeEntityTypeUnion } from './entity.js';
import { UMB_SETTINGS_SECTION_PATHNAME } from '@umbraco-cms/backoffice/settings';
import { UmbPathPattern } from '@umbraco-cms/backoffice/router';
import { UMB_WORKSPACE_PATH_PATTERN } from '@umbraco-cms/backoffice/workspace';

export const UMB_CREATE_DOCUMENT_TYPE_WORKSPACE_PRESET_TEMPLATE = 'template';
export const UMB_CREATE_DOCUMENT_TYPE_WORKSPACE_PRESET_ELEMENT = 'element';

export type UmbCreateDocumentTypeWorkspacePresetTemplateType =
	typeof UMB_CREATE_DOCUMENT_TYPE_WORKSPACE_PRESET_TEMPLATE;
export type UmbCreateDocumentTypeWorkspacePresetElementType = // line break thanks!
	typeof UMB_CREATE_DOCUMENT_TYPE_WORKSPACE_PRESET_ELEMENT;

export type UmbCreateDocumentTypeWorkspacePresetType =
	| UmbCreateDocumentTypeWorkspacePresetTemplateType
	| UmbCreateDocumentTypeWorkspacePresetElementType;

export const UMB_DOCUMENT_TYPE_WORKSPACE_PATH = UMB_WORKSPACE_PATH_PATTERN.generateAbsolute({
	sectionName: UMB_SETTINGS_SECTION_PATHNAME,
	entityType: UMB_DOCUMENT_TYPE_ENTITY_TYPE,
});

export const UMB_CREATE_DOCUMENT_TYPE_WORKSPACE_PATH_PATTERN = new UmbPathPattern<{
	parentEntityType: UmbDocumentTypeEntityTypeUnion;
	parentUnique?: string | null;
	presetAlias?: UmbCreateDocumentTypeWorkspacePresetType | null;
}>('create/parent/:parentEntityType/:parentUnique/:presetAlias', UMB_DOCUMENT_TYPE_WORKSPACE_PATH);

export const UMB_EDIT_DOCUMENT_TYPE_WORKSPACE_PATH_PATTERN = new UmbPathPattern<{ unique: string }>(
	'edit/:unique',
	UMB_DOCUMENT_TYPE_WORKSPACE_PATH,
);
