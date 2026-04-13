import type { ManifestValueSummary } from '../extensions/value-summary.extension.js';
import { customElement, html, nothing, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

const defaultApiLoader = () => import('../api/value-summary-api.base.js');

@customElement('umb-value-summary')
export class UmbValueSummaryElement extends UmbLitElement {
	@property({ attribute: false })
	set valueType(value: string | undefined) {
		if (this.#valueType === value) return;
		this.#valueType = value;
		this.#filter = (m: ManifestValueSummary) => m.forValueType === this.#valueType;
	}
	get valueType() {
		return this.#valueType;
	}

	@property({ attribute: false })
	value?: unknown;

	#valueType?: string;
	#filter: (m: ManifestValueSummary) => boolean = () => false;

	#fallbackRender = () => html`<span>${String(this.value ?? '')}</span>`;

	override render() {
		if (!this.#valueType) return nothing;
		return html`<umb-extension-with-api-slot
			type="valueSummary"
			single
			.filter=${this.#filter}
			.defaultApi=${defaultApiLoader}
			.apiProps=${{ valueType: this.#valueType, rawValue: this.value }}
			.fallbackRenderMethod=${this.#fallbackRender}>
		</umb-extension-with-api-slot>`;
	}
}

export { UmbValueSummaryElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-value-summary': UmbValueSummaryElement;
	}
}
