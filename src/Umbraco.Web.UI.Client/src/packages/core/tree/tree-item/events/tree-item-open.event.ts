export class UmbTreeItemOpenEvent extends Event {
	public static readonly TYPE = 'umb-tree-item-open';

	public readonly unique: string;
	public readonly entityType: string;

	constructor(args: { unique: string; entityType: string }, eventInit?: EventInit) {
		super(UmbTreeItemOpenEvent.TYPE, { bubbles: true, composed: true, ...eventInit });
		this.unique = args.unique;
		this.entityType = args.entityType;
	}
}

declare global {
	interface GlobalEventHandlersEventMap {
		[UmbTreeItemOpenEvent.TYPE]: UmbTreeItemOpenEvent;
	}
}
