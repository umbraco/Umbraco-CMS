import type { UmbSchemaLockdownContext } from './schema-lockdown.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

/**
 * Context token for the global {@link UmbSchemaLockdownContext}, which holds the schema lockdown restrictions.
 */
export const UMB_SCHEMA_LOCKDOWN_CONTEXT = new UmbContextToken<UmbSchemaLockdownContext>('UmbSchemaLockdownContext');
