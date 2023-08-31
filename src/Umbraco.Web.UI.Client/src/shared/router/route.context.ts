import type { UmbRoute } from './route.interface.js';
import { createRoutePathBuilder } from './generate-route-path-builder.function.js';
import type { IRoutingInfo, IRouterSlot } from '@umbraco-cms/backoffice/external/router-slot';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbBaseController, type UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UMB_MODAL_MANAGER_CONTEXT_TOKEN, UmbModalRouteRegistration } from '@umbraco-cms/backoffice/modal';

const EmptyDiv = document.createElement('div');

type UmbRoutePlusModalKey = UmbRoute & { __modalKey: string };

export class UmbRouteContext extends UmbBaseController {
	#mainRouter: IRouterSlot;
	#modalRouter: IRouterSlot;
	#modalRegistrations: UmbModalRouteRegistration[] = [];
	#modalContext?: typeof UMB_MODAL_MANAGER_CONTEXT_TOKEN.TYPE;
	#modalRoutes: UmbRoutePlusModalKey[] = [];
	#routerBasePath?: string;
	#routerActiveLocalPath?: string;
	#activeModalPath?: string;

	constructor(host: UmbControllerHostElement, mainRouter: IRouterSlot, modalRouter: IRouterSlot) {
		super(host);
		this.#mainRouter = mainRouter;
		this.#modalRouter = modalRouter;
		this.provideContext(UMB_ROUTE_CONTEXT_TOKEN, this);
		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT_TOKEN, (context) => {
			this.#modalContext = context;
			this.#generateModalRoutes();
		});
	}

	public registerModal(registration: UmbModalRouteRegistration) {
		this.#modalRegistrations.push(registration);
		this.#createNewUrlBuilder(registration);
		this.#generateModalRoutes();
		return registration;
	}

	public unregisterModal(registrationToken: ReturnType<typeof this.registerModal>) {
		const index = this.#modalRegistrations.indexOf(registrationToken);
		if (index === -1) return;
		this.#modalRegistrations.splice(index, 1);
		this.#generateModalRoutes();
	}

	#generateRoute(modalRegistration: UmbModalRouteRegistration): UmbRoutePlusModalKey {
		return {
			__modalKey: modalRegistration.key,
			path: '/' + modalRegistration.generateModalPath(),
			component: EmptyDiv,
			setup: async (component, info) => {
				if (!this.#modalContext) return;
				const modalContext = await modalRegistration.routeSetup(this.#modalRouter, this.#modalContext, info.match.params);
				if (modalContext) {
					modalContext.onSubmit().then(
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

	#generateModalRoutes() {
		const newModals = this.#modalRegistrations.filter(
			(x) => !this.#modalRoutes.find((route) => x.key === route.__modalKey)
		);
		const routesToRemove = this.#modalRoutes.filter(
			(route) => !this.#modalRegistrations.find((x) => x.key === route.__modalKey)
		);

		const cleanedRoutes = this.#modalRoutes.filter((route) => !routesToRemove.includes(route));

		this.#modalRoutes = [
			...cleanedRoutes,
			...newModals.map((modalRegistration) => {
				return this.#generateRoute(modalRegistration);
			}),
		];

		// Add an empty route, so there is a route for the router to react on when no modals are open.
		this.#modalRoutes.push({
			__modalKey: '_empty_',
			path: '',
			component: EmptyDiv,
		});

		// TODO: Should we await one frame, to ensure we don't call back too much?.
		this.#modalRouter.routes = this.#modalRoutes;
		this.#modalRouter.render();
	}

	public _internal_routerGotBasePath(routerBasePath: string) {
		if (this.#routerBasePath === routerBasePath) return;
		this.#routerBasePath = routerBasePath;
		this.#createNewUrlBuilders();
	}

	public _internal_routerGotActiveLocalPath(routerActiveLocalPath: string | undefined) {
		if (this.#routerActiveLocalPath === routerActiveLocalPath) return;
		this.#routerActiveLocalPath = routerActiveLocalPath;
		this.#createNewUrlBuilders();
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

	#createNewUrlBuilders() {
		this.#modalRegistrations.forEach(this.#createNewUrlBuilder);
	}

	#createNewUrlBuilder = (modalRegistration: UmbModalRouteRegistration) => {
		if (!this.#routerBasePath) return;

		const routeBasePath = this.#routerBasePath.endsWith('/') ? this.#routerBasePath : this.#routerBasePath + '/';
		const routeActiveLocalPath = this.#routerActiveLocalPath
			? this.#routerActiveLocalPath.endsWith('/')
				? this.#routerActiveLocalPath
				: this.#routerActiveLocalPath + '/'
			: '';
		const localPath = routeBasePath + routeActiveLocalPath + modalRegistration.generateModalPath();

		const urlBuilder = createRoutePathBuilder(localPath);

		modalRegistration._internal_setRouteBuilder(urlBuilder);
	};
}

export const UMB_ROUTE_CONTEXT_TOKEN = new UmbContextToken<UmbRouteContext>('UmbRouterContext');
