import { ExampleBlockViewBase } from './block-view-base.element.js';
import { exampleBlockViewStyles } from './block-view.styles.js';
import { getDocumentName, getDocumentValue } from './block-view.utils.js';
import { css, customElement, html, nothing, state } from '@umbraco-cms/backoffice/external/lit';
import type { UmbDocumentDetailModel } from '@umbraco-cms/backoffice/document';

@customElement('example-article-teaser-block-view')
export class ExampleArticleTeaserBlockView extends ExampleBlockViewBase {
	@state()
	private _article?: UmbDocumentDetailModel;

	@state()
	private _imageUrl?: string;

	protected override async _load() {
		this._article = await this._requestDocument(this.content?.article);
		this._imageUrl = await this._requestMediaUrl(getDocumentValue(this._article, 'heroImage', this._culture));
	}

	override render() {
		if (!this._article) return html`<div class="body"><p class="lead">Pick an article</p></div>`;

		return html`
			<div class="media">${this._imageUrl ? html`<img src=${this._imageUrl} alt="" />` : nothing}</div>
			<div class="body">
				<span class="eyebrow">Article</span>
				<h4 class="title">${getDocumentName(this._article, this._culture)}</h4>
				<p class="lead">${getDocumentValue<string>(this._article, 'teaser', this._culture)}</p>
				<span class="cta">Read <span aria-hidden="true">→</span></span>
			</div>
		`;
	}

	static override styles = [
		exampleBlockViewStyles,
		css`
			img {
				aspect-ratio: 3 / 2;
			}
		`,
	];
}

export default ExampleArticleTeaserBlockView;

declare global {
	interface HTMLElementTagNameMap {
		'example-article-teaser-block-view': ExampleArticleTeaserBlockView;
	}
}
