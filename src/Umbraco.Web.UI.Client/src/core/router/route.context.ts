import { IRoute } from 'router-slot';
import { UmbContextConsumerController, UmbContextProviderController, UmbContextToken } from '@umbraco-cms/context-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { UmbModalToken } from '@umbraco-cms/modal';

// Get the second generic type of UmbModalToken:
type GetResultType<T> = T extends UmbModalToken<infer Data, infer Result> ? Result : unknown;

const EmptyDiv = document.createElement('div');

export type UmbModalRoute<UmbModalTokenResult> = {
	path: string;
	onSetup: (routeInfo: any) => void;
	onSubmit: (data: UmbModalTokenResult) => void;
	onReject: () => void;
};

export class UmbRouteContext {
	#host: UmbControllerHostInterface;
	#contextRoutes: IRoute[] = [];

	constructor(host: UmbControllerHostInterface, private _onGotModals: (contextRoutes: any) => void) {
		this.#host = host;
		new UmbContextProviderController(host, UMB_ROUTE_CONTEXT_TOKEN, this);
		/*new UmbContextConsumerController(host, UMB_ROUTE_CONTEXT_TOKEN, (context) => {
			console.log('got a parent', this === context, this, context);
		});*/

		// Consider using this event, to stay up to date with current full-URL. which is necessary for Modal opening.
		// window.addEventListener('navigationsuccess', this._onNavigationChanged);
	}

	public registerModal<T extends UmbModalToken = UmbModalToken, R = GetResultType<T>>(
		modalAlias: T,
		options: UmbModalRoute<R>
	) {
		console.log('registerModalRoute', modalAlias.toString(), options);

		this.#contextRoutes.push({
			path: options.path,
			component: EmptyDiv,
			setup: (component, info) => {
				console.log('modal open?', info);
				options.onSetup(info);
			},
		});

		//TODO: move to a method:
		this._onGotModals(this.#contextRoutes);
	}
}

export const UMB_ROUTE_CONTEXT_TOKEN = new UmbContextToken<UmbRouteContext>('UmbRouterContext');
