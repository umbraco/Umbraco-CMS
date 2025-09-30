import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbPackageRepository } from '@umbraco-cms/backoffice/package';
import type { UmbSectionViewElement } from '@umbraco-cms/backoffice/section';

@customElement('umb-packages-marketplace-section-view')
export class UmbPackagesMarketplaceSectionViewElement extends UmbLitElement implements UmbSectionViewElement {
	@state()
	private _marketplaceUrl?: string;

	#packageRepository = new UmbPackageRepository(this);

	constructor() {
		super();

		this.#getMarketplaceUrl();
	}

	async #getMarketplaceUrl() {
		const configuration = await this.#packageRepository.configuration();
		this.observe(
			configuration,
			(configuration) => {
				this._marketplaceUrl = configuration.marketplaceUrl;
			},
			'_observeConfiguration',
		);
	}

	override render() {
		if (!this._marketplaceUrl) return html`<div id="loader"><uui-loader></uui-loader></div>`;
		return html`
			<div id="container">
				<iframe
					src=${this._marketplaceUrl}
					title="Umbraco Marketplace"
					allowfullscreen
					allow="geolocation; autoplay; clipboard-write; encrypted-media">
				</iframe>
			</div>
		`;
	}

	static override styles = [
		css`
			:host {
				height: 100%;
				display: block;
			}

			#loader {
				height: 100%;
				display: flex;
				align-items: center;
				justify-content: center;
			}

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
		`,
	];
}

export default UmbPackagesMarketplaceSectionViewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-packages-marketplace-section-view': UmbPackagesMarketplaceSectionViewElement;
	}
}
