import type { UmbFilterActiveValueApi } from '../filter-active-value-api.interface.js';
import type { UmbFilterActiveValueElement } from '../filter-active-value-element.interface.js';
import { property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

/**
 * Base class for filter active value elements. Observes the resolved value of the API.
 * Compose `<umb-filter-active-value-layout>` in `render()`, set its `label` to the value as displayed text, and
 * call `_remove()` on its `remove` event.
 */
export abstract class UmbFilterActiveValueElementBase<TValue = unknown, TResolvedValue = unknown>
	extends UmbLitElement
	implements UmbFilterActiveValueElement<TValue, TResolvedValue>
{
	#api?: UmbFilterActiveValueApi<TValue, TResolvedValue>;

	@property({ attribute: false })
	set api(api: UmbFilterActiveValueApi<TValue, TResolvedValue> | undefined) {
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
