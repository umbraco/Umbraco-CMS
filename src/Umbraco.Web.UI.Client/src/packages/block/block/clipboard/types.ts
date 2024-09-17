import type { UmbClipboardEntry } from '@umbraco-cms/backoffice/clipboard';

/**
 * A Clipboard entry for Blocks.
 */
export type UmbBlockClipboardEntry = UmbClipboardEntry<'block', UmbBlockClipboardEntryMeta, any>;

interface UmbBlockClipboardEntryMeta {
	/**
	 * The aliases of the content-types of these entries.
	 */
	contentTypeAliases: Array<string>;
}
