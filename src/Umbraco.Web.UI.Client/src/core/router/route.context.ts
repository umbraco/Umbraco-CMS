import { IRoute, IRoutingInfo, PARAM_IDENTIFIER, stripSlash } from 'router-slot';
import {
	UmbContextConsumerController,
	UmbContextProviderController,
	UmbContextToken,
} from '@umbraco-cms/backoffice/context-api';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';
import { UmbControllerHostInterface } from 'libs/controller/controller-host.mixin';
import { UMB_MODAL_CONTEXT_TOKEN } from 'libs/modal/modal.context';
import { UmbModalRouteOptions, UmbModalRouteRegistration } from 'libs/modal/modal-route-registration';

const EmptyDiv = document.createElement('div');

export class UmbRouteContext {
	//#host: UmbControllerHostInterface;
	#modalRegistrations: UmbModalRouteRegistration[] = [];
	#modalContext?: typeof UMB_MODAL_CONTEXT_TOKEN.TYPE;
	#contextRoutes: IRoute[] = [];
	#routerBasePath?: string;
	#activeModalPath?: string;

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
		if (window.location.href.includes(info.match.fragments.consumed)) {
			window.history.pushState({}, '', window.location.href.split(info.match.fragments.consumed)[0]);
		}
	}

	/*
	public registerModal<D extends object = object, R = any>(
		alias: UmbModalToken<D, R> | string,
		options: UmbModalRouteOptions<D, R>
	) {
		const registration = new UmbModalRouteRegistration(alias, options);

		this.#modalRegistrations.push(registration);
		this.#generateNewUrlBuilder(registration);
		this.#generateContextRoutes();
		return registration;
	}
	*/

	public registerModal(registration: UmbModalRouteRegistration) {
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

	#getModalRoutePath(modalRegistration: UmbModalRouteRegistration) {
		return `/modal/${modalRegistration.alias.toString()}/${modalRegistration.path}`;
	}

	#generateRoute(modalRegistration: UmbModalRouteRegistration): IRoute {
		return {
			path: this.#getModalRoutePath(modalRegistration),
			component: EmptyDiv,
			setup: (component: HTMLElement, info: IRoutingInfo<any, any>) => {
				if (!this.#modalContext) return;
				const modalHandler = modalRegistration.routeSetup(this.#modalContext, info.match.params);
				if (modalHandler) {
					modalHandler.onSubmit().then(
						() => {
							this.#removeModalPath(info);
						},
						() => {
							this.#removeModalPath(info);
						}
					);
				}
			},
		};
	}

	#generateContextRoutes() {
		this.#contextRoutes = this.#modalRegistrations.map((modalRegistration) => {
			return this.#generateRoute(modalRegistration);
		});

		// Add an empty route, so that we can always have a route to go to for closing the modals.
		// TODO: Check if this is nesecary with the _internal_modalRouterChanged present.
		this.#contextRoutes.push({
			path: '',
			component: EmptyDiv,
		});

		// TODO: Should we await one frame, to ensure we don't call back too much?.
		this._onGotModals(this.#contextRoutes);
	}

	public _internal_routerGotBasePath(routerBasePath: string) {
		if (this.#routerBasePath === routerBasePath) return;
		this.#routerBasePath = routerBasePath;
		this.#generateNewUrlBuilders();
	}
	// TODO: what is going on here, We need to make sure if the modals are still relevant then not close them.
	// Also notice each registration should now hold its handler when its active.
	public _internal_modalRouterChanged(activeModalPath: string | undefined) {
		if (this.#activeModalPath === activeModalPath) return;
		if (this.#activeModalPath) {
			// If if there is a modal using the old path.
			const activeModal = this.#modalRegistrations.find(
				(registration) => this.#getModalRoutePath(registration) === this.#activeModalPath
			);
			if (activeModal) {
				this.#modalContext?.close(activeModal.key);
			}
		}
		this.#activeModalPath = activeModalPath;
	}

	#generateNewUrlBuilders() {
		this.#modalRegistrations.forEach(this.#generateNewUrlBuilder);
	}

	#generateNewUrlBuilder = (modalRegistration: UmbModalRouteRegistration) => {
		if (!modalRegistration.usesRouteBuilder() || !this.#routerBasePath) return;

		const routeBasePath = this.#routerBasePath.endsWith('/') ? this.#routerBasePath : this.#routerBasePath + '/';
		const localPath = `modal/${modalRegistration.alias.toString()}/${modalRegistration.path}`;

		const urlBuilder = (params: { [key: string]: string | number }) => {
			const localRoutePath = stripSlash(
				localPath.replace(PARAM_IDENTIFIER, (substring: string, ...args: string[]) => {
					return params[args[0]].toString();
				})
			);
			return routeBasePath + localRoutePath;
		};

		modalRegistration._internal_setRouteBuilder(urlBuilder);
	};
}

export const UMB_ROUTE_CONTEXT_TOKEN = new UmbContextToken<UmbRouteContext>('UmbRouterContext');
