import type { UmbMediaPickerCollectionFilterModel, UmbMediaPickerCollectionItemModel } from './types.js';
import { UMB_MEDIA_PICKER_GRID_COLLECTION_VIEW_ALIAS } from './views/index.js';
import { UmbDefaultCollectionContext } from '@umbraco-cms/backoffice/collection';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbMediaPickerCollectionContext extends UmbDefaultCollectionContext<
	UmbMediaPickerCollectionItemModel,
	UmbMediaPickerCollectionFilterModel
> {
	constructor(host: UmbControllerHost) {
		super(host, UMB_MEDIA_PICKER_GRID_COLLECTION_VIEW_ALIAS);
	}
}
