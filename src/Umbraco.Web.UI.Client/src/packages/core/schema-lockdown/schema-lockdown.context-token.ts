import type { UmbSchemaLockdownContext } from './schema-lockdown.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_SCHEMA_LOCKDOWN_CONTEXT = new UmbContextToken<UmbSchemaLockdownContext>('UmbSchemaLockdownContext');
