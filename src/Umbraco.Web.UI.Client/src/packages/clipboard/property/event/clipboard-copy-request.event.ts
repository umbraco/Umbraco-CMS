/**
 * Dispatched to request that one item of a property value is copied to the clipboard, carrying the identity of
 * the item rather than a value. Producing the value is the receiver's job, because a copy translator consumes
 * the value of a specific property editor — so only the property editor hosting the dispatcher owns the shape.
 *
 * The name and icon travel with it because they are presentation the dispatcher has already resolved for
 * rendering, and the property value alone does not carry them.
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
