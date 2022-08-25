import { html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';

@customElement('umb-section-members')
export class UmbSectionMembers extends LitElement {
	render() {
		return html`
			<umb-section-layout>
				<umb-section-sidebar>RENDER TREE </umb-section-sidebar>
				<umb-section-main>Some main content here</umb-section-main>
			</umb-section-layout>
		`;
	}
}

export default UmbSectionMembers;

declare global {
	interface HTMLElementTagNameMap {
		'umb-section-members': UmbSectionMembers;
	}
}
