import type { UmbSelectOption } from '@umbraco-cms/backoffice/collection';
import { css, customElement, html, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbExtensionCollectionFilterApi } from './extension-collection-filter.api';

@customElement('umb-extension-collection-filter')
export class UmbExtensionCollectionFilterElement extends UmbLitElement {
	@state()
	private _options: Array<UmbSelectOption> = [];

	@state()
	private _value: Array<string> = [];

	private _api?: UmbExtensionCollectionFilterApi | undefined;
	public get api(): UmbExtensionCollectionFilterApi | undefined {
		return this._api;
	}
	public set api(api: UmbExtensionCollectionFilterApi | undefined) {
		this._api = api;
		this.observe(api?.options, (options) => (this._options = options ?? []));
		this.observe(api?.value, (value) => (this._value = value ?? []));
	}

	#onSelect(event: Event) {
		const target = event.target as HTMLInputElement;
		this._value = [target.value];
		this._api?.setValue([target.value]);
	}

	protected override render() {
		return html`
			<div class="filter">
				<span class="label">Filter:</span>
				<uui-combobox value=${this._value[0] ?? ''} @change=${this.#onSelect} placeholder="Placeholder">
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

export { UmbExtensionCollectionFilterElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-extension-collection-filter': UmbExtensionCollectionFilterElement;
	}
}
