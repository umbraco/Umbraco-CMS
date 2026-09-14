import type { UmbMemberGroupItemModel } from '../repository/item/types.js';
import { customElement, html, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbValueSummaryElementBase } from '@umbraco-cms/backoffice/value-summary';

@customElement('umb-member-groups-value-summary')
export class UmbMemberGroupsValueSummaryElement extends UmbValueSummaryElementBase<Array<UmbMemberGroupItemModel>> {
	override render() {
		if (!Array.isArray(this._value) || !this._value.length) return nothing;
		const text = this._value.map((item) => item.name).join(', ');
		return html`<span title="${text}">${text}</span>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-member-groups-value-summary': UmbMemberGroupsValueSummaryElement;
	}
}
