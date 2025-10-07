import type { IRouterSlot, Params } from '../router-slot/index.js';
import type { UmbModalRouteBuilder } from './modal-route-registration.controller.js';
import type {
	UmbModalContext,
	UmbModalManagerContext,
	UmbModalToken,
} from '@umbraco-cms/backoffice/modal';
import type { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export type UmbModalRouteSetupArgs = {
	router: IRouterSlot;
	modalManagerContext: UmbModalManagerContext;
	params: Params;
	routeContextToken: UmbContextToken<any>;
};

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
		args: UmbModalRouteSetupArgs,
	): Promise<undefined | UmbModalContext<UmbModalTokenData, UmbModalTokenValue>>;

	// eslint-disable-next-line @typescript-eslint/naming-convention
	_internal_setRouteBuilder(builder: UmbModalRouteBuilder): void;
}
