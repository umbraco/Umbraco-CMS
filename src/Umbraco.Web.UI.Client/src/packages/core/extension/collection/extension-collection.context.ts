import { UMB_EXTENSION_COLLECTION_EXTENSION_TYPE_FACET_FILTER_ALIAS } from './filter/constants.js';
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
		const filterValues = await this.filtering.getActiveFilterValues();
		const args: Record<string, any> = {};

		const extensionTypeFilters = filterValues.filter(
			(f) => f.alias === UMB_EXTENSION_COLLECTION_EXTENSION_TYPE_FACET_FILTER_ALIAS,
		);
		if (extensionTypeFilters.length) args.extensionTypes = extensionTypeFilters.map((f) => f.value);

		return args;
	}
}

export { UmbExtensionCollectionContext as api };
