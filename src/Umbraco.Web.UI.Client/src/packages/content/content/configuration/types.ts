import type { UmbRepositoryResponse } from '@umbraco-cms/backoffice/repository';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbContentConfigurationModel {
	disableUnpublishWhenReferenced: boolean;
}

export interface UmbContentConfigurationRepository extends UmbApi {
	requestConfiguration(): Promise<UmbRepositoryResponse<UmbContentConfigurationModel>>;
}
