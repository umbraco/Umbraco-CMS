import type { UmbClipboardEntryDetailModel } from '../../clipboard-entry/types.js';

/**
 * What a clipboard entry picker may list for a property, and which of those entries it may actually pick.
 * Held separately because this is the part a picker modal needs, and all a picker modal needs.
 */
export interface UmbClipboardPropertyPasteConfig {
	/**
	 * The clipboard entry value types to list. The property editor owns this, because only it knows which value
	 * types it has a paste translator for.
	 */
	types: Array<string>;

	/**
	 * Decides which of the listed entries are pastable into the property. Entries that are not stay in the list
	 * and render as disabled.
	 */
	pickableFilter?: (entry: UmbClipboardEntryDetailModel) => Promise<boolean>;
}

/**
 * The clipboard affordances a property editor offers the input it renders.
 *
 * A property editor hands this down instead of the input asking for it, because everything here is derived from
 * the property editor's own translators and configuration — and because the input must not touch a clipboard
 * value itself: a copy translator consumes, and a paste translator resolves to, the value of a specific property
 * editor. The input reports what the user did (`clipboard-copy-request`, `clipboard-entries-picked`) and the
 * property editor owns the value on both sides.
 */
export interface UmbClipboardPropertyConfig {
	/**
	 * Whether the property editor can copy — shows the copy affordance on each item of the input.
	 */
	copy: boolean;

	/**
	 * Present when the property editor can paste. Absent means no clipboard is offered when picking.
	 */
	paste?: UmbClipboardPropertyPasteConfig;
}
