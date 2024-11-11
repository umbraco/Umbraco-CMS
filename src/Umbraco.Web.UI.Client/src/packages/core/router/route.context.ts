import type { UmbRoute } from './route.interface.js';
import { umbGenerateRoutePathBuilder } from './generate-route-path-builder.function.js';
import type { UmbModalRouteRegistration } from './modal-registration/modal-route-registration.interface.js';
import type { IRoutingInfo, IRouterSlot } from '@umbraco-cms/backoffice/external/router-slot';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';

const EmptyDiv = document.createElement('div');

type UmbRoutePlusModalKey = UmbRoute & { __modalKey: string };

export class UmbRouteContext extends UmbContextBase<UmbRouteContext> {
	#modalRouter: IRouterSlot;
	#modalRegistrations: UmbModalRouteRegistration[] = [];
	#modalContext?: typeof UMB_MODAL_MANAGER_CONTEXT.TYPE;
	#modalRoutes: UmbRoutePlusModalKey[] = [];
	#routerBasePath?: string;
	#routerActiveLocalPath?: string;
	#activeModalPath?: string;

	constructor(host: UmbControllerHost, mainRouter: IRouterSlot, modalRouter: IRouterSlot) {
		super(host, UMB_ROUTE_CONTEXT);
		this.#modalRouter = modalRouter;
		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, (context) => {
			this.#modalContext = context;
			this.#generateModalRoutes();
		});
	}

	public registerModal(registration: UmbModalRouteRegistration) {
		this.#modalRegistrations.push(registration);
		this.#createNewUrlBuilder(registration);
		this.#generateModalRoutes();
	}

	public unregisterModal(registrationToken: UmbModalRouteRegistration) {
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
				const modalContext = await modalRegistration.routeSetup(
					this.#modalRouter,
					this.#modalContext,
					info.match.params,
				);
				if (modalContext) {
					modalContext.onSubmit().then(
						() => {
							this.#removeModalPath(info);
						},
						() => {
							this.#removeModalPath(info);
						},
					);
				}
			},
		};
	}

	#removeModalPath(info: IRoutingInfo) {
		// Reset the URL to the routerBasePath + routerActiveLocalPath [NL]
		const folderToRemove = info.match.fragments.consumed;
		if (folderToRemove && window.location.href.includes(folderToRemove)) {
			window.history.pushState({}, '', this.#routerBasePath + '/' + this.#routerActiveLocalPath);
		}
	}

	#generateModalRoutes() {
		const newModals = this.#modalRegistrations.filter(
			(x) => !this.#modalRoutes.find((route) => x.key === route.__modalKey),
		);
		const routesToRemove = this.#modalRoutes.filter(
			(route) => !this.#modalRegistrations.find((x) => x.key === route.__modalKey),
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
		const urlBuilder = umbGenerateRoutePathBuilder(localPath);

		modalRegistration._internal_setRouteBuilder(urlBuilder);
	};

	override hostDisconnected(): void {
		super.hostDisconnected();
		this._internal_modalRouterChanged(undefined);
	}
}

export const UMB_ROUTE_CONTEXT = new UmbContextToken<UmbRouteContext>('UmbRouterContext');
