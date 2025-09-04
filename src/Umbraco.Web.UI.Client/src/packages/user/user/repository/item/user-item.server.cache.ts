import type { UserItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbManagementApiItemDataCache } from '@umbraco-cms/backoffice/management-api';

const userItemCache = new UmbManagementApiItemDataCache<UserItemResponseModel>();

export { userItemCache };
