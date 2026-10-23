import type { RelationTypeItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbManagementApiItemDataCache } from '@umbraco-cms/backoffice/management-api';

const relationTypeItemCache = new UmbManagementApiItemDataCache<RelationTypeItemResponseModel>();

export { relationTypeItemCache };
