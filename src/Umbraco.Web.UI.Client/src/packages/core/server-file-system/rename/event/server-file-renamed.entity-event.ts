import type { UmbServerFileRenamedEventArgs } from './types.js';
import { UmbEntityActionEvent } from '@umbraco-cms/backoffice/entity-action';

export class UmbServerFileRenamedEntityEvent extends UmbEntityActionEvent<UmbServerFileRenamedEventArgs> {
	static readonly TYPE = 'server-file-renamed';

	constructor(args: UmbServerFileRenamedEventArgs) {
		super(UmbServerFileRenamedEntityEvent.TYPE, args);
	}

	getNewUnique(): string {
		return this._args.newUnique;
	}

	getNewName(): string {
		return this._args.newName;
	}
}
