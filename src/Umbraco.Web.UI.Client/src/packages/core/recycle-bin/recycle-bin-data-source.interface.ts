import type {
	UmbRecycleBinOriginalParentRequestArgs,
	UmbRecycleBinRestoreRequestArgs,
	UmbRecycleBinTrashRequestArgs,
} from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';

export interface UmbRecycleBinDataSourceConstructor {
	new (host: UmbControllerHost): UmbRecycleBinDataSource;
}

export interface UmbRecycleBinDataSource {
	trash(args: UmbRecycleBinTrashRequestArgs): Promise<UmbDataSourceResponse>;
	restore(args: UmbRecycleBinRestoreRequestArgs): Promise<UmbDataSourceResponse>;
	empty(): Promise<UmbDataSourceResponse>;
	getOriginalParent(args: UmbRecycleBinOriginalParentRequestArgs): Promise<UmbDataSourceResponse>;
}
