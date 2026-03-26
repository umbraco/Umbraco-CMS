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

@customElement('umb-media-caption-alt-text-modal')
export class UmbMediaCaptionAltTextModalElement extends UmbModalBaseElement<
	UmbMediaCaptionAltTextModalData,
	UmbMediaCaptionAltTextModalValue
> {
	#mediaUnique?: string;
	readonly #mediaDetailRepository = new UmbMediaDetailRepository(this);
	readonly #mediaUrlRepository = new UmbMediaUrlRepository(this);

	#naturalRatio?: number;

	@state()
	private _aspectLocked = true;

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

		// Auto-populate dimensions from natural image size if not already set
		if (url && !this.value.width) {
			const dims = await imageSize(url);
			this.#naturalRatio = dims.naturalWidth / dims.naturalHeight;

			let width = dims.naturalWidth;
			let height = dims.naturalHeight;

			// Cap to maxImageSize proportionally
			const maxSize = this.data?.maxImageSize;
			if (maxSize && (width > maxSize || height > maxSize)) {
				const ratio = Math.min(maxSize / width, maxSize / height);
				width = Math.round(width * ratio);
				height = Math.round(height * ratio);
			}

			this.value = {
				...this.value,
				width,
				height,
			};
		} else if (this.value.width && this.value.height) {
			this.#naturalRatio = this.value.width / this.value.height;
		}
	}

	#onWidthInput(e: UUIInputEvent) {
		const width = parseInt(e.target.value as string, 10);
		if (isNaN(width)) return;

		if (this._aspectLocked && this.#naturalRatio) {
			const height = Math.round(width / this.#naturalRatio);
			this.value = { ...this.value, width, height };
		} else {
			this.value = { ...this.value, width };
		}
	}

	#onHeightInput(e: UUIInputEvent) {
		const height = parseInt(e.target.value as string, 10);
		if (isNaN(height)) return;

		if (this._aspectLocked && this.#naturalRatio) {
			const width = Math.round(height * this.#naturalRatio);
			this.value = { ...this.value, width, height };
		} else {
			this.value = { ...this.value, height };
		}
	}

	#onToggleAspectLock() {
		this._aspectLocked = !this._aspectLocked;
	}

	override render() {
		return html`
			<umb-body-layout .headline=${this.localize.term('defaultdialogs_editSelectedMedia')}>
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

					<div id="dimensions">
						<div class="dimension-field">
							<uui-label for="width">${this.localize.term('general_width')}</uui-label>
							<uui-input
								id="width"
								type="number"
								label=${this.localize.term('general_width')}
								placeholder=${this.localize.term('general_width')}
								min="0"
								.value=${this.value?.width?.toString() ?? ''}
								@input=${this.#onWidthInput}>
								<span class="extra" slot="append">px</span>
							</uui-input>
						</div>
						<uui-button
							compact
							label=${this.localize.term('general_constrainProportions')}
							title=${this.localize.term('general_constrainProportions')}
							look=${this._aspectLocked ? 'primary' : 'default'}
							@click=${this.#onToggleAspectLock}>
							<uui-icon name=${this._aspectLocked ? 'icon-lock' : 'icon-unlocked'}></uui-icon>
						</uui-button>
						<div class="dimension-field">
							<uui-label for="height">${this.localize.term('general_height')}</uui-label>
							<uui-input
								id="height"
								type="number"
								label=${this.localize.term('general_height')}
								placeholder=${this.localize.term('general_height')}
								min="0"
								.value=${this.value?.height?.toString() ?? ''}
								@input=${this.#onHeightInput}>
								<span class="extra" slot="append">px</span>
							</uui-input>
						</div>
					</div>

					<figure id="mainobject">
						<img
							src=${this.value?.url ?? ''}
							alt=${this.value?.altText ?? ''}
							style=${this.value?.width && this.value?.height
								? `aspect-ratio: ${this.value.width} / ${this.value.height}`
								: ''} />
						<figcaption>${this.value?.caption ?? ''}</figcaption>
					</figure>
				</div>
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

			#dimensions {
				display: flex;
				align-items: end;
				gap: var(--uui-size-space-3);
				margin-bottom: var(--uui-size-layout-1);

				.dimension-field {
					flex: 1;
					display: flex;
					flex-direction: column;
					gap: var(--uui-size-space-1);

					uui-input {
						width: 100%;
						margin-bottom: 0;
					}

					.extra {
						user-select: none;
						height: 100%;
						padding: 0 var(--uui-size-3);
						border-left: 1px solid var(--uui-input-border-color, var(--uui-color-border));
						background: var(--uui-color-background);
						color: var(--uui-color-text);
						display: flex;
						justify-content: center;
						align-items: center;
					}
				}

				uui-button {
					margin-bottom: var(--uui-size-space-1);
				}
			}

			#mainobject {
				display: flex;
				flex-direction: column;
				max-width: 100%;

				img {
					max-width: 100%;
					height: auto;
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
