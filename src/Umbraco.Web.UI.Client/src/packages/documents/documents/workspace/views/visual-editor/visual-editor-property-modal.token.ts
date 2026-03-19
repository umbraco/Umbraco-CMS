import { UmbModalToken } from '@umbraco-cms/backoffice/modal';
import type { UmbPropertyEditorConfig } from '@umbraco-cms/backoffice/property-editor';
import type { UmbPropertyTypeValidationModel } from '@umbraco-cms/backoffice/content-type';

export interface UmbVisualEditorPropertyInfo {
	alias: string;
	name: string;
	description?: string;
	editorUiAlias: string;
	config?: UmbPropertyEditorConfig;
	validation?: UmbPropertyTypeValidationModel;
	containerId?: string | null;
}

export interface UmbVisualEditorPropertyGroup {
	id: string;
	name: string;
	sortOrder: number;
}

export interface UmbVisualEditorPropertyModalData {
	headline: string;
	properties: UmbVisualEditorPropertyInfo[];
	groups?: UmbVisualEditorPropertyGroup[];
	values: Array<{ alias: string; value: unknown }>;
	settingsProperties?: UmbVisualEditorPropertyInfo[];
	settingsGroups?: UmbVisualEditorPropertyGroup[];
	settingsValues?: Array<{ alias: string; value: unknown }>;
}

export interface UmbVisualEditorPropertyModalValue {
	values: Array<{ alias: string; value: unknown }>;
	settingsValues?: Array<{ alias: string; value: unknown }>;
}

export const UMB_VISUAL_EDITOR_PROPERTY_MODAL = new UmbModalToken<
	UmbVisualEditorPropertyModalData,
	UmbVisualEditorPropertyModalValue
>('Umb.Modal.VisualEditorProperty', {
	modal: {
		type: 'sidebar',
		size: 'small',
	},
});
