export * from './property-editor-ui-media-picker.element.js';

export type UmbMediaPickerPropertyValue = {
	key: string;
	mediaKey: string;
	mediaTypeAlias: string;
	focalPoint: UmbFocalPointModel | null;
	crops: Array<UmbCropModel>;
};

export type UmbCropModel = {
	alias: string;
	height: number;
	width: number;
	coordinates?: {
		x1: number;
		x2: number;
		y1: number;
		y2: number;
	};
};

export interface UmbConfiguredCropModel {
	label: string;
	alias: string;
	width: number;
	height: number;
}

export interface UmbFocalPointModel {
	left: number;
	top: number;
}
