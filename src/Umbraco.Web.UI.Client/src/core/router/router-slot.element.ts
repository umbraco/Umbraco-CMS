import type { IRoute } from 'router-slot/model';
import { RouterSlot } from 'router-slot';
import { LitElement, PropertyValueMap } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { UmbRouterSlotInitEvent } from './router-slot-init.event';
import { UmbRouterSlotChangeEvent } from './router-slot-change.event';

/**
 *  @element umb-router-slot-element
 *  @description - Component for wrapping Router Slot element, providing some local events for implementation.
 *  @extends UmbRouterSlotElement
 * @fires {UmbRouterSlotInitEvent} init - fires when the router is connected
 * @fires {UmbRouterSlotChangeEvent} change - fires when a path of this router is changed
 */
@customElement('umb-router-slot')
export class UmbRouterSlotElement extends LitElement {
	#router: RouterSlot = new RouterSlot();
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
		this.#router.addEventListener('changestate', this._onChangeState);
		this.#router.appendChild(document.createElement('slot'));
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

	private _onChangeState = () => {
		const newAbsolutePath = this.#router.constructAbsolutePath('') || '';
		if (this._routerPath !== newAbsolutePath) {
			this._routerPath = newAbsolutePath;
			this.dispatchEvent(new UmbRouterSlotInitEvent());

			const newActiveLocalPath = this.#router.match?.route.path;
			if (this._activeLocalPath !== newActiveLocalPath) {
				this._activeLocalPath = newActiveLocalPath;
				this.dispatchEvent(new UmbRouterSlotChangeEvent());
			}
		}
	};

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
