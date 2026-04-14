import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';

/**
 * The kind of a single title segment, indicating what part of the application
 * contributed it. Consumers can filter segments by kind — for example, the user
 * history list excludes `'tab'` segments from its breadcrumb but includes them
 * in the browser's `document.title`.
 */
export type UmbViewTitleKind =
	| 'section'
	| 'workspace-type'
	| 'workspace-ancestor'
	| 'workspace'
	| 'tab'
	| 'modal';

/**
 * A single segment in the active view's title chain, annotated with the kind
 * of view that produced it.
 */
export type UmbCurrentViewTitleSegment = {
	label: string;
	kind: UmbViewTitleKind;
	/**
	 * Optional icon name (e.g. `icon-document-js`) associated with this segment.
	 * Consumers like the user history list may render this alongside the label.
	 */
	icon?: string;
	/**
	 * When true, this segment replaces the immediately preceding segment if it
	 * has the same label. Use this when a workspace root shares its hosting
	 * section's name (e.g. "Content" root under the Content section) to avoid
	 * "Content › Content" in the breadcrumb while keeping the richer segment's
	 * metadata (icon, kind).
	 */
	replaces?: boolean;
};

/**
 * Structured representation of the currently active view's title, published by {@link UmbViewController}.
 * Parallel to `document.title`, but exposed as an observable so consumers do not have to parse the joined string.
 */
export type UmbCurrentViewTitle = {
	/**
	 * `window.location.pathname` at the moment the title was published.
	 * Subscribers can use this to guard against stale emissions during fast navigation.
	 */
	path: string;
	/**
	 * Title segments ordered from topmost ancestor to leaf (current) view.
	 * `segments.map(s => s.label).join(' | ')` reproduces the string form used for `document.title`.
	 */
	segments: ReadonlyArray<UmbCurrentViewTitleSegment>;
};

const state = new UmbObjectState<UmbCurrentViewTitle | undefined>(undefined);

/**
 * Observable of the currently active view's structured title.
 * Emits whenever the active view's title chain changes.
 */
export const umbCurrentViewTitle = state.asObservable();

/**
 * Internal: set by UmbViewController only.
 * @param value The new value for the current view title.
 * @internal
 */
// eslint-disable-next-line @typescript-eslint/naming-convention
export const _setUmbCurrentViewTitle = (value: UmbCurrentViewTitle | undefined): void => {
	state.setValue(value);
};
