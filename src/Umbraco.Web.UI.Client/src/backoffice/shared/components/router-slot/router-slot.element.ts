import { LitElement, PropertyValueMap } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { IRoute, RouterSlot } from 'router-slot';
import { UmbRouterSlotChangeEvent } from './router-slot-change.event';
import { UmbRouterSlotInitEvent } from './router-slot-init.event';

/**
 *  @element umb-router-slot-element
 *  @description - Component for wrapping Router Slot element, providing some local events for implementation.
 *  @extends UmbRouterSlotElement
 */
@customElement('umb-router-slot')
export class UmbRouterSlotElement extends LitElement {
	#router: RouterSlot;
	#listening = false;

	@property()
	public get routes(): IRoute[] | undefined {
		return (this.#router as any).routes;
	}
	public set routes(value: IRoute[] | undefined) {
		(this.#router as any).routes = value;
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

	constructor() {
		super();
		this.#router = document.createElement('router-slot');
	}

	connectedCallback() {
		super.connectedCallback();
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
		this._routerPath = this.#router.constructAbsolutePath('') || '';
		this.dispatchEvent(new UmbRouterSlotInitEvent());
	}

	private _onNavigationChanged = (event?: any) => {
		if (event.detail.slot === this.#router) {
			this._activeLocalPath = event.detail.match.route.path;
			this.dispatchEvent(new UmbRouterSlotChangeEvent());
		}
	};

	render() {
		return this.#router;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-router-slot': UmbRouterSlotElement;
	}
}
