import type { UmbRepositoryBase } from '../repository/repository-base.js';
import type { UmbRepositoryErrorResponse } from '@umbraco-cms/backoffice/repository';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbRecycleBinRepository extends UmbRepositoryBase, UmbApi {
	requestTrash(args: any): Promise<UmbRepositoryErrorResponse>;
	requestRestore(args: any): Promise<UmbRepositoryErrorResponse>;
	requestEmpty(): Promise<UmbRepositoryErrorResponse>;
}
