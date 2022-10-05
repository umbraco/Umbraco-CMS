import { html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';

@customElement('umb-section-users')
export class UmbSectionUsersElement extends LitElement {
	render() {
		return html` <umb-section></umb-section> `;
	}
}

export default UmbSectionUsersElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-section-users': UmbSectionUsersElement;
	}
}
