import { css, html, LitElement } from 'lit';
import { customElement, property } from 'lit/decorators.js';

@customElement('umb-packages-market-place-section-view')
export class UmbPackagesMarketPlaceSectionViewElement extends LitElement {
	static styles = [css`
		#container {
			height: 100%;
			display: flex;
			align-items: stretch;
		}


		iframe {
			width: 100%;
  			height: 100%;
  			overflow: hidden;
			border: none;
		}
	`];

	// TODO: This URL comes from the server
	// Was previously found in 'Umbraco.Sys.ServerVariables.umbracoUrls.marketplaceUrl'
	@property()
	marketplaceUrl: string = 'https://marketplace.umbraco.com/?umbversion=11.1.0&style=backoffice';

	render() {
		return html`
			<div id="container">
				<iframe
					src="${this.marketplaceUrl}"
					title="Umbraco Marketplace"
					allowfullscreen
					allow="geolocation; autoplay; clipboard-write; encrypted-media">
				</iframe>
			</div>`;
	}
}

export default UmbPackagesMarketPlaceSectionViewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-packages-market-place-section-view': UmbPackagesMarketPlaceSectionViewElement;
	}
}
