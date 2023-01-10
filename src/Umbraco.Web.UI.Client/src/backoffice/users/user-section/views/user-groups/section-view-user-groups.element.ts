import { html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';

import './workspace-view-user-groups.element';

@customElement('umb-section-view-user-groups')
export class UmbSectionViewUserGroupsElement extends LitElement {
	render() {
		return html`<umb-workspace-view-user-groups></umb-workspace-view-user-groups>`;
	}
}

export default UmbSectionViewUserGroupsElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-section-view-user-groups': UmbSectionViewUserGroupsElement;
	}
}
