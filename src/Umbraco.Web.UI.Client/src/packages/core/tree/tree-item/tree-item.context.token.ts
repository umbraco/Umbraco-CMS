import type { UmbTreeItemApi } from './tree-item-base/tree-item-api-base.js';
import type { UmbTreeItemContext } from './tree-item-context.interface.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

/**
 * Base token for any tree item provider — matches both classic tree item contexts
 * and card api providers. Use this in entity action conditions that should work
 * regardless of which tree view is active.
 
 * Ideally this would be named `UMB_TREE_ITEM_CONTEXT`, but that name was already
 * taken by the full tree item context token (which includes children, expansion, and pagination).
 */
export const UMB_TREE_ITEM_API_CONTEXT = new UmbContextToken<UmbTreeItemApi>('UmbTreeItemContext');

/**
 * Full tree item context token. Only matches providers that implement children,
 * expansion, and pagination. Use this when you specifically need child loading
 * or expansion state.
 */
export const UMB_TREE_ITEM_CONTEXT = new UmbContextToken<UmbTreeItemApi, UmbTreeItemContext>(
	'UmbTreeItemContext',
	undefined,
	(context): context is UmbTreeItemContext => 'loadChildren' in context,
);
