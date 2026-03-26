import type { MemberTypeResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbManagementApiDetailDataCache } from '@umbraco-cms/backoffice/management-api';

const memberTypeDetailCache = new UmbManagementApiDetailDataCache<MemberTypeResponseModel>();

export { memberTypeDetailCache };
