import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { IRoutingInfo, PageComponent } from './router-slot/index.js';

export interface UmbRouteEntry extends UmbApi {
	getPath?(): string;
	setup?(element: PageComponent, info: IRoutingInfo): void;
}
