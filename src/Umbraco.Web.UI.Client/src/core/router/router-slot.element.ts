import type { IRoute } from 'router-slot/model';
import { RouterSlot } from 'router-slot';
import { css, html, PropertyValueMap } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { UmbLitElement } from '../lit-element';
import { UmbRouterSlotInitEvent } from './router-slot-init.event';
import { UmbRouterSlotChangeEvent } from './router-slot-change.event';
import { UmbRouteContext, UmbRoute } from '@umbraco-cms/backoffice/router';

/**
 *  @element umb-router-slot
 *  @description - Component for wrapping Router Slot element, providing some local events for implementation.
 *  @extends UmbLitElement
 * @fires {UmbRouterSlotInitEvent} init - fires when the router is connected
 * @fires {UmbRouterSlotChangeEvent} change - fires when a path of this router is changed
 */
@customElement('umb-router-slot')
export class UmbRouterSlotElement extends UmbLitElement {
	static styles = [
		css`
			:host {
				display: flex;
				flex-direction: column;
				height: 100%;
			}

			router-slot {
				height: 100%;
			}
		`,
	];

	#router: RouterSlot = new RouterSlot();
	#modalRouter: RouterSlot = new RouterSlot();
	#listening = false;

	@property()
	public get routes(): UmbRoute[] | undefined {
		return (this.#router as any).routes;
	}
	public set routes(value: UmbRoute[] | undefined) {
		this.#router.routes = (value as IRoute[]) || [];
	}

	private _routerPath?: string;
	public get absoluteRouterPath() {
		return this._routerPath;
	}

	private _activeLocalPath?: string;
	public get localActiveViewPath() {
		return this._activeLocalPath;
	}

	public get absoluteActiveViewPath() {
		return this._routerPath + '/' + this._activeLocalPath;
	}

	#routeContext = new UmbRouteContext(this, (contextRoutes) => {
		(this.#modalRouter as any).routes = contextRoutes;
		// Force a render?
		this.#modalRouter.render();
	});

	constructor() {
		super();
		this.#modalRouter.parent = this.#router;
		this.#modalRouter.style.display = 'none';
		this.#router.addEventListener('changestate', this._updateRouterPath.bind(this));
		//this.#router.appendChild(this.#modalRouter);
		this.#router.appendChild(document.createElement('slot'));
	}

	protected _constructAbsoluteRouterPath() {
		return this.#router.constructAbsolutePath('') || '';
	}

	connectedCallback() {
		super.connectedCallback();
		// Currently we have to set this every time as RouteSlot looks for its parent every-time it is connected. Aka it has not way to explicitly set the parent.
		// And we cannot insert the modal router as a slotted-child of the router, as it flushes its children on every route change.
		this.#modalRouter.parent = this.#router;
		if (this.#listening === false) {
			window.addEventListener('navigationsuccess', this._onNavigationChanged);
			this.#listening = true;
		}
	}

	disconnectedCallback() {
		super.disconnectedCallback();
		window.removeEventListener('navigationsuccess', this._onNavigationChanged);
		this.#listening = false;
	}

	protected firstUpdated(_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>): void {
		super.firstUpdated(_changedProperties);
		this._routerPath = this._constructAbsoluteRouterPath();
		this.#routeContext._internal_routerGotBasePath(this._routerPath);
		this.dispatchEvent(new UmbRouterSlotInitEvent());
	}

	protected _updateRouterPath() {
		const newAbsolutePath = this._constructAbsoluteRouterPath();
		if (this._routerPath !== newAbsolutePath) {
			this._routerPath = newAbsolutePath;
			this.#routeContext._internal_routerGotBasePath(this._routerPath);
			this.dispatchEvent(new UmbRouterSlotInitEvent());

			const newActiveLocalPath = this.#router.match?.route.path;
			if (this._activeLocalPath !== newActiveLocalPath) {
				this._activeLocalPath = newActiveLocalPath;
				this.dispatchEvent(new UmbRouterSlotChangeEvent());
			}
		}
	}

	private _onNavigationChanged = (event?: any) => {
		if (event.detail.slot === this.#router) {
			this._activeLocalPath = event.detail.match.route.path;
			this.dispatchEvent(new UmbRouterSlotChangeEvent());
		} else if (event.detail.slot === this.#modalRouter) {
			const newActiveModalLocalPath = event.detail.match.route.path;
			this.#routeContext._internal_modalRouterChanged(newActiveModalLocalPath);
		}
	};

	render() {
		return html`${this.#router}${this.#modalRouter}`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-router-slot': UmbRouterSlotElement;
	}
}
