import type { UUIModalSidebarSize } from '@umbraco-cms/backoffice/external/uui';

export interface UmbBlockTypeBase {
	contentElementTypeKey: string;
	settingsElementTypeKey?: string;
	label?: string;
	view?: string;
	stylesheet?: string;
	iconColor?: string;
	backgroundColor?: string;
	editorSize?: UUIModalSidebarSize;
	icon?: string;
}
