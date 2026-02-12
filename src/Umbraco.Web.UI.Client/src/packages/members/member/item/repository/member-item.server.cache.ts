import type { MemberItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbManagementApiItemDataCache } from '@umbraco-cms/backoffice/management-api';

const memberItemCache = new UmbManagementApiItemDataCache<MemberItemResponseModel>();

export { memberItemCache };
