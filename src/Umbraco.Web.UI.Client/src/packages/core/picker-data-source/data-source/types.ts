import type { UmbItemDataResolver, UmbItemModel } from '@umbraco-cms/backoffice/entity-item';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbItemRepository } from '@umbraco-cms/backoffice/repository';
import type { UmbConfigCollectionModel } from '@umbraco-cms/backoffice/utils';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export interface UmbPickerDataSource<PickedItemType extends UmbItemModel = UmbItemModel>
	extends UmbItemRepository<PickedItemType>,
		UmbApi {
	setConfig?(config: UmbConfigCollectionModel | undefined): void;
	getConfig?(): UmbConfigCollectionModel | undefined;

	/**
	 * Creates an item data resolver for this data source, bound to the given host.
	 * Pass the consuming element or context as host so the resolver can reach local DOM contexts (e.g. `UMB_VARIANT_CONTEXT`).
	 * @param {UmbControllerHost} host The controller host of the picker consumer.
	 * @returns {UmbItemDataResolver} A resolver that can provide the display name, icon, and other metadata for an item.
	 */
	createItemDataResolver?(host: UmbControllerHost): UmbItemDataResolver;
}
