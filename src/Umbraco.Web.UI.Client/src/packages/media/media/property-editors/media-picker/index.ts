export * from './property-editor-ui-media-picker.element.js';

export type UmbMediaPickerPropertyValue = {
	key: string;
	mediaKey: string;
	mediaTypeAlias: string;
	focalPoint: UmbFocalPointModel | null;
	crops: Array<UmbCropModel>;
};

export interface UmbCropModel {
	label: string;
	alias: string;
	width: number;
	height: number;
}

export interface UmbFocalPointModel {
	left: number;
	top: number;
}
