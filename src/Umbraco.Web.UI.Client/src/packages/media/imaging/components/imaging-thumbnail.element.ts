import { UmbImagingCropMode } from '../types.js';
import { UmbImagingRepository } from '../imaging.repository.js';
import { css, customElement, html, nothing, property, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

@customElement('umb-imaging-thumbnail')
export class UmbImagingThumbnailElement extends UmbLitElement {
	/**
	 * The unique identifier for the media item.
	 * @description This is also known as the media key and is used to fetch the resource.
	 */
	@property()
	unique = '';

	/**
	 * The width of the thumbnail in pixels.
	 * @default 300
	 */
	@property({ type: Number })
	width = 300;

	/**
	 * The height of the thumbnail in pixels.
	 * @default 300
	 */
	@property({ type: Number })
	height = 300;

	/**
	 * The mode of the thumbnail.
	 * @description The mode determines how the image is cropped.
	 * @enum {UmbImagingCropMode}
	 */
	@property()
	mode: UmbImagingCropMode = UmbImagingCropMode.MIN;

	/**
	 * The alt text for the thumbnail.
	 */
	@property()
	alt = '';

	/**
	 * The fallback icon for the thumbnail.
	 */
	@property()
	icon = 'icon-picture';

	/**
	 * The `loading` state of the thumbnail.
	 * @enum {'lazy' | 'eager'}
	 * @default 'lazy'
	 */
	@property()
	loading: (typeof HTMLImageElement)['prototype']['loading'] = 'lazy';

	@state()
	private _isLoading = true;

	@state()
	private _thumbnailUrl = '';

	#imagingRepository = new UmbImagingRepository(this);

	#intersectionObserver?: IntersectionObserver;

	override render() {
		return html` ${this.#renderThumbnail()} ${when(this._isLoading, () => this.#renderLoading())} `;
	}

	override connectedCallback() {
		super.connectedCallback();

		if (this.loading === 'lazy') {
			this.#intersectionObserver = new IntersectionObserver((entries) => {
				if (entries[0].isIntersecting) {
					this.#generateThumbnailUrl();
					this.#intersectionObserver?.disconnect();
				}
			});
			this.#intersectionObserver.observe(this);
		} else {
			this.#generateThumbnailUrl();
		}
	}

	override disconnectedCallback() {
		super.disconnectedCallback();
		this.#intersectionObserver?.disconnect();
	}

	#renderLoading() {
		return html`<div id="loader"><uui-loader></uui-loader></div>`;
	}

	#renderThumbnail() {
		if (this._isLoading) return nothing;

		return when(
			this._thumbnailUrl,
			() =>
				html`<img
					id="figure"
					src="${this._thumbnailUrl}"
					alt="${this.alt}"
					loading="${this.loading}"
					draggable="false" />`,
			() => html`<umb-icon id="icon" name="${this.icon}"></umb-icon>`,
		);
	}

	async #generateThumbnailUrl() {
		const { data } = await this.#imagingRepository.requestThumbnailUrls(
			[this.unique],
			this.height,
			this.width,
			this.mode,
		);

		this._thumbnailUrl = data?.[0]?.url ?? '';
		this._isLoading = false;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				position: relative;
				overflow: hidden;
				display: flex;
				justify-content: center;
				align-items: center;
				width: 100%;
				height: 100%;
			}

			#loader {
				display: flex;
				justify-content: center;
				align-items: center;
				height: 100%;
				width: 100%;
			}

			#figure {
				display: block;
				width: 100%;
				height: 100%;
				object-fit: cover;

				background-image: url('data:image/svg+xml;charset=utf-8,<svg xmlns="http://www.w3.org/2000/svg" width="100" height="100" fill-opacity=".1"><path d="M50 0h50v50H50zM0 50h50v50H0z"/></svg>');
				background-size: 10px 10px;
				background-repeat: repeat;
			}

			#icon {
				width: 100%;
				height: 100%;
				font-size: var(--uui-size-8);
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-imaging-thumbnail': UmbImagingThumbnailElement;
	}
}
