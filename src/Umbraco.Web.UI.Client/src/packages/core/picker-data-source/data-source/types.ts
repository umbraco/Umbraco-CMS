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
	createItemDataResolver?(host: UmbControllerHost): UmbItemDataResolver;
}
