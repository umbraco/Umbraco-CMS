import { html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';

@customElement('umb-section-members')
export class UmbSectionMembersElement extends LitElement {
	render() {
		return html`<umb-section></umb-section>`;
	}
}

export default UmbSectionMembersElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-section-members': UmbSectionMembersElement;
	}
}
