// eslint-disable-next-line local-rules/no-external-imports
import type { IRoutingInfo, IRouterSlot } from 'router-slot/model';
import type { UmbRoute } from './route.interface';
import { generateRoutePathBuilder } from './generate-route-path-builder.function';
import {
	UmbContextConsumerController,
	UmbContextProviderController,
	UmbContextToken,
} from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UMB_MODAL_CONTEXT_TOKEN, UmbModalRouteRegistration } from '@umbraco-cms/backoffice/modal';

const EmptyDiv = document.createElement('div');

export class UmbRouteContext {
	#mainRouter: IRouterSlot;
	#modalRouter: IRouterSlot;
	#modalRegistrations: UmbModalRouteRegistration[] = [];
	#modalContext?: typeof UMB_MODAL_CONTEXT_TOKEN.TYPE;
	#contextRoutes: UmbRoute[] = [];
	#routerBasePath?: string;
	#routerActiveLocalPath?: string;
	#activeModalPath?: string;

	constructor(host: UmbControllerHostElement, mainRouter: IRouterSlot, modalRouter: IRouterSlot) {
		this.#mainRouter = mainRouter;
		this.#modalRouter = modalRouter;
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

	#generateRoute(modalRegistration: UmbModalRouteRegistration): UmbRoute {
		return {
			path: '/' + modalRegistration.generateModalPath(),
			component: EmptyDiv,
			setup: (component, info) => {
				if (!this.#modalContext) return;
				const modalHandler = modalRegistration.routeSetup(this.#modalRouter, this.#modalContext, info.match.params);
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
		this.#modalRouter.routes = this.#contextRoutes;
		this.#modalRouter.render();
	}

	public _internal_routerGotBasePath(routerBasePath: string) {
		if (this.#routerBasePath === routerBasePath) return;
		this.#routerBasePath = routerBasePath;
		this.#generateNewUrlBuilders();
	}

	public _internal_routerGotActiveLocalPath(routerActiveLocalPath: string | undefined) {
		if (this.#routerActiveLocalPath === routerActiveLocalPath) return;
		this.#routerActiveLocalPath = routerActiveLocalPath;
		this.#generateNewUrlBuilders();
	}

	// Also notice each registration should now hold its handler when its active.
	public _internal_modalRouterChanged(activeModalPath: string | undefined) {
		if (this.#activeModalPath === activeModalPath) return;
		if (this.#activeModalPath) {
			// If if there is a modal using the old path.
			const activeModal = this.#modalRegistrations.find((registration) => {
				return '/' + registration.generateModalPath() === this.#activeModalPath;
			});
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
		const routeActiveLocalPath = this.#routerActiveLocalPath
			? this.#routerActiveLocalPath.endsWith('/')
				? this.#routerActiveLocalPath
				: this.#routerActiveLocalPath + '/'
			: '';
		const localPath = routeBasePath + routeActiveLocalPath + modalRegistration.generateModalPath();

		const urlBuilder = generateRoutePathBuilder(localPath);

		modalRegistration._internal_setRouteBuilder(urlBuilder);
	};
}

export const UMB_ROUTE_CONTEXT_TOKEN = new UmbContextToken<UmbRouteContext>('UmbRouterContext');
