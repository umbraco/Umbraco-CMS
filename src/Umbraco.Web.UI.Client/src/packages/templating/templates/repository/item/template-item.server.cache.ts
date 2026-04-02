import type { TemplateItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbManagementApiItemDataCache } from '@umbraco-cms/backoffice/management-api';

const templateItemCache = new UmbManagementApiItemDataCache<TemplateItemResponseModel>();

export { templateItemCache };
