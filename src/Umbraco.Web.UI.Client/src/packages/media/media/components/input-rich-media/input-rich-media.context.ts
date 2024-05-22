import { UMB_MEDIA_ITEM_REPOSITORY_ALIAS, type UmbMediaItemModel } from '../../repository/index.js';
import type { UmbCropModel, UmbMediaPickerPropertyValue } from '../../property-editors/index.js';
import {
	UMB_MEDIA_PICKER_MODAL,
	type UmbMediaPickerModalData,
	type UmbMediaPickerModalValue,
} from '../../modals/index.js';
import type { UmbImageCropperCrop, UmbImageCropperCrops } from '../input-image-cropper/types.js';
import type { UmbRichMediaItemModel } from './index.js';
import { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbArrayState, UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';
import { UmbImagingRepository } from '@umbraco-cms/backoffice/imaging';
import { ImageCropModeModel } from '@umbraco-cms/backoffice/external/backend-api';

export class UmbRichMediaPickerContext extends UmbPickerInputContext<
	UmbMediaItemModel,
	UmbMediaItemModel,
	UmbMediaPickerModalData<UmbMediaItemModel>,
	UmbMediaPickerModalValue
> {
	#imagingRepository: UmbImagingRepository;

	#selectedRichItems = new UmbArrayState<UmbRichMediaItemModel>([], (x) => x.key);
	readonly richItems = this.#selectedRichItems.asObservable();

	#preselectedCrops = new UmbArrayState<UmbCropModel>([], (x) => x.alias);
	readonly preselectedCrops = this.#preselectedCrops.asObservable();

	#focalPointEnabled = new UmbBooleanState<boolean>(false);
	readonly focalPointEnabled = this.#focalPointEnabled.asObservable();

	#mediaPickerValue = new UmbArrayState<UmbMediaPickerPropertyValue>([], (x) => x.key);
	readonly mediaPickerValue = this.#mediaPickerValue.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_MEDIA_ITEM_REPOSITORY_ALIAS, UMB_MEDIA_PICKER_MODAL);

		this.#imagingRepository = new UmbImagingRepository(host);

		this.observe(this.selectedItems, async (selectedItems) => {
			if (!selectedItems?.length) {
				this.#selectedRichItems.setValue([]);
				return;
			}

			const { data } = await this.#imagingRepository.requestResizedItems(
				selectedItems.map((x) => x.unique),
				{ height: 400, width: 400, mode: ImageCropModeModel.CROP },
			);
			if (!data) return;

			const previously = this.#selectedRichItems.getValue();

			const richItems: Array<UmbRichMediaItemModel> = selectedItems.map((item) => {
				const url = data.find((x) => x.unique === item.unique)?.url;
				const previous = previously.find((x) => x.unique === item.unique);

				return {
					...item,
					src: url ?? '',
					crops: previous?.crops ?? [],
					focalPoint: previous?.focalPoint ?? null,
				};
			});

			this.#selectedRichItems.setValue(richItems);
		});
	}

	setFocalPointEnabled(enabled: boolean) {
		this.#focalPointEnabled.setValue(enabled);
	}

	getFocalPointEnabled() {
		return this.#focalPointEnabled.getValue();
	}

	setPreselectedCrops(crops: Array<UmbCropModel>) {
		this.#preselectedCrops.setValue(crops);
	}

	getPreselectedCrops() {
		return this.#preselectedCrops.getValue();
	}

	updateFocalPointOf(unique: string, focalPoint: { left: number; top: number }) {
		this.#selectedRichItems.updateOne(unique, { focalPoint });
	}

	updateCropsOf(unique: string, crops: UmbImageCropperCrops) {
		this.#selectedRichItems.updateOne(unique, { crops });
	}

	updateOneCropOf(unique: string, alias: string, newCrop: UmbImageCropperCrop) {
		const item = this.#selectedRichItems.getValue().find((item) => item.unique);
		if (!item) return;

		const crops = item.crops.map((crop) => {
			if (crop.alias !== alias) return crop;
			return newCrop;
		});
		this.#selectedRichItems.updateOne(unique, { crops });
	}
}
