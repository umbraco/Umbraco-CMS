import type { UmbServerContext } from './server.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_SERVER_CONTEXT = new UmbContextToken<UmbServerContext>('UmbServerContext');
