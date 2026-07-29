/**
 * Dispatched when clipboard entries have been picked, carrying the entry uniques rather than a property value.
 * Translating them is the receiver's job, because a paste translator resolves to the value of a specific
 * property editor — so only the property editor hosting the picker knows which shape to ask for.
 */
export class UmbClipboardEntriesPickedEvent extends Event {
	public static readonly TYPE = 'clipboard-entries-picked';

	/**
	 * The uniques of the picked clipboard entries.
	 */
	public entryUniques: Array<string>;

	public constructor(entryUniques: Array<string>, args?: EventInit) {
		super(UmbClipboardEntriesPickedEvent.TYPE, {
			bubbles: true,
			composed: false,
			cancelable: false,
			...args,
		});
		this.entryUniques = entryUniques;
	}
}
