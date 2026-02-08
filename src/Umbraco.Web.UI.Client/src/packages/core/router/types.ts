import type { IRoutingInfo, PageComponent } from './router-slot/index.js';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
export type * from './conditions/is-routable-context/types.js';

export interface UmbRouteEntry extends UmbApi {
	getPath?(): string;
	setup?(element: PageComponent, info: IRoutingInfo): void;
}
