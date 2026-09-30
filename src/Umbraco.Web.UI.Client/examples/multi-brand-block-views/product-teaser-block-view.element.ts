import { ExampleBlockViewBase } from './block-view-base.element.js';
import { exampleBlockViewStyles } from './block-view.styles.js';
import { getDocumentName, getDocumentValue } from './block-view.utils.js';
import { css, customElement, html, nothing, state } from '@umbraco-cms/backoffice/external/lit';
import type { UmbDocumentDetailModel } from '@umbraco-cms/backoffice/document';

@customElement('example-product-teaser-block-view')
export class ExampleProductTeaserBlockView extends ExampleBlockViewBase {
	@state()
	private _product?: UmbDocumentDetailModel;

	@state()
	private _imageUrl?: string;

	protected override async _load() {
		this._product = await this._requestDocument(this.content?.product);
		this._imageUrl = await this._requestMediaUrl(getDocumentValue(this._product, 'picture', this._culture));
	}

	override render() {
		if (!this._product) return html`<div class="body"><p class="lead">Pick a product</p></div>`;

		const price = getDocumentValue<number>(this._product, 'price', this._culture);
		const inStock = getDocumentValue<boolean>(this._product, 'inStock', this._culture);

		return html`
			<div class="media">${this._imageUrl ? html`<img src=${this._imageUrl} alt="" />` : nothing}</div>
			<div class="body">
				${this.content?.label ? html`<span class="pill">${this.content.label}</span>` : nothing}
				<h4 class="title">${getDocumentName(this._product, this._culture)}</h4>
				<div class="row">
					<span class="price">${price !== undefined ? `${price} DKK` : ''}</span>
					<span class="stock ${inStock ? 'in' : 'out'}">${inStock ? 'In stock' : 'Out of stock'}</span>
				</div>
			</div>
		`;
	}

	static override styles = [
		exampleBlockViewStyles,
		css`
			img {
				aspect-ratio: 1;
			}

			.row {
				display: flex;
				align-items: baseline;
				justify-content: space-between;
				align-self: stretch;
			}

			.price {
				font-size: 1.125rem;
				font-weight: 800;
				letter-spacing: -0.02em;
			}

			.stock {
				font-size: 0.6875rem;
				font-weight: 700;
				letter-spacing: 0.12em;
				text-transform: uppercase;
			}

			.stock::before {
				content: '';
				display: inline-block;
				width: 0.5em;
				height: 0.5em;
				margin-right: 0.6em;
				border-radius: 50%;
				background-color: currentColor;
			}

			.stock.in {
				color: var(--uui-color-positive);
			}

			.stock.out {
				color: var(--uui-color-danger);
			}
		`,
	];
}

export default ExampleProductTeaserBlockView;

declare global {
	interface HTMLElementTagNameMap {
		'example-product-teaser-block-view': ExampleProductTeaserBlockView;
	}
}
