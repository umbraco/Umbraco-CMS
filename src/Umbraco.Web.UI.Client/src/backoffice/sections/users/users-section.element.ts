import { html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';

import '../../editors/shared/editor-entity-layout/editor-entity-layout.element';

@customElement('umb-section-users')
export class UmbSectionUsersElement extends LitElement {
	render() {
		return html` <umb-editor-entity-layout alias="Umb.Editor.Users"></umb-editor-entity-layout> `;
	}
}

export default UmbSectionUsersElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-section-users': UmbSectionUsersElement;
	}
}
