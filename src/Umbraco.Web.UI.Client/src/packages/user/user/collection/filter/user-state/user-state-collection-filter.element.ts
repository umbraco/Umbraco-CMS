import { css, customElement, html, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { ManifestCollectionFilter, UmbSelectOption } from '@umbraco-cms/backoffice/collection';
import { type UmbUserStateFilterType } from '../../utils';
import type { UmbUserStateCollectionFilterApi } from './user-state-collection-filter.api';

@customElement('umb-user-state-collection-filter')
export class UmbUserStateCollectionFilterElement extends UmbLitElement {
	public manifest?: ManifestCollectionFilter;

	@state()
	private _options: Array<UmbSelectOption> = [];

	@state()
	private _value: Array<UmbUserStateFilterType> = [];

	private _api?: UmbUserStateCollectionFilterApi | undefined;
	public get api(): UmbUserStateCollectionFilterApi | undefined {
		return this._api;
	}
	public set api(value: UmbUserStateCollectionFilterApi | undefined) {
		this._api = value;
		this.observe(value?.options, (options) => (this._options = options ?? []));
	}

	#onChange(event: Event) {
		const target = event.currentTarget as HTMLInputElement;
		const value = target.value as UmbUserStateFilterType;
		const isChecked = target.checked;

		this._value = isChecked ? [...this._value, value] : this._value.filter((v) => v !== value);

		this._api?.setValue(this._value);
	}

	#getStatusFilterLabel() {
		const length = this._value.length;
		const max = 2;
		//TODO: Temp solution to limit the amount of states shown
		return length === 0
			? this.localize.term('general_all')
			: this._value
					.slice(0, max)
					.map((state) => this.localize.term('user_state' + state))
					.join(', ') + (length > max ? ' + ' + (length - max) : '');
	}

	protected override render() {
		return html`
			<div class="filter">
				<span class="label">${this.manifest?.meta.label}</span>
				<uui-button popovertarget="collection-multi-select-filter-popover" label="Multi Select" compact>
					<b>${this.#getStatusFilterLabel()}</b>
				</uui-button>
				<uui-popover-container id="collection-multi-select-filter-popover" placement="bottom">
					<umb-popover-layout>
						<div class="filter-dropdown">
							${this._options.map(
								(option) =>
									html`<uui-checkbox
										label=${this.localize.term('user_state' + option.value)}
										@change=${this.#onChange}
										name="state"
										value=${option.value}></uui-checkbox>`,
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

export { UmbUserStateCollectionFilterElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-state-collection-filter': UmbUserStateCollectionFilterElement;
	}
}
