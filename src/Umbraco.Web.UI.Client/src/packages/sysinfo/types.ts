import type { UpgradeCheckResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

/**
 * @deprecated This type is deprecated and will be removed in Umbraco 19. It is no longer possible to check for updates from the backoffice.
 */
export type UmbServerUpgradeCheck = UpgradeCheckResponseModel & {
	expires: string;
	version?: string;
	createdAt?: string;
};
