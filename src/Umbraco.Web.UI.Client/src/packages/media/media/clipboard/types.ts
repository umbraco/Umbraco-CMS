import type { UmbCropModel, UmbFocalPointModel } from '../property-editors/types.js';
import type { UmbClipboardEntryDetailModel } from '@umbraco-cms/backoffice/clipboard';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';

/**
 * A single media reference with crop/focal-point fidelity, as stored on a `richMedia` clipboard entry.
 */
export interface UmbRichMediaClipboardEntryItemModel extends UmbReferenceByUnique {
	focalPoint: UmbFocalPointModel | null;
	crops: Array<UmbCropModel>;
}

/**
 * The value stored on a `richMedia` clipboard entry: media references with crops and focal point.
 */
export type UmbRichMediaClipboardEntryValueModel = Array<UmbRichMediaClipboardEntryItemModel>;

/**
 * The value stored on a `media` clipboard entry: bare media references, without crops or focal point.
 */
export type UmbMediaClipboardEntryValueModel = Array<UmbReferenceByUnique>;

/**
 * Whether a media input offers copying its items to the clipboard.
 */
export interface UmbMediaClipboardCopyConfig {
	/**
	 * Shows the copy affordance on each item of the input.
	 */
	enabled: boolean;
}

/**
 * What a clipboard entry picker lists for a media input, and which of those entries it may pick. Held separately
 * because it is the part a picker modal needs, and all it needs.
 */
export interface UmbMediaClipboardPasteConfig {
	/**
	 * Shows the clipboard tab when picking.
	 */
	enabled: boolean;

	/**
	 * The clipboard entry value types to list, filtered by the clipboard collection itself. The property editor
	 * owns this, because only it knows which value types it has a paste translator for. Empty when it has none.
	 */
	types: Array<string>;

	/**
	 * Decides which of the listed entries are pastable. Entries that are not stay in the list and render as
	 * disabled.
	 */
	pickableFilter?: (entry: UmbClipboardEntryDetailModel) => Promise<boolean>;
}

/**
 * The clipboard affordances a property editor offers the media input it renders. Handed down rather than asked
 * for, because it is derived from the property editor's own translators and configuration.
 */
export interface UmbMediaClipboardConfig {
	copy: UmbMediaClipboardCopyConfig;
	paste: UmbMediaClipboardPasteConfig;
}
