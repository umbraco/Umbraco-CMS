import { UmbMediaDetailRepository } from '../../repository/index.js';
import type {
	UmbMediaCaptionAltTextModalData,
	UmbMediaCaptionAltTextModalValue,
} from './media-caption-alt-text-modal.token.js';
import { css, html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import type { UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-media-caption-alt-text-modal')
export class UmbMediaCaptionAltTextModalElement extends UmbModalBaseElement<
	UmbMediaCaptionAltTextModalData,
	UmbMediaCaptionAltTextModalValue
> {
	#mediaUnique?: string;
	readonly #mediaDetailRepository = new UmbMediaDetailRepository(this);

	override connectedCallback() {
		super.connectedCallback();
		this.#mediaUnique = this.data?.mediaUnique;
		this.#getMediaDetail();
	}

	async #getMediaDetail() {
		if (!this.#mediaUnique) return;
		const { data } = await this.#mediaDetailRepository.requestByUnique(this.#mediaUnique);
		if (!data) return;

		this.value = { ...this.value, altText: this.value?.altText ?? data.variants[0].name, url: data.urls[0]?.url ?? '' };
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

					<figure id="mainobject">
						<img src=${this.value?.url ?? ''} alt=${this.value?.altText ?? ''} />
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
