import type { UpgradeCheckResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

export type UmbServerUpgradeCheck = UpgradeCheckResponseModel & {
	expires: string;
	version?: string;
	createdAt?: string;
};
