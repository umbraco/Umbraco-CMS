import type { UmbRepositoryBase } from '../repository/repository-base.js';
import type {
	UmbRecycleBinOriginalParentRequestArgs,
	UmbRecycleBinRestoreRequestArgs,
	UmbRecycleBinTrashRequestArgs,
} from './types.js';
import type { UmbRepositoryErrorResponse, UmbRepositoryResponse } from '@umbraco-cms/backoffice/repository';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbRecycleBinRepository extends UmbRepositoryBase, UmbApi {
	requestTrash(args: UmbRecycleBinTrashRequestArgs): Promise<UmbRepositoryErrorResponse>;
	requestRestore(args: UmbRecycleBinRestoreRequestArgs): Promise<UmbRepositoryErrorResponse>;
	requestEmpty(): Promise<UmbRepositoryErrorResponse>;
	requestOriginalParent(args: UmbRecycleBinOriginalParentRequestArgs): Promise<UmbRepositoryResponse<any>>;
}
