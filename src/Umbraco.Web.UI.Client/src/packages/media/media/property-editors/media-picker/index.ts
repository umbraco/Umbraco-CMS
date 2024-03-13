export * from './property-editor-ui-media-picker.element.js';

export type UmbMediaPickerPropertyValue = {
	key: string;
	mediaKey: string;
	mediaTypeAlias: string;
	focalPoint: { left: number; top: number } | null;
	crops: Array<{ alias: string; width: number; height: number }>;
};
