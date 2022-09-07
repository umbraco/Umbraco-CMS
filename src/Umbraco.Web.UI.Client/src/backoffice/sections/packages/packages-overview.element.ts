import { html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';

@customElement('umb-packages-overview')
export class UmbPackagesOverview extends LitElement {
	render() {
		return html`
			<uui-box headline="Popular"></uui-box>
			<uui-box headline="Promoted"></uui-box>
		`;
	}
}

export default UmbPackagesOverview;

declare global {
	interface HTMLElementTagNameMap {
		'umb-packages-overview': UmbPackagesOverview;
	}
}
