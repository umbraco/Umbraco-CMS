import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';

@customElement('umb-content-section')
export class UmbContentSection extends LitElement {
	static styles = [UUITextStyles];

	render() {
		return html`<umb-section></umb-section>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-content-section': UmbContentSection;
	}
}
