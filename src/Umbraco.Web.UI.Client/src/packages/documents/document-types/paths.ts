import type { UmbDocumentTypeEntityTypeUnion } from './entity.js';
import { umbUrlPatternToString } from '@umbraco-cms/backoffice/utils';

export const UMB_CREATE_DOCUMENT_TYPE_WORKSPACE_PRESET_TEMPLATE = 'template';
export const UMB_CREATE_DOCUMENT_TYPE_WORKSPACE_PRESET_ELEMENT = 'element';

export type UmbCreateDocumentTypeWorkspacePresetTemplateType =
	typeof UMB_CREATE_DOCUMENT_TYPE_WORKSPACE_PRESET_TEMPLATE;
export type UmbCreateDocumentTypeWorkspacePresetElementType = // line break thanks!
	typeof UMB_CREATE_DOCUMENT_TYPE_WORKSPACE_PRESET_ELEMENT;

export type UmbCreateDocumentTypeWorkspacePresetType =
	| UmbCreateDocumentTypeWorkspacePresetTemplateType
	| UmbCreateDocumentTypeWorkspacePresetElementType;

export const UMB_CREATE_DOCUMENT_TYPE_WORKSPACE_PATH = 'create/:entityType/:parentUnique/:presetAlias';
export const umbCreateDocumentTypeWorkspacePathGenerator = (params: {
	entityType: UmbDocumentTypeEntityTypeUnion;
	parentUnique?: string | null;
	presetAlias?: UmbCreateDocumentTypeWorkspacePresetType | null;
}) => umbUrlPatternToString(UMB_CREATE_DOCUMENT_TYPE_WORKSPACE_PATH, params);

export const UMB_EDIT_DOCUMENT_TYPE_WORKSPACE_PATH = 'edit/:id';
export const umbEditDocumentTypeWorkspacePathGenerator = (params: { id: string }) =>
	umbUrlPatternToString(UMB_CREATE_DOCUMENT_TYPE_WORKSPACE_PATH, params);
