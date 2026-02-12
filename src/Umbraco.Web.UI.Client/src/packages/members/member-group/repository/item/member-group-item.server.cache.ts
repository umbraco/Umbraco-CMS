import type { MemberGroupItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbManagementApiItemDataCache } from '@umbraco-cms/backoffice/management-api';

const memberGroupItemCache = new UmbManagementApiItemDataCache<MemberGroupItemResponseModel>();

export { memberGroupItemCache };
