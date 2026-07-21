import type { UmbTreeItemApi } from '../tree-item-api/tree-item-api.interface.js';
import type { UmbTreeItemContext } from './tree-item-context.interface.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

/**
 * The base tree item context — the shared contract every tree item provider implements,
 * covering item data, selection, active state, path, and entity actions. Matches any
 * provider, whether a full tree item context or a card api. Use this in entity action
 * conditions that should work regardless of which tree view is active.
 *
 * For child loading or expansion state, use {@link UMB_TREE_ITEM_CONTEXT} instead.
 */
export const UMB_TREE_ITEM_BASE_CONTEXT = new UmbContextToken<UmbTreeItemApi>('UmbTreeItemContext');

/**
 * The full tree item context, extending the base with child loading, expansion, and
 * pagination. Only matches providers that implement those — use it when you specifically
 * need them; otherwise prefer {@link UMB_TREE_ITEM_BASE_CONTEXT}.
 */
export const UMB_TREE_ITEM_CONTEXT = new UmbContextToken<UmbTreeItemApi, UmbTreeItemContext>(
	'UmbTreeItemContext',
	undefined,
	(context): context is UmbTreeItemContext => 'loadChildren' in context,
);
