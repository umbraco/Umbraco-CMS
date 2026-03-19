import type { UmbExtensionCollectionFilterModel, UmbExtensionCollectionItemModel } from './types.js';
import { UmbDefaultCollectionContext } from '@umbraco-cms/backoffice/collection';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

const UMB_EXTENSION_COLLECTION_VIEW_TABLE = 'Umb.CollectionView.Extension.Table';

export class UmbExtensionCollectionContext extends UmbDefaultCollectionContext<
	UmbExtensionCollectionItemModel,
	UmbExtensionCollectionFilterModel
> {
	constructor(host: UmbControllerHost) {
		super(host, UMB_EXTENSION_COLLECTION_VIEW_TABLE);
	}

	protected override async _getFilterArgs(): Promise<Record<string, any>> {
		const activeFilters = await this.filtering.getActiveFilters();
		const args: Record<string, any> = {};
		for (const filter of activeFilters) {
			if (filter.alias === 'Umb.CollectionFacetFilter.Extension.Type') {
				args.extensionTypes = filter.value.map((v: { unique: string }) => v.unique);
			}
		}
		return args;
	}
}

export { UmbExtensionCollectionContext as api };
