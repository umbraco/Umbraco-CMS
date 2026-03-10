import { css, customElement, html, ifDefined, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

import type { ManifestCollectionFilter, UmbSelectOption } from '@umbraco-cms/backoffice/collection';
import type { UmbUserGroupCollectionFilterApi } from './user-group-collection-filter.api';

@customElement('umb-user-group-collection-filter')
export class UmbUserGroupCollectionFilterElement extends UmbLitElement {
	public manifest?: ManifestCollectionFilter;

	@state()
	private _options: Array<UmbSelectOption> = [];

	@state()
	private _value: Array<string> = [];

	private _api?: UmbUserGroupCollectionFilterApi | undefined;
	public get api(): UmbUserGroupCollectionFilterApi | undefined {
		return this._api;
	}
	public set api(api: UmbUserGroupCollectionFilterApi | undefined) {
		this._api = api;
		this.observe(api?.options, (options) => (this._options = options ?? []));
		this.observe(api?.value, (value) => (this._value = value ?? []));
	}

	#onChange(event: Event) {
		const target = event.currentTarget as HTMLInputElement;
		const item = this._options.find((option) => option.value === target.value);

		if (!item) return;

		let value;
		if (target.checked) {
			value = [...this._value, item.value];
		} else {
			value = this._value.filter((group) => group !== item.value);
		}

		this._api?.setValue(value);
	}

	#getUserGroupFilterLabel() {
		const length = this._value.length;
		const max = 2;
		//TODO: Temp solution to limit the amount of states shown
		return length === 0
			? this.localize.term('general_all')
			: this._value
					.slice(0, max)
					.map((group) => group)
					.join(', ') + (length > max ? ' + ' + (length - max) : '');
	}

	protected override render() {
		return html`
			<div class="filter">
				<span class="label">${this.manifest?.meta.label}</span>
				<uui-button popovertarget="collection-multi-select-filter-popover" label="Multi Select" compact>
					<b>${this.#getUserGroupFilterLabel()}</b>
				</uui-button>
				<uui-popover-container id="collection-multi-select-filter-popover" placement="bottom">
					<umb-popover-layout>
						<div class="filter-dropdown">
							${repeat(
								this._options,
								(group) => group.value,
								(group) => html`
									<uui-checkbox
										label=${ifDefined(group.label)}
										value=${ifDefined(group.value)}
										@change=${this.#onChange}></uui-checkbox>
								`,
							)}
						</div>
					</umb-popover-layout>
				</uui-popover-container>
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
			.filter-dropdown {
				display: flex;
				gap: var(--uui-size-space-3);
				flex-direction: column;
				padding: var(--uui-size-space-3);
			}
			.label {
				font-weight: 600;
				font-size: var(--uui-size-4);
			}
		`,
	];
}

export { UmbUserGroupCollectionFilterElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-group-collection-filter': UmbUserGroupCollectionFilterElement;
	}
}
