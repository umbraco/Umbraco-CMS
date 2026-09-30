import { ExampleBlockViewBase } from './block-view-base.element.js';
import { exampleBlockViewStyles } from './block-view.styles.js';
import { css, customElement, html, nothing, state } from '@umbraco-cms/backoffice/external/lit';

@customElement('example-image-block-view')
export class ExampleImageBlockView extends ExampleBlockViewBase {
	@state()
	private _imageUrl?: string;

	protected override async _load() {
		this._imageUrl = await this._requestMediaUrl(this.content?.image);
	}

	override render() {
		return html`
			<div class="media">
				${this._imageUrl ? html`<img src=${this._imageUrl} alt=${(this.content?.caption as string) ?? ''} />` : nothing}
			</div>
			${this.content?.caption
				? html`<div class="caption"><span class="eyebrow">${this.content.caption}</span></div>`
				: nothing}
		`;
	}

	static override styles = [
		exampleBlockViewStyles,
		css`
			img {
				aspect-ratio: 4 / 3;
			}

			.caption {
				padding: var(--uui-size-space-4) var(--uui-size-space-5);
				border-top: 1px solid var(--uui-color-border);
			}
		`,
	];
}

export default ExampleImageBlockView;

declare global {
	interface HTMLElementTagNameMap {
		'example-image-block-view': ExampleImageBlockView;
	}
}
