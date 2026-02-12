import type { WebhookItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbManagementApiItemDataCache } from '@umbraco-cms/backoffice/management-api';

const webhookItemCache = new UmbManagementApiItemDataCache<WebhookItemResponseModel>();

export { webhookItemCache };
