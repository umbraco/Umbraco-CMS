import type {
	ServerConfigurationResponseModel,
	UpgradeCheckResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';

export type UmbServerConfiguration = ServerConfigurationResponseModel;
export type UmbServerUpgradeCheck = UpgradeCheckResponseModel & { expires: string };
