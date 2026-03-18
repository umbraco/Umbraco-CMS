import { customElement, html, nothing, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-user-groups-value-minimal-display')
export class UmbUserGroupsValueMinimalDisplayElement extends UmbLitElement {
	@property({ attribute: false })
	value?: string;

	override render() {
		if (this.value === undefined) return nothing;
		return html`<span>${this.value}</span>`;
	}
}

export { UmbUserGroupsValueMinimalDisplayElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-groups-value-minimal-display': UmbUserGroupsValueMinimalDisplayElement;
	}
}
