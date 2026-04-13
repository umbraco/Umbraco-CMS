import type { UmbUserGroupItemModel } from '../repository/item/types.js';
import { customElement, html, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbValueSummaryElementBase } from '@umbraco-cms/backoffice/value-summary';

@customElement('umb-user-groups-value-summary')
export class UmbUserGroupsValueSummaryElement extends UmbValueSummaryElementBase<ReadonlyArray<UmbUserGroupItemModel>> {
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
