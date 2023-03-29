import { customElement, property } from 'lit/decorators.js';
import { UmbVariantId } from '../../backoffice/shared/variants/variant-id.class';
import { UmbRouterSlotElement } from './router-slot.element';
import { UmbRoute } from '@umbraco-cms/backoffice/router';

function variantIdsToString(variantIds: UmbVariantId[]): string {
	return variantIds.map((id) => id.toString()).join('_&_');
}

/**
 *  @element umb-variant-router-slot-element
 *  @description - Component for wrapping Router Slot element, providing
 *  @extends UmbRouterSlotElement
 * @fires {UmbRouterSlotInitEvent} init - fires when the router is connected
 * @fires {UmbRouterSlotChangeEvent} change - fires when a path of this router is changed
 */
@customElement('umb-variant-router-slot')
export class UmbVariantRouterSlotElement extends UmbRouterSlotElement {
	#variantIds: UmbVariantId[] = [];

	#getPathPrefix() {
		return variantIdsToString(this.#variantIds);
	}

	#currentPathPrefix = '';
	private _routes?: UmbRoute[];
	public get routes(): UmbRoute[] | undefined {
		return this._routes;
	}
	public set routes(value: UmbRoute[] | undefined) {
		this._routes = value;
		if (this.#variantIds.length > 0) {
			this._updateRoutes();
		}
	}

	private _updateRoutes() {
		const newPrefix = this.#getPathPrefix();
		if (newPrefix !== this.#currentPathPrefix) {
			this.#currentPathPrefix = newPrefix;
			const prepend = newPrefix === '' ? '' : newPrefix + '/';
			const mappedRoutes = this._routes?.map((route) => {
				return {
					...route,
					path: prepend + route.path,
				};
			});
			super.routes = mappedRoutes;
			this._updateRouterPath();
		}
	}

	@property()
	public get variantId(): UmbVariantId[] {
		return this.#variantIds;
	}
	public set variantId(value: UmbVariantId[] | UmbVariantId) {
		if (Array.isArray(value)) {
			this.#variantIds = [...(value as UmbVariantId[])];
		} else if (value) {
			this.#variantIds = [value];
		} else {
			this.#variantIds = [];
		}
		if (this._routes) {
			this._updateRoutes();
		}
	}

	protected _constructAbsoluteRouterPath() {
		return super._constructAbsoluteRouterPath() + (this.#currentPathPrefix !== '' ? '/' + this.#currentPathPrefix : '');
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-variant-router-slot': UmbVariantRouterSlotElement;
	}
}
