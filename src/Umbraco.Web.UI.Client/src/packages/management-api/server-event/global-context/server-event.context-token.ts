import type { UmbClipboardContext } from './clipboard.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbManagementApiServerEventContext } from './server-event.context.js';

export const UMB_MANAGEMENT_API_SERVER_EVENT_CONTEXT = new UmbContextToken<UmbManagementApiServerEventContext>(
	'UmbClipboardContext',
);
