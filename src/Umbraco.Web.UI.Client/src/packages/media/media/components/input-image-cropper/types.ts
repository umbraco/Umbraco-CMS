export type UmbImageCropperPropertyEditorValue = {
	crops: Array<{
		alias: string;
		coordinates?: {
			x1: number;
			x2: number;
			y1: number;
			y2: number;
		};
		height: number;
		width: number;
	}>;
	focalPoint: { left: number; top: number };
	src: string;
};

export type UmbImageCropperCrop = UmbImageCropperPropertyEditorValue['crops'][number];
export type UmbImageCropperCrops = UmbImageCropperPropertyEditorValue['crops'];
export type UmbImageCropperFocalPoint = UmbImageCropperPropertyEditorValue['focalPoint'];
