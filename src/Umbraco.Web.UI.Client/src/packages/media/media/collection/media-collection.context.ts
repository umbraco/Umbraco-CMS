import type { UmbMediaCollectionFilterModel, UmbMediaCollectionItemModel } from './types.js';
import { UMB_MEDIA_GRID_COLLECTION_VIEW_ALIAS } from './views/index.js';
import { UmbDefaultCollectionContext } from '@umbraco-cms/backoffice/collection';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbMediaCollectionContext extends UmbDefaultCollectionContext<
	UmbMediaCollectionItemModel,
	UmbMediaCollectionFilterModel
> {
	constructor(host: UmbControllerHost) {
		super(host, UMB_MEDIA_GRID_COLLECTION_VIEW_ALIAS);

		this.selection.setSelectable(true);
	}
}
