import { html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';

@customElement('umb-packages-market-place-section-view')
export class UmbPackagesMarketPlaceSectionViewElement extends LitElement {
	render() {
		return html`<div>Render Marketplace</div> `;
	}
}

export default UmbPackagesMarketPlaceSectionViewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-packages-market-place-section-view': UmbPackagesMarketPlaceSectionViewElement;
	}
}
