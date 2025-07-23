import type { UmbContext } from '@umbraco-cms/backoffice/class-api';
import type { Observable } from '@umbraco-cms/backoffice/observable-api';

export interface UmbRoutePathAddendum extends UmbContext {
	addendum: Observable<string | undefined>;
}
