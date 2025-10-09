import type { UmbManagementApiServerEventContext } from './server-event.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_MANAGEMENT_API_SERVER_EVENT_CONTEXT = new UmbContextToken<UmbManagementApiServerEventContext>(
	'UmbManagementApiServerEventContext',
);
