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
}

export interface UmbBlockGridType extends UmbBlockTypeBase {
	columnSpanOptions: Array<number>;
	allowAtRoot: boolean;
	allowInAreas: boolean;
	rowMinSpan: number;
	rowMaxSpan: number;
	thumbnail?: string;
	areaGridColumns?: number;
	areas: Array<any>;
	groupKey: null | string;
}
