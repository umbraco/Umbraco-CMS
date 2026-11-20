import type { Observable } from '@umbraco-cms/backoffice/observable-api';
import type { UmbTreeItemContext } from '@umbraco-cms/backoffice/tree';

export interface UmbContentTreeItemContext extends UmbTreeItemContext {
	typeUnique: Observable<string | undefined>;
}
