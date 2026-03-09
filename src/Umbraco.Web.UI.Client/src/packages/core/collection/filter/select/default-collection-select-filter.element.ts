import type { UmbCollectionFilterApi } from '../collection-filter-api.interface.js';
import { css, customElement, html, repeat, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { ManifestCollectionFilter } from '../collection-filter.extension.js';

@customElement('umb-default-collection-select-filter')
export class UmbDefaultCollectionSelectFilterElement extends UmbLitElement {
	#api?: UmbCollectionFilterApi;

	public get api(): UmbCollectionFilterApi | undefined {
		return this.#api;
	}
	public set api(value: UmbCollectionFilterApi | undefined) {
		this.#api = value;
		this.observe(
			this.#api?.selection,
			(selection) => {
				this._selected = selection?.[0];
			},
			'umbApiSelectionObserver',
		);
	}

	@property({ attribute: false })
	public manifest?: ManifestCollectionFilter;

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
		this.#api?.setSelection([target.value]);
	}

	protected override render() {
		return html`
			<div class="filter">
				<span class="label">${this.manifest?.meta?.label ?? 'Filter'}:</span>
				<uui-combobox value=${this._selected ?? ''} @change=${this.#onSelect} placeholder="Placeholder">
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
