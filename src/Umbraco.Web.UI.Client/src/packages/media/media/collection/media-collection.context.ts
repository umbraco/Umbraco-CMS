import type { UmbMediaCollectionFilterModel, UmbMediaCollectionItemModel } from './types.js';
import { UMB_MEDIA_GRID_COLLECTION_VIEW_ALIAS } from './views/index.js';
import { UmbDefaultCollectionContext } from '@umbraco-cms/backoffice/collection';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbMediaCollectionContext extends UmbDefaultCollectionContext<
	UmbMediaCollectionItemModel,
	UmbMediaCollectionFilterModel
> {
	/**
	 * The thumbnail items that are currently displayed in the collection.
	 * @deprecated Use the `<umb-imaging-thumbnail>` element instead.
	 */
	public readonly thumbnailItems = this.items;

	constructor(host: UmbControllerHost) {
		super(host, UMB_MEDIA_GRID_COLLECTION_VIEW_ALIAS);
	}
}

export { UmbMediaCollectionContext as api };
