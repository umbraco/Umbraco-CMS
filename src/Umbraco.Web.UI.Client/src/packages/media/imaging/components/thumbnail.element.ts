import { UmbImagingCropMode } from '../types.js';
import { UmbImagingRepository } from '../imaging.repository.js';
import { css, customElement, html, property, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UmbEntityUpdatedEvent } from '@umbraco-cms/backoffice/entity-action';

/**
 * Displays a thumbnail for a media item, with optional server-side cropping and transparency support.
 * This is the recommended component for rendering media images in the backoffice.
 * @element umb-thumbnail
 * @cssprop [--umb-thumbnail-background] - Background shown behind the image. Defaults to a checkerboard
 * pattern that reveals transparency; set to `none` for a transparent background.
 * @csspart img - The underlying `<img>` element.
 */
@customElement('umb-thumbnail')
export class UmbThumbnailElement extends UmbLitElement {
	/**
	 * The unique identifier for the media item.
	 * @description This is also known as the media key and is used to fetch the resource.
	 */
	@property({ type: String })
	unique?: string;

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
	@property({ type: String })
	mode: UmbImagingCropMode = UmbImagingCropMode.MIN;

	/**
	 * The output format of the thumbnail.
	 * @description The format to convert the image to. If not specified, the backend automatically determines the best format based on the source file type.
	 */
	@property({ type: String })
	format?: string;

	/**
	 * The alt text for the thumbnail.
	 */
	@property({ type: String })
	alt = '';

	/**
	 * The fallback icon for the thumbnail.
	 */
	@property({ type: String })
	icon = 'icon-picture';

	/**
	 * The `loading` state of the thumbnail.
	 * @enum {'lazy' | 'eager'}
	 * @default 'lazy'
	 */
	@property({ type: String })
	loading: (typeof HTMLImageElement)['prototype']['loading'] = 'lazy';

	/**
	 * External loading state (e.g., when parent is waiting for metadata)
	 */
	@property({ type: Boolean, reflect: false, attribute: 'external-loading' })
	externalLoading = false;

	@state()
	private _isLoading = false;

	@state()
	private _thumbnailUrl = '';

	#imagingRepository = new UmbImagingRepository(this);

	#intersectionObserver?: IntersectionObserver;

	#actionEventContext?: typeof UMB_ACTION_EVENT_CONTEXT.TYPE;

	override render() {
		return when(
			this.externalLoading || this._isLoading,
			() => this.#renderLoading(),
			() => this.#renderThumbnail(),
		);
	}

	override connectedCallback() {
		super.connectedCallback();

		if (this.loading === 'lazy') {
			this.#intersectionObserver = new IntersectionObserver((entries) => {
				if (entries[0].isIntersecting) {
					this.#generateThumbnailUrl();
					this.#intersectionObserver?.disconnect();
					this.#observeActionEvent();
				}
			});
			this.#intersectionObserver.observe(this);
		} else {
			this.#generateThumbnailUrl();
			this.#observeActionEvent();
		}
	}

	override disconnectedCallback() {
		super.disconnectedCallback();
		this.#intersectionObserver?.disconnect();
		this.#removeActionEventListener();
	}

	/**
	 * Observes the action events for the current media item.
	 * Reloads the thumbnail when the media item is updated.
	 * Note: Call this only when the media item is in view or otherwise is supposed to be loaded already.
	 */
	#observeActionEvent() {
		this.consumeContext(UMB_ACTION_EVENT_CONTEXT, (actionEventContext) => {
			this.#removeActionEventListener();
			this.#actionEventContext = actionEventContext;
			this.#actionEventContext?.addEventListener(UmbEntityUpdatedEvent.TYPE, this.#onEntityUpdated as EventListener);
		});
	}

	#removeActionEventListener() {
		this.#actionEventContext?.removeEventListener(UmbEntityUpdatedEvent.TYPE, this.#onEntityUpdated as EventListener);
	}

	#onEntityUpdated = (event: UmbEntityUpdatedEvent) => {
		if (event.getUnique() === this.unique) {
			this.#generateThumbnailUrl();
		}
	};

	#renderLoading() {
		return html`<uui-loader-circle id="loader"></uui-loader-circle>`;
	}

	#renderThumbnail() {
		return when(
			this._thumbnailUrl,
			(url) =>
				html`<img
					id="figure"
					part="img"
					src=${url}
					alt=${this.alt}
					loading=${this.loading}
					decoding="async"
					draggable="false" />`,
			() => html`<umb-icon id="icon" name=${this.icon}></umb-icon>`,
		);
	}

	async #generateThumbnailUrl() {
		if (!this.unique) return;
		this._isLoading = true;

		const { data } = await this.#imagingRepository.requestResizedItems([this.unique], {
			height: this.height,
			width: this.width,
			mode: this.mode,
			format: this.format,
		});

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
				font-size: 2em;
				margin-bottom: 1em;
			}

			#figure {
				display: block;
				width: 100%;
				height: 100%;
				object-fit: contain;
				object-position: center;

				background-image: var(
					--umb-thumbnail-background,
					url('data:image/svg+xml;charset=utf-8,<svg xmlns="http://www.w3.org/2000/svg" width="100" height="100" fill-opacity=".1"><path d="M50 0h50v50H50zM0 50h50v50H0z"/></svg>')
				);
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
		'umb-thumbnail': UmbThumbnailElement;
	}
}
