import type { IRoutingInfo, ISlashOptions } from 'router-slot';
import { UmbRoute } from './route.interface';
import {
	UmbContextConsumerController,
	UmbContextProviderController,
	UmbContextToken,
} from '@umbraco-cms/backoffice/context-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { UMB_MODAL_CONTEXT_TOKEN, UmbModalRouteRegistration } from '@umbraco-cms/backoffice/modal';

const EmptyDiv = document.createElement('div');

const PARAM_IDENTIFIER = /:([^\\/]+)/g;

function stripSlash(path: string): string {
	return slashify(path, { start: false, end: false });
}

function slashify(path: string, { start = true, end = true }: Partial<ISlashOptions> = {}): string {
	path = start && !path.startsWith('/') ? `/${path}` : !start && path.startsWith('/') ? path.slice(1) : path;
	return end && !path.endsWith('/') ? `${path}/` : !end && path.endsWith('/') ? path.slice(0, path.length - 1) : path;
}

export class UmbRouteContext {
	#modalRegistrations: UmbModalRouteRegistration[] = [];
	#modalContext?: typeof UMB_MODAL_CONTEXT_TOKEN.TYPE;
	#contextRoutes: UmbRoute[] = [];
	#routerBasePath?: string;
	#activeModalPath?: string;

	constructor(host: UmbControllerHostElement, private _onGotModals: (contextRoutes: any) => void) {
		new UmbContextProviderController(host, UMB_ROUTE_CONTEXT_TOKEN, this);
		new UmbContextConsumerController(host, UMB_MODAL_CONTEXT_TOKEN, (context) => {
			this.#modalContext = context;
			this.#generateContextRoutes();
		});
	}

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

	#generateRoute(modalRegistration: UmbModalRouteRegistration): UmbRoute {
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

	#removeModalPath(info: IRoutingInfo) {
		if (window.location.href.includes(info.match.fragments.consumed)) {
			window.history.pushState({}, '', window.location.href.split(info.match.fragments.consumed)[0]);
		}
	}

	#generateContextRoutes() {
		this.#contextRoutes = this.#modalRegistrations.map((modalRegistration) => {
			return this.#generateRoute(modalRegistration);
		});

		// Add an empty route, so there is a route for the router to react on when no modals are open.
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
		if (!this.#routerBasePath) return;

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
