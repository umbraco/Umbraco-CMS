/**
 * Dispatched to request that clipboard entries are pasted into the dispatcher's value, carrying the uniques of
 * the entries rather than a value. Translating them is the receiver's job, because a paste translator resolves
 * to the value of a specific property editor — so only the element owning that value can ask for the right shape.
 */
export class UmbClipboardPasteRequestEvent extends Event {
	public static readonly TYPE = 'clipboard-paste-request';

	/**
	 * The uniques of the clipboard entries to paste.
	 */
	public entryUniques: Array<string>;

	public constructor(entryUniques: Array<string>, args?: EventInit) {
		super(UmbClipboardPasteRequestEvent.TYPE, {
			bubbles: true,
			composed: false,
			cancelable: false,
			...args,
		});

		this.entryUniques = entryUniques;
	}
}
