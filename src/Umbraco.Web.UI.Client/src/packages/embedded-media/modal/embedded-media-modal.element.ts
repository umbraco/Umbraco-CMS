import { UmbOEmbedRepository } from '../repository/oembed.repository.js';
import type { UmbEmbeddedMediaModalData, UmbEmbeddedMediaModalValue } from './embedded-media-modal.token.js';
import { css, html, unsafeHTML, when, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { umbFocus } from '@umbraco-cms/backoffice/lit-element';
import { debounce } from '@umbraco-cms/backoffice/utils';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UUIButtonState, UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';
import type { UmbInputDimensionsElement } from '@umbraco-cms/backoffice/components';

const DEFAULT_WIDTH = 500;
const DEFAULT_HEIGHT = Math.round(500 / (16 / 9));

@customElement('umb-embedded-media-modal')
export class UmbEmbeddedMediaModalElement extends UmbModalBaseElement<
	UmbEmbeddedMediaModalData,
	UmbEmbeddedMediaModalValue
> {
	#oEmbedRepository = new UmbOEmbedRepository(this);
	#validUrl?: string;

	@state()
	private _loading?: UUIButtonState;

	@state()
	private _width = DEFAULT_WIDTH;

	@state()
	private _height = DEFAULT_HEIGHT;

	@state()
	private _constrain = true;

	@state()
	private _url = '';

	override connectedCallback() {
		super.connectedCallback();

		if (this.data?.width) this._width = this.data.width > 0 ? this.data.width : DEFAULT_WIDTH;
		if (this.data?.height) this._height = this.data.height > 0 ? this.data.height : DEFAULT_HEIGHT;
		if (this.data?.constrain !== undefined) this._constrain = this.data.constrain;

		this.value = { ...this.value, constrain: this._constrain, width: this._width, height: this._height };

		if (this.data?.url) {
			this._url = this.data.url;
			this.#getPreview();
		}
	}

	async #getPreview() {
		this._loading = 'waiting';

		const { data } = await this.#oEmbedRepository.requestOEmbed({
			url: this._url,
			maxWidth: this._width > 0 ? this._width : undefined,
			maxHeight: this._height > 0 ? this._height : undefined,
		});

		if (data) {
			this.#validUrl = this._url;
			this.value = { ...this.value, markup: data.markup, url: this.#validUrl };
			this._loading = 'success';
		} else {
			this.#validUrl = undefined;
			this._loading = 'failed';
		}
	}

	#onUrlChange(e: UUIInputEvent) {
		this._url = e.target.value as string;
	}

	#debouncedGetPreview = debounce(() => this.#getPreview(), 500);

	#onDimensionsChange(e: Event) {
		const target = e.target as UmbInputDimensionsElement;
		this._width = target.width ?? DEFAULT_WIDTH;
		this._height = target.height ?? DEFAULT_HEIGHT;
		this._constrain = target.locked;
		this.value = { ...this.value, width: this._width, height: this._height, constrain: this._constrain };
		this.#debouncedGetPreview();
	}

	protected override updated() {
		// Iframes don't have intrinsic aspect ratios like images, so height:auto collapses to 150px.
		// Read the provider's width/height attributes and apply aspect-ratio to keep proportions.
		const iframe = this.renderRoot.querySelector<HTMLIFrameElement>('#preview iframe');
		if (iframe?.width && iframe?.height) {
			iframe.style.aspectRatio = `${iframe.width} / ${iframe.height}`;
			iframe.style.height = 'auto';
		}
	}

	override render() {
		const isDisabled = !this.#validUrl;
		return html`
			<umb-body-layout headline="Embed">
				<uui-box>
					<umb-property-layout label=${this.localize.term('general_url')} orientation="vertical">
						<div slot="editor">
							<uui-input id="url" .value=${this._url} @input=${this.#onUrlChange} required="true" ${umbFocus()}>
								<uui-button
									slot="append"
									look="primary"
									color="positive"
									@click=${this.#getPreview}
									label=${this.localize.term('general_retrieve')}></uui-button>
							</uui-input>
						</div>
					</umb-property-layout>

					<umb-property-layout
						label=${this.localize.term('general_dimensions')}
						description=${this.localize.term('embeddedMedia_dimensionsDescription')}
						orientation="vertical">
						<umb-input-dimensions
							slot="editor"
							.width=${this._width}
							.height=${this._height}
							.locked=${this._constrain}
							?disabled=${isDisabled}
							@change=${this.#onDimensionsChange}></umb-input-dimensions>
					</umb-property-layout>

					${when(
						this.#validUrl !== undefined,
						() =>
							html` <umb-property-layout label=${this.localize.term('general_preview')} orientation="vertical">
								<div slot="editor">
									${when(this._loading === 'waiting', () => html`<uui-loader-circle></uui-loader-circle>`)}
									${when(this.value?.markup, () => html`<div id="preview">${unsafeHTML(this.value.markup)}</div>`)}
								</div>
							</umb-property-layout>`,
					)}
				</uui-box>

				<uui-button
					slot="actions"
					id="cancel"
					label=${this.localize.term('buttons_confirmActionCancel')}
					@click=${() => this.modalContext?.reject()}></uui-button>
				<uui-button
					slot="actions"
					id="submit"
					color="positive"
					look="primary"
					label=${this.localize.term('buttons_confirmActionConfirm')}
					@click=${() => this.modalContext?.submit()}></uui-button>
			</umb-body-layout>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			uui-input {
				width: 100%;
				--uui-button-border-radius: 0;
			}

			#preview {
				max-width: 100%;
				overflow: hidden;

				> *:first-child {
					max-width: 100%;
				}
			}

			umb-property-layout:first-child {
				padding-top: 0;
			}
		`,
	];
}

export default UmbEmbeddedMediaModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-embedded-media-modal': UmbEmbeddedMediaModalElement;
	}
}
