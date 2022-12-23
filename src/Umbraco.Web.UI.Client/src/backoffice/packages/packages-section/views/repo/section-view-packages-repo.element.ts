import { html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';

@customElement('umb-section-view-packages-repo')
export class UmbSectionViewPackagesRepoElement extends LitElement {
	
	render() {
		return html`
			<uui-box headline="Popular"></uui-box>
			<uui-box headline="Promoted"></uui-box>
		`;
	}
}

export default UmbSectionViewPackagesRepoElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-section-view-packages-repo': UmbSectionViewPackagesRepoElement;
	}
}
