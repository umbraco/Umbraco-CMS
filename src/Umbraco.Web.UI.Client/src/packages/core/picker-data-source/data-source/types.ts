import type { UmbItemModel } from '@umbraco-cms/backoffice/entity-item';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbItemRepository } from '@umbraco-cms/backoffice/repository';
import type { UmbConfigCollectionModel } from '@umbraco-cms/backoffice/utils';

export interface UmbPickerDataSource<PickedItemType extends UmbItemModel = UmbItemModel>
	extends UmbItemRepository<PickedItemType>,
		UmbApi {
	setConfig?(config: UmbConfigCollectionModel | undefined): void;
	getConfig?(): UmbConfigCollectionModel | undefined;
}
