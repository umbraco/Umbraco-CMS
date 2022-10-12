import { html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';

import './editor-view-user-groups.element';

@customElement('umb-section-view-user-groups')
export class UmbSectionViewUserGroupsElement extends LitElement {
	render() {
		return html`<umb-editor-view-user-groups></umb-editor-view-user-groups>`;
	}
}

export default UmbSectionViewUserGroupsElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-section-view-user-groups': UmbSectionViewUserGroupsElement;
	}
}
