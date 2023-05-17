// eslint-disable-next-line local-rules/no-external-imports
import type { IRoutingInfo } from 'router-slot/model';
import type { UmbRoute } from './route.interface';
import { generateRoutePathBuilder } from './generate-route-path-builder.function';
import { UmbContextConsumerController, UmbContextProviderController, UmbContextToken } from 'src/libs/context-api';
import type { UmbControllerHostElement } from 'src/libs/controller-api';
import { UMB_MODAL_CONTEXT_TOKEN, UmbModalRouteRegistration } from 'src/libs/modal';

const EmptyDiv = document.createElement('div');

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
			setup: (component, info) => {
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
		const localPath = routeBasePath + `modal/${modalRegistration.alias.toString()}/${modalRegistration.path}`;

		const urlBuilder = generateRoutePathBuilder(localPath);

		modalRegistration._internal_setRouteBuilder(urlBuilder);
	};
}

export const UMB_ROUTE_CONTEXT_TOKEN = new UmbContextToken<UmbRouteContext>('UmbRouterContext');
