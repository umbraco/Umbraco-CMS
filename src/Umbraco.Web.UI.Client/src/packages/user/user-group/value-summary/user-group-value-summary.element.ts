import type { UmbUserGroupItemModel } from '../repository/item/types.js';
import { customElement, html, nothing, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-user-groups-value-summary')
export class UmbUserGroupsValueSummaryElement extends UmbLitElement {
	@property({ attribute: false })
	value?: ReadonlyArray<UmbUserGroupItemModel>;

	override render() {
		if (!this.value?.length) return nothing;
		return html`<span>${this.value.map((g) => g.name).join(', ')}</span>`;
	}
}

export { UmbUserGroupsValueSummaryElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-groups-value-summary': UmbUserGroupsValueSummaryElement;
	}
}
