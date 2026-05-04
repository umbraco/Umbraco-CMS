import type { UmbValueSummaryApi } from '../extensions/value-summary-api.interface.js';
import type { UmbValueSummaryElement } from '../extensions/value-summary-element.interface.js';
import { property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

export abstract class UmbValueSummaryElementBase<ValueType = unknown>
	extends UmbLitElement
	implements UmbValueSummaryElement<ValueType>
{
	@property({ attribute: false })
	set api(api: UmbValueSummaryApi<ValueType> | undefined) {
		this.#api = api;
		if (api) {
			this.observe(api.value, (v) => (this._value = v), 'value');
		}
	}
	get api() {
		return this.#api;
	}

	#api?: UmbValueSummaryApi<ValueType>;

	@state()
	protected _value?: ValueType;
}
