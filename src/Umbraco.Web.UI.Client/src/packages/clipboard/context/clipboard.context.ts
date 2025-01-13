import { UmbClipboardEntryDetailRepository, type UmbClipboardEntryDetailModel } from '../clipboard-entry/index.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';

/**
 * Clipboard context for managing clipboard entries
 * @export
 * @class UmbClipboardContext
 * @augments {UmbContextBase<UmbClipboardContext>}
 */
export class UmbClipboardContext extends UmbContextBase<UmbClipboardContext> {
	#clipboardDetailRepository = new UmbClipboardEntryDetailRepository(this);

	/**
	 * Write to the clipboard
	 * @param {Partial<UmbClipboardEntryDetailModel>} entryPreset - The preset for the clipboard entry
	 * @returns {Promise<void>}
	 * @memberof UmbClipboardContext
	 */
	async write(entryPreset: Partial<UmbClipboardEntryDetailModel>): Promise<UmbClipboardEntryDetailModel | undefined> {
		if (!entryPreset) throw new Error('Entry preset is required');

		const { data: scaffoldData } = await this.#clipboardDetailRepository.createScaffold(entryPreset);
		if (!scaffoldData) return;

		const { data } = await this.#clipboardDetailRepository.create(scaffoldData);
		return data;
	}

	/**
	 * Read from the clipboard
	 * @param {string} unique - The unique id of the clipboard entry
	 * @returns {Promise<UmbClipboardEntryDetailModel | undefined>} - Returns the clipboard entry
	 * @memberof UmbClipboardContext
	 */
	async read(unique: string): Promise<UmbClipboardEntryDetailModel | undefined> {
		const { data } = await this.#clipboardDetailRepository.requestByUnique(unique);
		return data;
	}
}

export { UmbClipboardContext as api };
