import { UmbMediaUrlRepository } from '../../media/url/index.js';
import { css, customElement, html, nothing, property, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

@customElement('umb-media-image')
export class UmbMediaImageElement extends UmbLitElement {
	/**
	 * The unique identifier for the media item.
	 * @description This is also known as the media key and is used to fetch the resource.
	 */
	@property()
	unique?: string;

	/**
	 * The alt text for the thumbnail.
	 */
	@property()
	alt?: string;

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
	private _imageUrl = '';

	#mediaRepository = new UmbMediaUrlRepository(this);

	#intersectionObserver?: IntersectionObserver;

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

	async #generateThumbnailUrl() {
		if (!this.unique) throw new Error('Unique is missing');
		const { data } = await this.#mediaRepository.requestItems([this.unique]);

		this._imageUrl = data?.[0]?.url ?? '';
		this._isLoading = false;
	}

	override render() {
		return html` ${this.#renderThumbnail()} ${when(this._isLoading, () => this.#renderLoading())} `;
	}

	#renderLoading() {
		return html`<div id="loader"><uui-loader></uui-loader></div>`;
	}

	#renderThumbnail() {
		if (this._isLoading) return nothing;

		return when(
			this._imageUrl,
			() =>
				html`<img
					part="img"
					src="${this._imageUrl}"
					alt="${this.alt ?? ''}"
					loading="${this.loading}"
					draggable="false" />`,
			() => html`<umb-icon id="icon" name="${this.icon}"></umb-icon>`,
		);
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: contents;
			}

			#loader {
				display: flex;
				justify-content: center;
				align-items: center;
				height: 100%;
				width: 100%;
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
		'umb-media-image': UmbMediaImageElement;
	}
}
