import type { UmbModalRouteBuilder } from './modal-route-registration.controller.js';
import type { IRouterSlot, Params } from '@umbraco-cms/backoffice/external/router-slot';
import type { UmbModalContext, UmbModalManagerContext, UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbModalRouteRegistration<
	UmbModalTokenData extends { [key: string]: any } = { [key: string]: any },
	UmbModalTokenValue = any,
> {
	key: string;
	alias: UmbModalToken<UmbModalTokenData, UmbModalTokenValue> | string;

	generateModalPath(): string;

	path: string | undefined;
	open(params: { [key: string]: string | number }, prepend?: string): void;

	routeSetup(
		router: IRouterSlot,
		modalManagerContext: UmbModalManagerContext,
		params: Params,
	): Promise<undefined | UmbModalContext<UmbModalTokenData, UmbModalTokenValue>>;

	_internal_setRouteBuilder(builder: UmbModalRouteBuilder): void;
}
