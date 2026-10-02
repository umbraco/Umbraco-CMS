import { ExampleBlockViewBase } from './block-view-base.element.js';
import { exampleBlockViewStyles } from './block-view.styles.js';
import type { ExampleDocumentPickerItem } from './block-view.utils.js';
import { css, customElement, html, nothing, state } from '@umbraco-cms/backoffice/external/lit';

@customElement('example-hero-block-view')
export class ExampleHeroBlockView extends ExampleBlockViewBase {
	@state()
	private _imageUrl?: string;

	protected override async _load() {
		this._imageUrl = await this._requestMediaUrl(this.content?.image);
	}

	override render() {
		const link = (this.content?.link as Array<ExampleDocumentPickerItem> | undefined)?.[0];

		return html`
			<div class="hero" style=${this._imageUrl ? `background-image: url('${this._imageUrl}')` : ''}>
				<div class="overlay">
					<span class="eyebrow">Featured</span>
					<h2 class="headline">${this.content?.headline}</h2>
					<p class="subheadline">${this.content?.subheadline}</p>
					${link
						? html`<span class="cta">${link.name ?? 'Discover'} <span aria-hidden="true">→</span></span>`
						: nothing}
				</div>
			</div>
		`;
	}

	static override styles = [
		exampleBlockViewStyles,
		css`
			:host {
				border: none;
			}

			.hero {
				background-color: #111;
				background-size: cover;
				background-position: center;
			}

			.overlay {
				box-sizing: border-box;
				min-height: 340px;
				padding: var(--uui-size-layout-2);
				display: flex;
				flex-direction: column;
				align-items: flex-start;
				justify-content: flex-end;
				gap: var(--uui-size-space-4);
				background: linear-gradient(180deg, transparent 20%, rgb(0 0 0 / 70%));
				color: #fff;
			}

			.eyebrow {
				color: rgb(255 255 255 / 75%);
			}

			.headline {
				max-width: 14ch;
				font-size: clamp(2.25rem, 6vw, 4.5rem);
				font-weight: 800;
				letter-spacing: -0.04em;
				line-height: 0.95;
			}

			.subheadline {
				max-width: 44ch;
				font-size: 1.0625rem;
				line-height: 1.45;
				color: rgb(255 255 255 / 85%);
			}
		`,
	];
}

export default ExampleHeroBlockView;

declare global {
	interface HTMLElementTagNameMap {
		'example-hero-block-view': ExampleHeroBlockView;
	}
}
