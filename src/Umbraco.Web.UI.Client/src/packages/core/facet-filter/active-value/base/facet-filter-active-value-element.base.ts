import type { UmbFacetFilterActiveValueApi } from '../facet-filter-active-value-api.interface.js';
import type { UmbFacetFilterActiveValueElement } from '../facet-filter-active-value-element.interface.js';
import { property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

/**
 * Base class for facet filter active value elements. Observes the resolved value of the API.
 * Compose `<umb-facet-filter-active-value-layout>` in `render()`, set its `label` to the value as displayed text, and
 * call `_remove()` on its `remove` event.
 */
export abstract class UmbFacetFilterActiveValueElementBase<TValue = unknown, TResolvedValue = unknown>
	extends UmbLitElement
	implements UmbFacetFilterActiveValueElement<TValue, TResolvedValue>
{
	#api?: UmbFacetFilterActiveValueApi<TValue, TResolvedValue>;

	@property({ attribute: false })
	set api(api: UmbFacetFilterActiveValueApi<TValue, TResolvedValue> | undefined) {
		this.#api = api;
		this.observe(api?.resolvedValue, (resolvedValue) => (this._resolvedValue = resolvedValue), 'resolvedValue');
	}
	get api() {
		return this.#api;
	}

	@state()
	protected _resolvedValue?: TResolvedValue;

	protected _remove = () => {
		this.#api?.remove();
	};
}
