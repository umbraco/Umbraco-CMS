/**
 * Dispatched to request that one item is copied to the clipboard, carrying the identity of the item rather than
 * a value: a copy translator consumes the value of a specific property editor, so only the element owning that
 * value can produce it.
 *
 * The name and icon travel along because the value alone does not carry them.
 */
export class UmbClipboardCopyRequestEvent extends Event {
	public static readonly TYPE = 'clipboard-copy-request';

	/**
	 * The unique of the item to copy, as it is identified within the property value.
	 */
	public unique: string;

	/**
	 * The label of the item, appended to the name of the clipboard entry.
	 */
	public name?: string;

	/**
	 * The icon of the item, used as the icon of the clipboard entry.
	 */
	public icon?: string;

	public constructor(item: { unique: string; name?: string; icon?: string }, args?: EventInit) {
		super(UmbClipboardCopyRequestEvent.TYPE, {
			bubbles: true,
			composed: false,
			cancelable: false,
			...args,
		});

		this.unique = item.unique;
		this.name = item.name;
		this.icon = item.icon;
	}
}
