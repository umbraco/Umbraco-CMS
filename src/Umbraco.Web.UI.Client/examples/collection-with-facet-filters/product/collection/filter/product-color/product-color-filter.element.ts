import type { ExampleProductColorFilterApi, ExampleProductColorFacetOptionModel } from './product-color-filter.api.js';
import { css, customElement, html, nothing, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('example-product-color-filter')
export class ExampleProductColorFilterElement extends UmbLitElement {
	@state()
	private _options: Array<ExampleProductColorFacetOptionModel> = [];

	@state()
	private _value: Array<string> = [];

	#api?: ExampleProductColorFilterApi;
	public get api(): ExampleProductColorFilterApi | undefined {
		return this.#api;
	}
	public set api(api: ExampleProductColorFilterApi | undefined) {
		this.#api = api;
		if (!api) return;

		this.observe(api.options, (options) => (this._options = options));
		this.observe(api.value, (value) => (this._value = value));
	}

	#onSelected(unique: string) {
		if (this._value.includes(unique)) return;
		this.#api?.setValue([...this._value, unique]);
	}

	#onDeselected(unique: string) {
		this.#api?.setValue(this._value.filter((value) => value !== unique));
	}

	protected override render() {
		if (this._options.length === 0) return nothing;

		return html`
			<div id="options">
				${repeat(
					this._options,
					(option) => option.unique,
					(option) => {
						return html`<div class="option" @click=${(event: MouseEvent) => this.#onOptionClick(event, option.unique)}>
							<uui-color-swatch
								label="${option.name} (${option.count})"
								.value=${option.unique}
								.color=${option.hex}
								selectable
								deselectable
								.selected=${this._value.includes(option.unique)}
								@selected=${() => this.#onSelected(option.unique)}
								@deselected=${() => this.#onDeselected(option.unique)}></uui-color-swatch>
							<span aria-hidden="true">${option.name} (${option.count})</span>
						</div>`;
					},
				)}
			</div>
		`;
	}

	#onOptionClick(event: MouseEvent, unique: string) {
		// The swatch handles clicks on itself; clicks on the name or count toggle the same value.
		const swatch = (event.currentTarget as HTMLElement).querySelector('uui-color-swatch');
		if (swatch && event.composedPath().includes(swatch)) return;

		if (this._value.includes(unique)) {
			this.#onDeselected(unique);
		} else {
			this.#onSelected(unique);
		}
	}

	static override styles = [
		css`
			#options {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-2);
			}

			.option {
				display: flex;
				align-items: center;
				gap: var(--uui-size-space-3);
				cursor: pointer;
			}

			uui-color-swatch {
				--uui-swatch-size: 20px;
			}
		`,
	];
}

export { ExampleProductColorFilterElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'example-product-color-filter': ExampleProductColorFilterElement;
	}
}
