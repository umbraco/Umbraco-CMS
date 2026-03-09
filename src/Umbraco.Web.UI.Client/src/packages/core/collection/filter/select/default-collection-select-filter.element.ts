import type { UmbCollectionFilterApi } from '../collection-filter-api.interface.js';
import { css, customElement, html, state } from '@umbraco-cms/backoffice/external/lit';
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

	#onSelect(value: string) {
		this._selected = value;
		this.#api?.setSelection([value]);
	}

	protected override render() {
		return html`
			<uui-button popovertarget="collection-select-filter-popover" label="Select" compact>
				${this._selected ?? 'Select'}
			</uui-button>
			<uui-popover-container id="collection-select-filter-popover" placement="bottom">
				<umb-popover-layout>
					<div class="filter-dropdown">
						${this._options.map(
							(option) => html`
								<uui-menu-item
									label=${option.label}
									@click-label=${() => this.#onSelect(option.value)}
									?active=${this._selected === option.value}></uui-menu-item>
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

export { UmbDefaultCollectionSelectFilterElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-default-collection-select-filter': UmbDefaultCollectionSelectFilterElement;
	}
}
