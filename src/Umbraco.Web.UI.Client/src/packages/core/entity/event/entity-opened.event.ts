export class UmbEntityOpenedEvent extends Event {
	public static readonly TYPE = 'entity-opened';

	public readonly unique: string;
	public readonly entityType: string;

	constructor(args: { unique: string; entityType: string }) {
		super(UmbEntityOpenedEvent.TYPE, { bubbles: false, composed: false, cancelable: false });
		this.unique = args.unique;
		this.entityType = args.entityType;
	}
}

declare global {
	interface GlobalEventHandlersEventMap {
		[UmbEntityOpenedEvent.TYPE]: UmbEntityOpenedEvent;
	}
}
