import type { UmbCollectionFilterApi } from '../collection-filter-api.interface.js';
import { css, customElement, html, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-default-collection-select-filter')
export class UmbDefaultCollectionSelectFilterElement extends UmbLitElement {
	#api?: UmbCollectionFilterApi;

	@state()
	private _options = [
		{ label: 'Option A', value: 'optionA' },
		{ label: 'Option B', value: 'optionB' },
		{ label: 'Option C', value: 'optionC' },
	];

	@state()
	private _selected?: string;

	#onSelect(event: Event) {
		const target = event.target as HTMLInputElement;
		this._selected = target.value;
	}

	protected override render() {
		return html`
			<div class="filter">
				<span class="label">Filter:</span>
				<uui-combobox @change=${this.#onSelect} placeholder="Placeholder">
					<uui-combobox-list>
						${repeat(
							this._options,
							(option) => option.value,
							(option) => html`
								<uui-combobox-list-option value=${option.value}> ${option.label} </uui-combobox-list-option>
							`,
						)}
					</uui-combobox-list>
				</uui-combobox>
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
			.filter {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-2);
			}
			.label {
				font-weight: 600;
				font-size: var(--uui-size-4);
			}
		`,
	];
}

export { UmbDefaultCollectionSelectFilterElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-default-collection-select-filter': UmbDefaultCollectionSelectFilterElement;
	}
}
