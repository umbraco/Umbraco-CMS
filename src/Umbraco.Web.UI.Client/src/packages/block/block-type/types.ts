import type { UUIModalSidebarSize } from '@umbraco-cms/backoffice/external/uui';

// Shared with the Property Editor
export interface UmbBlockTypeBaseModel {
	contentElementTypeKey: string;
	settingsElementTypeKey?: string;
	label?: string;
	thumbnail?: string;
	iconColor?: string;
	backgroundColor?: string;
	editorSize?: UUIModalSidebarSize;
	forceHideContentEditorInOverlay: boolean;
}
export interface UmbBlockTypeGroup {
	name?: string;
	key: string;
}

export interface UmbBlockTypeWithGroupKey extends UmbBlockTypeBaseModel {
	groupKey?: string | null;
}
