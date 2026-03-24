import type { ExampleRangeFacetFilterApi } from './range-facet-filter.api.js';
import { css, customElement, html, state, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('example-range-facet-filter')
export class ExampleRangeFacetFilterElement extends UmbLitElement {
	@state()
	private _min = 0;

	@state()
	private _max = 0;

	@state()
	private _currentMin = 0;

	@state()
	private _currentMax = 0;

	#api?: ExampleRangeFacetFilterApi;
	public get api(): ExampleRangeFacetFilterApi | undefined {
		return this.#api;
	}
	public set api(api: ExampleRangeFacetFilterApi | undefined) {
		this.#api = api;
		if (!api) return;

		this.observe(api.min, (min) => (this._min = min));
		this.observe(api.max, (max) => (this._max = max));
		this.observe(api.currentMin, (min) => (this._currentMin = min));
		this.observe(api.currentMax, (max) => (this._currentMax = max));
	}

	#onChange(event: Event) {
		const target = event.target as HTMLInputElement;
		const [low, high] = target.value.split(',').map((v) => parseInt(v, 10));
		if (!isNaN(low) && !isNaN(high)) {
			this.#api?.setValue(low, high);
		}
	}

	protected override render() {
		if (this._max === 0) return nothing;

		return html`
			<div class="range-filter">
				<label>$${this._currentMin} – $${this._currentMax}</label>
				<uui-range-slider
					label="Price range"
					.min=${this._min}
					.max=${this._max}
					.step=${1}
					.value="${this._currentMin},${this._currentMax}"
					@change=${this.#onChange}>
				</uui-range-slider>
			</div>
		`;
	}

	static override styles = [
		css`
			:host {
				display: block;
				width: 100%;
				border-top: 1px solid var(--uui-color-border);
				padding-top: var(--uui-size-space-5);
			}
			.range-filter {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-1);
			}
			label {
				font-size: var(--uui-size-4);
			}
		`,
	];
}

export { ExampleRangeFacetFilterElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'example-range-facet-filter': ExampleRangeFacetFilterElement;
	}
}
