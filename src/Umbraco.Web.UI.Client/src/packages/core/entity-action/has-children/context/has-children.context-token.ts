import type { UmbHasChildrenEntityContext } from './has-children.entity-context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_HAS_CHILDREN_ENTITY_CONTEXT = new UmbContextToken<UmbHasChildrenEntityContext>(
	'UmbHasChildrenEntityContext',
);
