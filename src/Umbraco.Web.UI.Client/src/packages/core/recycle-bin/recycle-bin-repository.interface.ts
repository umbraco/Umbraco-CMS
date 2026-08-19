import type {
	UmbRecycleBinOriginalParentRequestArgs,
	UmbRecycleBinRestoreRequestArgs,
	UmbRecycleBinTrashRequestArgs,
} from './types.js';
import type {
	UmbRepositoryBase,
	UmbRepositoryErrorResponse,
	UmbRepositoryResponse,
} from '@umbraco-cms/backoffice/repository';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbRecycleBinRepository extends UmbRepositoryBase, UmbApi {
	/**
	 * @param args The item to trash.
	 * @param abortSignal Aborts the request.
	 */
	requestTrash(args: UmbRecycleBinTrashRequestArgs, abortSignal?: AbortSignal): Promise<UmbRepositoryErrorResponse>;
	requestRestore(args: UmbRecycleBinRestoreRequestArgs): Promise<UmbRepositoryErrorResponse>;
	requestEmpty(): Promise<UmbRepositoryErrorResponse>;
	requestOriginalParent(args: UmbRecycleBinOriginalParentRequestArgs): Promise<UmbRepositoryResponse<any>>;
}
