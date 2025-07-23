export type UmbMediaPickerPropertyValueEntry = {
	key: string;
	mediaKey: string;
	mediaTypeAlias: string;
	focalPoint: UmbFocalPointModel | null;
	crops: Array<UmbCropModel>;
};

/**
 * @deprecated Use UmbMediaPickerPropertyValueEntry instead — Will be removed in v.17.
 * Also notice this is a modal for the entry type, use UmbMediaPickerPropertyValueModel for the type of the value.
 */
export type UmbMediaPickerPropertyValue = UmbMediaPickerPropertyValueEntry;

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
};

export interface UmbFocalPointModel {
	left: number;
	top: number;
}
