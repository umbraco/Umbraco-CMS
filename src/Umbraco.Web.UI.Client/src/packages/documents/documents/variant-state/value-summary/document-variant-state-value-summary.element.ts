import type { UmbDocumentVariantStateValueModel } from '../value-type/constants.js';
import { getDocumentVariantStateTagConfig } from '../utils.js';
import type { UmbDocumentVariantState } from '../../variant-state.js';
import { customElement, html, nothing, state, type PropertyValues } from '@umbraco-cms/backoffice/external/lit';
import { UmbValueSummaryElementBase } from '@umbraco-cms/backoffice/value-summary';
import { UmbVariantResolver } from '@umbraco-cms/backoffice/variant';

type UmbDocumentVariantStateValue = Array<UmbDocumentVariantStateValueModel>;

@customElement('umb-document-variant-state-value-summary')
export class UmbDocumentVariantStateValueSummaryElement extends UmbValueSummaryElementBase<UmbDocumentVariantStateValue> {
	#variantResolver = new UmbVariantResolver<UmbDocumentVariantStateValueModel>(this);

	@state()
	private _state?: UmbDocumentVariantState | null;

	constructor() {
		super();
		this.observe(this.#variantResolver.variant, (variant) => (this._state = variant?.state));
	}

	override willUpdate(changedProperties: PropertyValues) {
		super.willUpdate(changedProperties);
		if (changedProperties.has('_value')) {
			this.#variantResolver.setVariants(this._value);
		}
	}

	override render() {
		// No tag until a variant matching the current culture has been resolved.
		if (this._state === undefined) return nothing;
		const { color, label } = getDocumentVariantStateTagConfig(this._state, this.localize);
		return html`<uui-tag color=${color} look="secondary">${label}</uui-tag>`;
	}
}

export { UmbDocumentVariantStateValueSummaryElement as element };

declare global {
	interface HTMLElementTagNameMap {
		['umb-document-variant-state-value-summary']: UmbDocumentVariantStateValueSummaryElement;
	}
}
