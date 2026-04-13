import type { UmbUserGroupItemModel } from '../repository/item/types.js';
import type { UmbValueSummaryApi } from '@umbraco-cms/backoffice/value-summary';
import type { UmbValueSummaryElementInterface } from '@umbraco-cms/backoffice/value-summary';
import { customElement, html, nothing, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-user-groups-value-summary')
export class UmbUserGroupsValueSummaryElement extends UmbLitElement implements UmbValueSummaryElementInterface {
	@property({ attribute: false })
	set api(api: UmbValueSummaryApi | undefined) {
		this.#api = api;
		if (api) {
			this.observe(api.value, (v) => (this._value = v as ReadonlyArray<UmbUserGroupItemModel>), 'value');
		}
	}
	get api() {
		return this.#api;
	}

	#api?: UmbValueSummaryApi;

	@state()
	private _value?: ReadonlyArray<UmbUserGroupItemModel>;

	override render() {
		if (!this._value?.length) return nothing;
		return html`<span>${this._value.map((g) => g.name).join(', ')}</span>`;
	}
}

export { UmbUserGroupsValueSummaryElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-groups-value-summary': UmbUserGroupsValueSummaryElement;
	}
}
