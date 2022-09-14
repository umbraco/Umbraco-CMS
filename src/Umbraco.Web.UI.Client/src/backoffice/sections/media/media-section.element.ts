import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';

@customElement('umb-media-section')
export class UmbMediaSection extends LitElement {
	static styles = [UUITextStyles];

	render() {
		return html`<umb-section></umb-section>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-section': UmbMediaSection;
	}
}
