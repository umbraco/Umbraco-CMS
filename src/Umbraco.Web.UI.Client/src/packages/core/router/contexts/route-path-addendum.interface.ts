import type { Observable } from '@umbraco-cms/backoffice/observable-api';

export interface UmbRoutePathAddendum {
	addendum: Observable<string | undefined>;
}
