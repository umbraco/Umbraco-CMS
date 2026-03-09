import { css, customElement, html, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-default-collection-multi-select-filter')
export class UmbDefaultCollectionMultiSelectFilterElement extends UmbLitElement {
	@state()
	private _options = [
		{ label: 'Option A', value: 'optionA' },
		{ label: 'Option B', value: 'optionB' },
		{ label: 'Option C', value: 'optionC' },
	];

	@state()
	private _selection: Array<string> = [];

	#onChange(event: Event) {
		const target = event.currentTarget as HTMLInputElement;
		const value = target.value;
		const isChecked = target.checked;

		this._selection = isChecked ? [...this._selection, value] : this._selection.filter((v) => v !== value);
	}

	protected override render() {
		return html`
			<uui-button popovertarget="collection-multi-select-filter-popover" label="Multi Select" compact>
				${this._selection.length ? this._selection.join(', ') : 'Multi Select'}
			</uui-button>
			<uui-popover-container id="collection-multi-select-filter-popover" placement="bottom">
				<umb-popover-layout>
					<div class="filter-dropdown">
						${this._options.map(
							(option) => html`
								<uui-checkbox label=${option.label} value=${option.value} @change=${this.#onChange}></uui-checkbox>
							`,
						)}
					</div>
				</umb-popover-layout>
			</uui-popover-container>
		`;
	}

	static override styles = [
		css`
			.filter-dropdown {
				display: flex;
				gap: var(--uui-size-space-3);
				flex-direction: column;
				padding: var(--uui-size-space-3);
			}
		`,
	];
}

export { UmbDefaultCollectionMultiSelectFilterElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-default-collection-multi-select-filter': UmbDefaultCollectionMultiSelectFilterElement;
	}
}
