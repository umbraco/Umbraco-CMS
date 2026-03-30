import { UmbMediaDetailRepository } from '../../repository/index.js';
import { UmbMediaUrlRepository } from '../../url/index.js';
import type {
	UmbMediaCaptionAltTextModalData,
	UmbMediaCaptionAltTextModalValue,
} from './media-caption-alt-text-modal.token.js';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { imageSize } from '@umbraco-cms/backoffice/utils';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import type { UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';
import type { UmbInputDimensionsElement } from '@umbraco-cms/backoffice/components';

@customElement('umb-media-caption-alt-text-modal')
export class UmbMediaCaptionAltTextModalElement extends UmbModalBaseElement<
	UmbMediaCaptionAltTextModalData,
	UmbMediaCaptionAltTextModalValue
> {
	#mediaUnique?: string;
	readonly #mediaDetailRepository = new UmbMediaDetailRepository(this);
	readonly #mediaUrlRepository = new UmbMediaUrlRepository(this);

	@state()
	private _naturalWidth?: number;

	@state()
	private _naturalHeight?: number;

	override connectedCallback() {
		super.connectedCallback();
		this.#mediaUnique = this.data?.mediaUnique;
		this.#getMediaDetail();
	}

	async #getMediaDetail() {
		if (!this.#mediaUnique) return;
		const { data: mediaData } = await this.#mediaDetailRepository.requestByUnique(this.#mediaUnique);
		if (!mediaData) return;

		const { data: mediaUrlData } = await this.#mediaUrlRepository.requestItems([this.#mediaUnique]);

		const url = mediaUrlData?.[0].url ?? '';

		this.value = {
			...this.value,
			altText: this.value?.altText ?? mediaData.variants[0].name,
			url,
		};

		if (url) {
			try {
				const dims = await imageSize(url);
				let naturalWidth = dims.naturalWidth;
				let naturalHeight = dims.naturalHeight;

				// Cap natural dimensions to maxImageSize proportionally
				const maxSize = this.data?.maxImageSize;
				if (maxSize && (naturalWidth > maxSize || naturalHeight > maxSize)) {
					const ratio = Math.min(maxSize / naturalWidth, maxSize / naturalHeight);
					naturalWidth = Math.round(naturalWidth * ratio);
					naturalHeight = Math.round(naturalHeight * ratio);
				}

				this._naturalWidth = naturalWidth;
				this._naturalHeight = naturalHeight;

				// Auto-populate dimensions from natural size if not already set
				if (!this.value.width) {
					this.value = {
						...this.value,
						width: naturalWidth,
						height: naturalHeight,
					};
				}
			} catch {
				// If image dimensions cannot be resolved, leave width/height unset.
			}
		}
	}

	#onDimensionsChange(e: Event) {
		const target = e.target as UmbInputDimensionsElement;
		this.value = { ...this.value, width: target.width, height: target.height };
	}

	override render() {
		return html`
			<umb-body-layout .headline=${this.localize.term('defaultdialogs_editSelectedMedia')}>
				<uui-box>
				<div id="wrapper">
					<uui-label for="alt-text">${this.localize.term('content_altTextOptional')}</uui-label>
					<uui-input
						id="alt-text"
						label="alt text"
						.value=${this.value?.altText ?? ''}
						@input=${(e: UUIInputEvent) =>
							(this.value = { ...this.value, altText: e.target.value as string })}></uui-input>

					<uui-label for="caption">${this.localize.term('content_captionTextOptional')}</uui-label>
					<uui-input
						id="caption"
						label="caption"
						.value=${this.value?.caption ?? ''}
						@input=${(e: UUIInputEvent) =>
							(this.value = { ...this.value, caption: e.target.value as string })}></uui-input>

					<umb-input-dimensions
						.width=${this.value?.width}
						.height=${this.value?.height}
						.naturalWidth=${this._naturalWidth}
						.naturalHeight=${this._naturalHeight}
						@change=${this.#onDimensionsChange}></umb-input-dimensions>

					<figure id="mainobject">
						<img
							src=${this.value?.url ?? ''}
							alt=${this.value?.altText ?? ''}
							style=${this.value?.width && this.value?.height
								? `width: ${this.value.width}px; aspect-ratio: ${this.value.width} / ${this.value.height}`
								: ''} />
						<figcaption>${this.value?.caption ?? ''}</figcaption>
					</figure>
				</div>
				</uui-box>
				<div slot="actions">
					<uui-button label=${this.localize.term('general_close')} @click=${this._rejectModal}></uui-button>
					<uui-button
						label=${this.localize.term('general_submit')}
						look="primary"
						color="positive"
						@click=${this._submitModal}></uui-button>
				</div>
			</umb-body-layout>
		`;
	}

	static override readonly styles = [
		css`
			uui-input {
				margin-bottom: var(--uui-size-layout-1);
			}

			#wrapper {
				display: flex;
				flex-direction: column;
			}

			umb-input-dimensions {
				margin-bottom: var(--uui-size-layout-1);
			}

			#mainobject {
				display: flex;
				flex-direction: column;
				align-items: center;
				max-width: 100%;

				img {
					max-width: 100%;
					height: auto;
					transition: aspect-ratio 0.3s ease;
				}
			}
		`,
	];
}

export default UmbMediaCaptionAltTextModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-caption-alt-text-modal': UmbMediaCaptionAltTextModalElement;
	}
}
