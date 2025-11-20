import type { MemberTypeItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbManagementApiItemDataCache } from '@umbraco-cms/backoffice/management-api';

const memberTypeItemCache = new UmbManagementApiItemDataCache<MemberTypeItemResponseModel>();

export { memberTypeItemCache };
