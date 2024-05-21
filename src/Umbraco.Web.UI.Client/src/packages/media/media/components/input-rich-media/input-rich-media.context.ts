import { UMB_MEDIA_ITEM_REPOSITORY_ALIAS, type UmbMediaItemModel } from '../../repository/index.js';
import {
	UMB_MEDIA_PICKER_MODAL,
	type UmbMediaCardItemModel,
	type UmbMediaPickerModalData,
	type UmbMediaPickerModalValue,
} from '../../modals/index.js';

import type { UmbImageCropperCrop, UmbImageCropperCrops } from '../input-image-cropper/types.js';
import { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { UmbImagingRepository } from '@umbraco-cms/backoffice/imaging';
import { ImageCropModeModel } from '@umbraco-cms/backoffice/external/backend-api';

interface UmbRichMediaItemModel extends UmbMediaCardItemModel {
	crops: UmbImageCropperCrops;
	focalPoint: { left: number; top: number };
}

export class UmbRichMediaPickerContext extends UmbPickerInputContext<
	UmbMediaItemModel,
	UmbMediaItemModel,
	UmbMediaPickerModalData<UmbMediaItemModel>,
	UmbMediaPickerModalValue
> {
	#imagingRepository: UmbImagingRepository;

	#richItems = new UmbArrayState<UmbRichMediaItemModel>([], (x) => x.unique);
	readonly richItems = this.#richItems.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_MEDIA_ITEM_REPOSITORY_ALIAS, UMB_MEDIA_PICKER_MODAL);
		this.#imagingRepository = new UmbImagingRepository(host);

		this.observe(this.selectedItems, async (selectedItems) => {
			if (!selectedItems?.length) {
				this.#richItems.setValue([]);
				return;
			}
			const { data } = await this.#imagingRepository.requestResizedItems(
				selectedItems.map((x) => x.unique),
				{ height: 400, width: 400, mode: ImageCropModeModel.CROP },
			);
		});
	}

	setFocalPoint(unique: string, focalPoint: { left: number; top: number }) {
		this.#richItems.updateOne(unique, { focalPoint });
	}

	setCrops(unique: string, crops: UmbImageCropperCrops) {
		this.#richItems.updateOne(unique, { crops });
	}

	setOneCrop(unique: string, alias: string, newCrop: UmbImageCropperCrop) {
		const item = this.#richItems.getValue().find((item) => item.unique);
		if (!item) return;

		const crops = item.crops.map((crop) => {
			if (crop.alias !== alias) return crop;
			return newCrop;
		});
		this.#richItems.updateOne(unique, { crops });
	}
}
