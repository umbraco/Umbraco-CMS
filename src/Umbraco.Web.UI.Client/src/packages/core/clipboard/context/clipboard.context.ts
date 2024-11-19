import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_CLIPBOARD_CONTEXT } from './clipboard.context-token';
import type { UmbClipboardEntry } from '../types';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import { UmbClipboardDetailRepository } from '../detail/index.js';

/**
 * Clipboard context for managing clipboard entries
 * @export
 * @class UmbClipboardContext
 * @extends {UmbContextBase<UmbClipboardContext>}
 */
export class UmbClipboardContext extends UmbContextBase<UmbClipboardContext> {
	#entries = new UmbArrayState<UmbClipboardEntry>([], (x) => x.unique);
	#init?: Promise<unknown>;

	/**
	 * Observable that emits all entries in the clipboard
	 * @memberof UmbClipboardContext
	 */
	public readonly entries = this.#entries.asObservable();

	/**
	 * Observable that emits true if there are any entries in the clipboard
	 * @memberof UmbClipboardContext
	 */
	public hasEntries = this.#entries.asObservablePart((x) => x.length > 0);

	#modalManagerContext?: typeof UMB_MODAL_MANAGER_CONTEXT.TYPE;
	#clipboardDetailRepository = new UmbClipboardDetailRepository(this);

	constructor(host: UmbControllerHost) {
		super(host, UMB_CLIPBOARD_CONTEXT);

		this.#init = Promise.all([
			this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, (context) => {
				this.#modalManagerContext = context;
			}).asPromise(),
		]);
	}

	/**
	 * Get all entries from the clipboard
	 * @returns {Array<UmbClipboardEntry>} An array of all entries in the clipboard
	 * @memberof UmbClipboardContext
	 */
	getEntries(): Array<UmbClipboardEntry> {
		return this.#entries.getValue();
	}

	/**
	 * Set entries in the clipboard
	 * @param {Array<UmbClipboardEntry>} entries An array of entries to set in the clipboard
	 * @memberof UmbClipboard
	 **/
	setEntries(entries: UmbClipboardEntry[]): void {
		if (!entries) throw new Error('Entries are required');
		this.#entries.setValue(entries);
	}

	/**
	 * Create an entry in the clipboard
	 * @param {UmbClipboardEntry} entry A clipboard entry to insert into the clipboard
	 * @memberof UmbClipboardContext
	 */
	async create(entry: UmbClipboardEntry): Promise<void> {
		if (!entry) throw new Error('Entry is required');
		if (!entry.unique) throw new Error('Entry must have a unique property');
		this.setEntries([...this.#entries.getValue(), entry]);
	}

	/**
	 * Read an entry from the clipboard
	 * @param {UmbClipboardEntry} entry A clipboard entry to insert into the clipboard
	 * @memberof UmbClipboardContext
	 */
	async read(unique: string): Promise<UmbClipboardEntry> {
		const entry = this.#entries.getValue().find((x) => x.unique === unique);
		if (!entry) throw new Error(`Entry with unique ${unique} not found`);
		return entry;
	}

	/**
	 * Update an entry in the clipboard
	 * @param {UmbClipboardEntry} entry A clipboard entry to update in the clipboard
	 * @memberof UmbClipboardContext
	 */
	async update(entry: UmbClipboardEntry): Promise<void> {
		if (!entry) throw new Error('Entry is required');
		if (!entry.unique) throw new Error('Entry must have a unique property');
		this.#entries.updateOne(entry.unique, entry);
	}

	/**
	 * Delete an entry from the clipboard
	 * @param {string} unique A unique identifier of the entry to remove from the clipboard
	 * @memberof UmbClipboardContext
	 */
	async delete(unique: string): Promise<void> {
		const entry = this.read(unique);
		if (!entry) return;
		this.#entries.setValue(this.#entries.getValue().filter((x) => x.unique !== unique));
	}

	/**
	 * Remove all entries from the clipboard
	 * @memberof UmbClipboardContext
	 */
	async clear(): Promise<void> {
		this.#entries.setValue([]);
	}

	/**
	 * Open the clipboard
	 * @memberof UmbClipboardContext
	 */
	async open() {
		await this.#init;
		alert('Open clipboard');
	}

	observeEntriesOf(types: Array<string>, filter?: (entry: UmbClipboardEntry) => boolean) {
		throw new Error('Method not implemented.');
	}

	observeHasEntriesOf(types: Array<string>, filter?: (entry: UmbClipboardEntry) => boolean) {
		// TODO: Implement filter
		return this.#entries.asObservablePart((entries) => entries.some((entry) => types.includes(entry.type)));
	}
}

export { UmbClipboardContext as api };
