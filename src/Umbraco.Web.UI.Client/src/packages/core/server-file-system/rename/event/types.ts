import type { UmbEntityActionEventArgs } from '@umbraco-cms/backoffice/entity-action';

export interface UmbServerFileRenamedEventArgs extends UmbEntityActionEventArgs {
	newUnique: string;
	newName: string;
}
