export type UmbMediaPickerPropertyValueEntry = {
	key: string;
	mediaKey: string;
	mediaTypeAlias: string;
	focalPoint: UmbFocalPointModel | null;
	crops: Array<UmbCropModel>;
	altText?: string;
	altTextByCulture?: Record<string, string>;
};

export type UmbMediaPickerValueModel = Array<UmbMediaPickerPropertyValueEntry>;

export type UmbCropModel = {
	label?: string;
	alias: string;
	height: number;
	width: number;
	coordinates?: {
		x1: number;
		x2: number;
		y1: number;
		y2: number;
	};
	altText?: string;
	/** Per-culture alternative text for this crop. Keys are ISO culture codes (e.g. 'en-US', 'da-DK'). */
	altTextByCulture?: Record<string, string>;
};

export interface UmbFocalPointModel {
	left: number;
	top: number;
}
