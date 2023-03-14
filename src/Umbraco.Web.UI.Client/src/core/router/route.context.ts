import { IRoute, IRoutingInfo, PARAM_IDENTIFIER, stripSlash } from 'router-slot';
import { UmbContextConsumerController, UmbContextProviderController, UmbContextToken } from '@umbraco-cms/context-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { UmbModalToken, UMB_MODAL_CONTEXT_TOKEN } from '@umbraco-cms/modal';

const EmptyDiv = document.createElement('div');

// TODO: Consider accepting the Token as a generic:
export type UmbModalRoute<UmbModalTokenData extends object, UmbModalTokenResult> = {
	path: string;
	onSetup: (routingInfo: IRoutingInfo) => UmbModalTokenData | false;
	onSubmit: (data: UmbModalTokenResult) => void | PromiseLike<void>;
	onReject: () => void;
};

export type UmbModalRouteBuilder = (params: { [key: string]: string | number }) => string;

export class UmbRouteContext {
	#host: UmbControllerHostInterface;
	#modalContext?: typeof UMB_MODAL_CONTEXT_TOKEN.TYPE;
	#contextRoutes: IRoute[] = [];

	constructor(host: UmbControllerHostInterface, private _onGotModals: (contextRoutes: any) => void) {
		this.#host = host;
		new UmbContextProviderController(host, UMB_ROUTE_CONTEXT_TOKEN, this);
		/*new UmbContextConsumerController(host, UMB_ROUTE_CONTEXT_TOKEN, (context) => {
			console.log('got a parent', this === context, this, context);
		});*/
		new UmbContextConsumerController(host, UMB_MODAL_CONTEXT_TOKEN, (context) => {
			this.#modalContext = context;
		});

		// Consider using this event, to stay up to date with current full-URL. which is necessary for Modal opening.
		// window.addEventListener('navigationsuccess', this._onNavigationChanged);
	}

	public registerModal<D extends object = object, R = unknown>(
		modalAlias: UmbModalToken<D, R> | string,
		options: UmbModalRoute<D, R>
	): UmbModalRouteBuilder {
		const localPath = `modal/${modalAlias.toString()}/${options.path}`;
		this.#contextRoutes.push({
			path: localPath,
			pathMatch: 'suffix',
			component: EmptyDiv,
			setup: (component, info) => {
				const modalData = options.onSetup(info);
				if (modalData && this.#modalContext) {
					const modalHandler = this.#modalContext.open<D, R>(modalAlias, modalData);
					modalHandler.onSubmit().then(
						() => this._removeModalPath(info),
						() => this._removeModalPath(info)
					);
					modalHandler.onSubmit().then(options.onSubmit, options.onReject);
				}
			},
		});

		//TODO: move to a method:
		this._onGotModals(this.#contextRoutes);

		return (params: { [key: string]: string | number }) => {
			const localRoutePath = stripSlash(
				localPath.replace(PARAM_IDENTIFIER, (substring: string, ...args: string[]) => {
					return params[args[0]];
					//return `([^\/]+)`;
				})
			);
			const baseRoutePath = window.location.href;
			return (baseRoutePath.endsWith('/') ? baseRoutePath : baseRoutePath + '/') + localRoutePath;
		};
	}

	private _removeModalPath(info: IRoutingInfo) {
		window.history.pushState({}, '', window.location.href.split(info.match.fragments.consumed)[0]);
		console.log('ask to remove path', info.match.fragments.consumed);
	}
}

export const UMB_ROUTE_CONTEXT_TOKEN = new UmbContextToken<UmbRouteContext>('UmbRouterContext');
