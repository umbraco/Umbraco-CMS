import { html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';

@customElement('umb-section-view-user-groups')
export class UmbSectionViewUserGroupsElement extends LitElement {
	render() {
		return html`<div>CONTENT FOR USER GROUPS SECTION VIEW</div>`;
	}
}

export default UmbSectionViewUserGroupsElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-section-view-user-groups': UmbSectionViewUserGroupsElement;
	}
}
