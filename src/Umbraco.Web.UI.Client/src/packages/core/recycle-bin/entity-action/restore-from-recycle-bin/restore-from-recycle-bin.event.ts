import { UmbEntityActionEvent, type UmbEntityActionEventArgs } from '@umbraco-cms/backoffice/entity-action';

export class UmbEntityRestoredFromRecycleBinEvent extends UmbEntityActionEvent {
	static readonly TYPE = 'entity-restored-from-recycle-bin';

	constructor(args: UmbEntityActionEventArgs) {
		super(UmbEntityRestoredFromRecycleBinEvent.TYPE, args);
	}
}
