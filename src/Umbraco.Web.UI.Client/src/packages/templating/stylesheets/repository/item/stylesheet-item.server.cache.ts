import type { StylesheetItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbManagementApiItemDataCache } from '@umbraco-cms/backoffice/management-api';

const stylesheetItemCache = new UmbManagementApiItemDataCache<StylesheetItemResponseModel>();

export { stylesheetItemCache };
