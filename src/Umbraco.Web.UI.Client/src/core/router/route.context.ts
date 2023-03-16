import { IRoute, IRoutingInfo, Params, PARAM_IDENTIFIER, stripSlash } from 'router-slot';
import { UmbContextConsumerController, UmbContextProviderController, UmbContextToken } from '@umbraco-cms/context-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { UmbModalConfig, UmbModalToken, UMB_MODAL_CONTEXT_TOKEN } from '@umbraco-cms/modal';

const EmptyDiv = document.createElement('div');

export type UmbModalRouteOptions<UmbModalTokenData extends object = object, UmbModalTokenResult = unknown> = {
	path: string;
	config?: UmbModalConfig;
	onSetup?: (routingInfo: Params) => UmbModalTokenData | false;
	onSubmit?: (data: UmbModalTokenResult) => void | PromiseLike<void>;
	onReject?: () => void;
	getUrlBuilder?: (urlBuilder: UmbModalRouteBuilder) => void;
};

export type UmbModalRegistrationToken = UmbModalRouteRegistration;

type UmbModalRouteRegistration<D extends object = object, R = any> = {
	alias: UmbModalToken | string;
	options: UmbModalRouteOptions<D, R>;
	routeSetup: (component: HTMLElement, info: IRoutingInfo) => void;
};

export type UmbModalRouteBuilder = (params: { [key: string]: string | number }) => string;

export class UmbRouteContext {
	//#host: UmbControllerHostInterface;
	#modalRegistrations: UmbModalRouteRegistration[] = [];
	#modalContext?: typeof UMB_MODAL_CONTEXT_TOKEN.TYPE;
	#contextRoutes: IRoute[] = [];
	#routerBasePath?: string;

	constructor(host: UmbControllerHostInterface, private _onGotModals: (contextRoutes: any) => void) {
		//this.#host = host;
		new UmbContextProviderController(host, UMB_ROUTE_CONTEXT_TOKEN, this);
		/*new UmbContextConsumerController(host, UMB_ROUTE_CONTEXT_TOKEN, (context) => {
			console.log('got a parent', this === context, this, context);
			// Why did i want a parent route? was it to capture and inherit routes?
			// This is maybe not so necessary as it stands right now, so lets see how it goes.
		});*/
		new UmbContextConsumerController(host, UMB_MODAL_CONTEXT_TOKEN, (context) => {
			this.#modalContext = context;
			this.#generateContextRoutes();
		});
	}

	#removeModalPath(info: IRoutingInfo) {
		window.history.pushState({}, '', window.location.href.split(info.match.fragments.consumed)[0]);
	}

	public registerModal<D extends object = object, R = any>(
		alias: UmbModalToken<D, R> | string,
		options: UmbModalRouteOptions<D, R>
	) {
		const registration = {
			alias: alias,
			options: options,
			routeSetup: (component: HTMLElement, info: IRoutingInfo<any, any>) => {
				const modalData = options.onSetup?.(info.match.params);
				if (modalData !== false && this.#modalContext) {
					const modalHandler = this.#modalContext.open(alias, modalData, options.config);
					modalHandler.onSubmit().then(
						() => this.#removeModalPath(info),
						() => this.#removeModalPath(info)
					);
					modalHandler.onSubmit().then(options.onSubmit, options.onReject);
				}
			},
		};
		this.#modalRegistrations.push(registration);
		this.#generateNewUrlBuilder(registration);
		this.#generateContextRoutes();
		return registration;
	}

	public unregisterModal(registrationToken: ReturnType<typeof this.registerModal>) {
		const index = this.#modalRegistrations.indexOf(registrationToken);
		if (index === -1) return;
		this.#modalRegistrations.splice(index, 1);
		this.#generateContextRoutes();
	}

	#generateRoute(modalRegistration: UmbModalRouteRegistration): IRoute {
		const localPath = `modal/${modalRegistration.alias.toString()}/${modalRegistration.options.path}`;
		return {
			path: localPath,
			component: EmptyDiv,
			setup: modalRegistration.routeSetup,
		};
	}

	#generateContextRoutes() {
		this.#contextRoutes = this.#modalRegistrations.map((modalRegistration) => {
			return this.#generateRoute(modalRegistration);
		});

		// TODO: Should we await one frame, to ensure we don't call back too much?.
		this._onGotModals(this.#contextRoutes);
	}

	public _internal_routerGotBasePath(routerBasePath: string) {
		if (this.#routerBasePath === routerBasePath) return;
		this.#routerBasePath = routerBasePath;
		this.#generateNewUrlBuilders();
	}

	#generateNewUrlBuilders() {
		this.#modalRegistrations.forEach(this.#generateNewUrlBuilder);
	}

	#generateNewUrlBuilder = (modalRegistration: UmbModalRouteRegistration) => {
		if (!modalRegistration.options.getUrlBuilder || !this.#routerBasePath) return;

		const routeBasePath = this.#routerBasePath.endsWith('/') ? this.#routerBasePath : this.#routerBasePath + '/';
		const localPath = `modal/${modalRegistration.alias.toString()}/${modalRegistration.options.path}`;

		const urlBuilder = (params: { [key: string]: string | number }) => {
			const localRoutePath = stripSlash(
				localPath.replace(PARAM_IDENTIFIER, (substring: string, ...args: string[]) => {
					return params[args[0]].toString();
				})
			);
			return routeBasePath + localRoutePath;
		};

		modalRegistration.options.getUrlBuilder(urlBuilder);
	};
}

export const UMB_ROUTE_CONTEXT_TOKEN = new UmbContextToken<UmbRouteContext>('UmbRouterContext');
