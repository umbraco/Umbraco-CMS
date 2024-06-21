import type {
	UmbMediaCaptionAltTextModalData,
	UmbMediaCaptionAltTextModalValue,
} from './media-caption-alt-text-modal.token.js';
import { css, html, customElement, state, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import '@umbraco-cms/backoffice/block-type';
import { UmbMediaDetailRepository } from '@umbraco-cms/backoffice/media';
import type { UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-media-caption-alt-text-modal')
export class UmbMediaCaptionAltTextModalElement extends UmbModalBaseElement<
	UmbMediaCaptionAltTextModalData,
	UmbMediaCaptionAltTextModalValue
> {
	#mediaUnique?: string;
	#mediaDetailRepository = new UmbMediaDetailRepository(this);

	override connectedCallback() {
		super.connectedCallback();
		this.#mediaUnique = this.data?.mediaUnique;
		this.#getMediaDetail();
	}

	async #getMediaDetail() {
		if (!this.#mediaUnique) return;
		const { data } = await this.#mediaDetailRepository.requestByUnique(this.#mediaUnique);
		if (!data) return;

		this.value = { altText: data.variants[0].name, caption: undefined, url: data.urls[0]?.url ?? '' };
	}

	render() {
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
						@input=${(e: UUIInputEvent) =>
							(this.value = { ...this.value, caption: e.target.value as string })}></uui-input>

					<img src=${this.value?.url ?? ''} alt=${this.value?.altText ?? ''} />
					${this.value?.caption ?? ''}
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

	static override styles = [
		css`
			uui-input {
				margin-bottom: var(--uui-size-layout-1);
			}

			#wrapper {
				display: flex;
				flex-direction: column;
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
