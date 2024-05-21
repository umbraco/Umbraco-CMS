import {
	UMB_MEDIA_PICKER_MODAL,
	type UmbMediaPickerModalData,
	type UmbMediaPickerModalValue,
	type UmbMediaCardItemModel,
} from '../../modals/index.js';
import { UMB_MEDIA_ITEM_REPOSITORY_ALIAS } from '../../repository/index.js';
import type { UmbMediaItemModel } from '../../repository/item/types.js';
import { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { UmbImagingRepository } from '@umbraco-cms/backoffice/imaging';
import { ImageCropModeModel } from '@umbraco-cms/backoffice/external/backend-api';

export class UmbMediaPickerContext extends UmbPickerInputContext<
	UmbMediaItemModel,
	UmbMediaItemModel,
	UmbMediaPickerModalData<UmbMediaItemModel>,
	UmbMediaPickerModalValue
> {
	#imagingRepository: UmbImagingRepository;

	#cardItems = new UmbArrayState<UmbMediaCardItemModel>([], (x) => x.unique);
	readonly cardItems = this.#cardItems.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_MEDIA_ITEM_REPOSITORY_ALIAS, UMB_MEDIA_PICKER_MODAL);
		this.#imagingRepository = new UmbImagingRepository(host);

		this.observe(this.selectedItems, async (selectedItems) => {
			if (!selectedItems?.length) {
				this.#cardItems.setValue([]);
				return;
			}
			const { data } = await this.#imagingRepository.requestResizedItems(
				selectedItems.map((x) => x.unique),
				{ height: 400, width: 400, mode: ImageCropModeModel.MIN },
			);

			this.#cardItems.setValue(
				selectedItems.map((item) => {
					const url = data?.find((x) => x.unique === item.unique)?.url;
					return { ...item, url };
				}),
			);
		});
	}
}
