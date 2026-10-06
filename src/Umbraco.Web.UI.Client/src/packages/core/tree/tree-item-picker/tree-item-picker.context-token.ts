import type { UmbTreeItemPickerContext } from './tree-item-picker.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbPickerContext } from '@umbraco-cms/backoffice/picker';

/**
 * A picker context that browses a tree, and therefore has a location.
 *
 * The alias is the one every picker context is provided under; the discriminator is what narrows it to those that can
 * be browsed. A picker with no location — a flat collection picker, say — deliberately does not match.
 */
export const UMB_TREE_ITEM_PICKER_CONTEXT = new UmbContextToken<UmbPickerContext, UmbTreeItemPickerContext>(
	'UmbPickerContext',
	undefined,
	(context): context is UmbTreeItemPickerContext => (context as UmbTreeItemPickerContext).location !== undefined,
);
