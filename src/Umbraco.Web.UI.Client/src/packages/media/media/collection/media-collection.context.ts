import type { UmbMediaCollectionFilterModel, UmbMediaCollectionItemModel } from './types.js';
import { UMB_MEDIA_GRID_COLLECTION_VIEW_ALIAS } from './views/index.js';
import { UmbImagingRepository } from '@umbraco-cms/backoffice/imaging';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { UmbDefaultCollectionContext } from '@umbraco-cms/backoffice/collection';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { ImageCropModeModel } from '@umbraco-cms/backoffice/external/backend-api';

export class UmbMediaCollectionContext extends UmbDefaultCollectionContext<
	UmbMediaCollectionItemModel,
	UmbMediaCollectionFilterModel
> {
	#imagingRepository: UmbImagingRepository;

	#thumbnailItems = new UmbArrayState<UmbMediaCollectionItemModel>([], (x) => x);
	public readonly thumbnailItems = this.#thumbnailItems.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_MEDIA_GRID_COLLECTION_VIEW_ALIAS);
		this.#imagingRepository = new UmbImagingRepository(host);

		this.observe(this.items, async (items) => {
			if (!items?.length) return;

			const { data } = await this.#imagingRepository.requestResizedItems(
				items.map((m) => m.unique),
				{ height: 400, width: 400, mode: ImageCropModeModel.MIN },
			);

			this.#thumbnailItems.setValue(
				items.map((item) => {
					const thumbnail = data?.find((m) => m.unique === item.unique)?.url;
					return { ...item, url: thumbnail };
				}),
			);
		});
	}
}

export { UmbMediaCollectionContext as api };
