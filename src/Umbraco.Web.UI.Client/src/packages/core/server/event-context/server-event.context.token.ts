import type { UmbServerEventContext } from './server-event.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_SERVER_EVENT_CONTEXT = new UmbContextToken<UmbServerEventContext>('UmbServerEventContext');
