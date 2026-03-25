import type { UmbCropModel, UmbFocalPointModel } from '../../types.js';

export type UmbImageCropperPropertyEditorValue = {
	temporaryFileId?: string | null;
	crops: Array<UmbCropModel>;
	focalPoint: UmbFocalPointModel | null;
	src: string;
};

export type UmbImageCropperCrop = UmbImageCropperPropertyEditorValue['crops'][number];
export type UmbImageCropperCrops = UmbImageCropperPropertyEditorValue['crops'];
export type UmbImageCropperFocalPoint = UmbImageCropperPropertyEditorValue['focalPoint'];
