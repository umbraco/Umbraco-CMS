import { html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';

@customElement('umb-section-settings')
export class UmbSectionSettingsElement extends LitElement {
	render() {
		return html` <umb-section></umb-section>`;
	}
}

export default UmbSectionSettingsElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-section-settings': UmbSectionSettingsElement;
	}
}
