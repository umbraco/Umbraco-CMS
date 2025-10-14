import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbItemRepository } from '@umbraco-cms/backoffice/repository';
import type { UmbConfigCollectionModel } from '@umbraco-cms/backoffice/utils';

export interface UmbPickerDataSource extends UmbItemRepository<any>, UmbApi {
	setConfig?(config: UmbConfigCollectionModel | undefined): void;
	getConfig?(): UmbConfigCollectionModel | undefined;
}
