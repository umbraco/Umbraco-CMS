import type { IRouterSlot } from '../router-slot/index.js';
import { UmbRoutePathAddendumResetContext } from '../contexts/route-path-addendum-reset.context.js';
import { UmbRouterSlotInitEvent } from './router-slot-init.event.js';
import { UmbRouterSlotChangeEvent } from './router-slot-change.event.js';
import type { UmbRoute } from './route.interface.js';
import { UmbRouteContext } from './route.context.js';
import { css, html, type PropertyValueMap, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

/**
 *  @element umb-router-slot
 *  @description - Component for wrapping Router Slot element, providing some local events for implementation.
 *  @augments UmbLitElement
 * @fires {UmbRouterSlotInitEvent} init - fires when the router is connected
 * @fires {UmbRouterSlotChangeEvent} change - fires when a path of this router is changed
 */
@customElement('umb-router-slot')
export class UmbRouterSlotElement extends UmbLitElement {
	#router: IRouterSlot = document.createElement('router-slot') as IRouterSlot;
	#modalRouter: IRouterSlot = document.createElement('router-slot') as IRouterSlot;
	#listening = false;

	@property({ type: Boolean, attribute: 'inherit-addendum', reflect: false })
	public inheritAddendum?: boolean;

	@property({ attribute: false })
	public get routes(): UmbRoute[] | undefined {
		return this.#router.routes;
	}
	public set routes(value: UmbRoute[] | undefined) {
		value ??= [];
		const oldValue = this.#router.routes;
		if (
			value.length !== oldValue?.length ||
			value.filter((route) => oldValue?.findIndex((r) => r.path === route.path) === -1).length > 0
		) {
			this.#router.routes = value;
		}
	}

	@property({ attribute: false })
	public get parent(): IRouterSlot | null | undefined {
		return this.#router.parent;
	}
	public set parent(parent: IRouterSlot | null | undefined) {
		this.#router.parent = parent;
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

	#routeContext = new UmbRouteContext(this, this.#router, this.#modalRouter);

	constructor() {
		super();

		this.#modalRouter.parent = this.#router;
		this.#modalRouter.style.display = 'none';
		this.#router.addEventListener('changestate', this._updateRouterPath.bind(this));
		this.#router.appendChild(document.createElement('slot'));
	}

	protected _constructAbsoluteRouterPath() {
		return this.#router.constructAbsolutePath('') || '';
	}

	protected _constructLocalRouterPath() {
		return this.#router.match?.fragments.consumed ?? '';
	}

	override connectedCallback() {
		if (this.inheritAddendum !== true) {
			new UmbRoutePathAddendumResetContext(this);
		}

		super.connectedCallback();
		// Currently we have to set this every time as RouteSlot looks for its parent every-time it is connected. Aka it has not way to explicitly set the parent.
		// And we cannot insert the modal router as a slotted-child of the router, as it flushes its children on every route change.
		this.#modalRouter.parent = this.#router;
		if (this.#listening === false) {
			window.addEventListener('navigationsuccess', this._onNavigationChanged);
			this.#listening = true;
		}
	}

	override disconnectedCallback() {
		super.disconnectedCallback();
		window.removeEventListener('navigationsuccess', this._onNavigationChanged);
		this.#listening = false;
	}

	protected override firstUpdated(_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>): void {
		super.firstUpdated(_changedProperties);
		this._updateRouterPath();
	}

	protected _updateRouterPath() {
		const newAbsolutePath = this._constructAbsoluteRouterPath();
		if (this._routerPath !== newAbsolutePath) {
			this._routerPath = newAbsolutePath;
			this.#routeContext._internal_routerGotBasePath(this._routerPath);
			this.dispatchEvent(new UmbRouterSlotInitEvent());

			const newActiveLocalPath = this._constructLocalRouterPath();
			if (this._activeLocalPath !== newActiveLocalPath) {
				this._activeLocalPath = newActiveLocalPath;
				this.#routeContext._internal_routerGotActiveLocalPath(this._activeLocalPath);
				this.dispatchEvent(new UmbRouterSlotChangeEvent());
			}
		}
	}

	private _onNavigationChanged = (event?: any) => {
		if (event.detail.slot === this.#router) {
			const newActiveLocalPath = this._constructLocalRouterPath();
			if (this._activeLocalPath !== newActiveLocalPath) {
				this._activeLocalPath = newActiveLocalPath;
				this.#routeContext._internal_routerGotActiveLocalPath(newActiveLocalPath);
				this.dispatchEvent(new UmbRouterSlotChangeEvent());
			}
		} else if (event.detail.slot === this.#modalRouter) {
			const newActiveModalLocalPath = this.#modalRouter.match?.route.path ?? '';
			this.#routeContext._internal_modalRouterChanged(newActiveModalLocalPath);
		}
	};

	override render() {
		return html`${this.#router}${this.#modalRouter}`;
	}

	static override styles = [
		css`
			:host {
				position: relative;
				height: 100%;
			}

			router-slot {
				height: 100%;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-router-slot': UmbRouterSlotElement;
	}
}
