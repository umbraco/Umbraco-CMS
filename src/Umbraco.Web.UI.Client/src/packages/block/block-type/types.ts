import type { UUIModalSidebarSize } from '@umbraco-cms/backoffice/external/uui';

export interface UmbBlockTypeBaseModel {
	contentElementTypeKey: string;
	settingsElementTypeKey?: string;
	label?: string;
	view?: string; // TODO: remove/replace with custom element manifest type for block list.
	stylesheet?: string; // TODO: remove/replace with custom element manifest type for block list.
	iconColor?: string;
	backgroundColor?: string;
	editorSize?: UUIModalSidebarSize;
	icon?: string; // remove later
}

export interface UmbBlockTypeGroup {
	name?: string | null;
	key: string;
}

export interface UmbBlockTypeWithGroupKey extends UmbBlockTypeBaseModel {
	groupKey?: string | null;
}
