import { UMB_MEDIA_ITEM_REPOSITORY_ALIAS } from '../../repository/index.js';
import type { UmbCropModel } from '../../property-editors/index.js';
import {
	UMB_MEDIA_PICKER_MODAL,
	type UmbMediaCardItemModel,
	type UmbMediaPickerModalData,
	type UmbMediaPickerModalValue,
} from '../../modals/index.js';
import type { UmbImageCropperCrop, UmbImageCropperCrops } from '../input-image-cropper/types.js';
import { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbArrayState, UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';
import { UmbImagingRepository } from '@umbraco-cms/backoffice/imaging';
import { ImageCropModeModel } from '@umbraco-cms/backoffice/external/backend-api';

interface UmbRichMediaItemModel extends UmbMediaCardItemModel {
	crops: UmbImageCropperCrops;
	focalPoint?: { left: number; top: number };
}

export class UmbRichMediaPickerContext extends UmbPickerInputContext<
	UmbRichMediaItemModel,
	UmbRichMediaItemModel,
	UmbMediaPickerModalData<UmbRichMediaItemModel>,
	UmbMediaPickerModalValue
> {
	#imagingRepository: UmbImagingRepository;

	#selectedRichItems = new UmbArrayState<UmbRichMediaItemModel>([], (x) => x.unique);
	readonly richItems = this.#selectedRichItems.asObservable();

	#preselectedCrops = new UmbArrayState<UmbCropModel>([], (x) => x.alias);
	readonly preselectedCrops = this.#preselectedCrops.asObservable();

	#focalPointEnabled = new UmbBooleanState<boolean>(false);
	readonly focalPointEnabled = this.#focalPointEnabled.asObservable();

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
					url: url ?? '',
					crops: previous?.crops ?? [],
					focalPoint: previous?.focalPoint,
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

	updateFocalPointOfSelectedRichItem(unique: string, focalPoint: { left: number; top: number }) {
		this.#selectedRichItems.updateOne(unique, { focalPoint });
	}

	updateCropsOfSelectedRichItem(unique: string, crops: UmbImageCropperCrops) {
		this.#selectedRichItems.updateOne(unique, { crops });
	}

	updateOneCropOfSelectedRichItem(unique: string, alias: string, newCrop: UmbImageCropperCrop) {
		const item = this.#selectedRichItems.getValue().find((item) => item.unique);
		if (!item) return;

		const crops = item.crops.map((crop) => {
			if (crop.alias !== alias) return crop;
			return newCrop;
		});
		this.#selectedRichItems.updateOne(unique, { crops });
	}
}
