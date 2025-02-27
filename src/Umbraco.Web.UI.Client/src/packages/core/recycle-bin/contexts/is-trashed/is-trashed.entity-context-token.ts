import type { UmbIsTrashedEntityContext } from './is-trashed.entity-context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_IS_TRASHED_ENTITY_CONTEXT = new UmbContextToken<UmbIsTrashedEntityContext>(
	'UmbEntityIsTrashedContext',
);
