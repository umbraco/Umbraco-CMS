import type { ScriptItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbManagementApiItemDataCache } from '@umbraco-cms/backoffice/management-api';

const scriptItemCache = new UmbManagementApiItemDataCache<ScriptItemResponseModel>();

export { scriptItemCache };
