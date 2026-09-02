import type { UmbMemberGroupItemModel } from '../repository/item/types.js';
import { customElement, html, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbValueSummaryElementBase } from '@umbraco-cms/backoffice/value-summary';

@customElement('umb-member-group-uniques-value-summary')
export class UmbMemberGroupUniquesValueSummaryElement extends UmbValueSummaryElementBase<
	Array<UmbMemberGroupItemModel>
> {
	override render() {
		if (!this._value?.length) return nothing;
		const text = this._value.map((item) => item.name).join(', ');
		return html`<span title="${text}">${text}</span>`;
	}
}

export { UmbMemberGroupUniquesValueSummaryElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-member-group-uniques-value-summary': UmbMemberGroupUniquesValueSummaryElement;
	}
}
