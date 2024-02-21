import type { UmbMediaCollectionFilterModel, UmbMediaCollectionItemModel } from './types.js';
import { UmbDefaultCollectionContext } from '@umbraco-cms/backoffice/collection';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbMediaCollectionContext extends UmbDefaultCollectionContext<
	UmbMediaCollectionItemModel,
	UmbMediaCollectionFilterModel
> {
	constructor(host: UmbControllerHost) {
		super(host, 'Umb.CollectionView.MediaGrid');

		this.selection.setSelectable(true);
	}
}
