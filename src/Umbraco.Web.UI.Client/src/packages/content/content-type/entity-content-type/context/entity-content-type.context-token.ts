import type { UmbEntityContentTypeEntityContext } from './entity-content-type.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_ENTITY_CONTENT_TYPE_ENTITY_CONTEXT = new UmbContextToken<UmbEntityContentTypeEntityContext>(
	'UmbEntityContext',
	'UmbEntityContentType',
);
