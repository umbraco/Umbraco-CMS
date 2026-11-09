import type { ExampleColorFacetFilterApi, ExampleColorFacetOptionModel } from './color-facet-filter.api.js';
import { exampleColors } from './colors.js';
import { css, customElement, html, nothing, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('example-color-facet-filter')
export class ExampleColorFacetFilterElement extends UmbLitElement {
	@state()
	private _options: Array<ExampleColorFacetOptionModel> = [];

	@state()
	private _value: Array<string> = [];

	#api?: ExampleColorFacetFilterApi;
	public get api(): ExampleColorFacetFilterApi | undefined {
		return this.#api;
	}
	public set api(api: ExampleColorFacetFilterApi | undefined) {
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
						const name = exampleColors[option.unique]?.name ?? option.unique;
						return html`<div class="option" @click=${(event: MouseEvent) => this.#onOptionClick(event, option.unique)}>
							<uui-color-swatch
								label="${name} (${option.count})"
								.value=${option.unique}
								.color=${exampleColors[option.unique]?.hex ?? option.unique}
								selectable
								deselectable
								.selected=${this._value.includes(option.unique)}
								@selected=${() => this.#onSelected(option.unique)}
								@deselected=${() => this.#onDeselected(option.unique)}></uui-color-swatch>
							<span aria-hidden="true">${name} (${option.count})</span>
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

export { ExampleColorFacetFilterElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'example-color-facet-filter': ExampleColorFacetFilterElement;
	}
}
