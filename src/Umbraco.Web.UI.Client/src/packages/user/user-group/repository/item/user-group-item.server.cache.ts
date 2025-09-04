import type { UserGroupItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbManagementApiItemDataCache } from '@umbraco-cms/backoffice/management-api';

const userGroupItemCache = new UmbManagementApiItemDataCache<UserGroupItemResponseModel>();

export { userGroupItemCache };
