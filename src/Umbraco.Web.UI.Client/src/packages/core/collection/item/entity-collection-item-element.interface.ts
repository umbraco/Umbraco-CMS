import type { UmbCollectionItemModel } from './types.js';

/**
 * An interface for elements that render collection items representing entities.
 */
export interface UmbEntityCollectionItemElement extends HTMLElement {
	/** The collection item model to render. */
	item?: UmbCollectionItemModel | undefined;

	/** Whether the item should render with selection affordances. */
	selectable?: boolean;

	/** When true, the item only supports selection (no navigation). */
	selectOnly?: boolean;

	/** Whether the item is currently selected. */
	selected?: boolean;

	/** Whether the item is disabled. */
	disabled?: boolean;

	/** Optional href used by card/ref renderers to provide a link. */
	href?: string | undefined;
}
