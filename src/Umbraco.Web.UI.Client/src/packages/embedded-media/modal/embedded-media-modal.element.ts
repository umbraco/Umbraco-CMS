import { UmbOEmbedRepository } from '../repository/oembed.repository.js';
import type { UmbEmbeddedMediaModalData, UmbEmbeddedMediaModalValue } from './embedded-media-modal.token.js';
import { css, html, unsafeHTML, when, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { umbFocus } from '@umbraco-cms/backoffice/lit-element';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UUIButtonState, UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';

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
	private _width = 360;

	@state()
	private _height = 240;

	@state()
	private _url = '';

	override connectedCallback() {
		super.connectedCallback();

		if (this.data?.width) this._width = this.data.width;
		if (this.data?.height) this._height = this.data.height;
		if (this.data?.constrain) this.value = { ...this.value, constrain: this.data.constrain };

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

	#onWidthChange(e: UUIInputEvent) {
		this._width = parseInt(e.target.value as string, 10);
		this.value = { ...this.value, width: this._width };
		this.#getPreview();
	}

	#onHeightChange(e: UUIInputEvent) {
		this._height = parseInt(e.target.value as string, 10);
		this.value = { ...this.value, height: this._height };
		this.#getPreview();
	}

	#onConstrainChange() {
		const constrain = !this.value?.constrain;
		this.value = { ...this.value, constrain };
	}

	override render() {
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

					${when(
						this.#validUrl !== undefined,
						() =>
							html` <umb-property-layout label=${this.localize.term('general_preview')} orientation="vertical">
								<div slot="editor">
									${when(this._loading === 'waiting', () => html`<uui-loader-circle></uui-loader-circle>`)}
									${when(this.value?.markup, () => html`${unsafeHTML(this.value.markup)}`)}
								</div>
							</umb-property-layout>`,
					)}

					<umb-property-layout label=${this.localize.term('general_width')} orientation="vertical">
						<uui-input
							slot="editor"
							.value=${this._width}
							type="number"
							@change=${this.#onWidthChange}
							?disabled=${this.#validUrl ? false : true}></uui-input>
					</umb-property-layout>

					<umb-property-layout label=${this.localize.term('general_height')} orientation="vertical">
						<uui-input
							slot="editor"
							.value=${this._height}
							type="number"
							@change=${this.#onHeightChange}
							?disabled=${this.#validUrl ? false : true}></uui-input>
					</umb-property-layout>

					<umb-property-layout label=${this.localize.term('general_constrainProportions')} orientation="vertical">
						<uui-toggle
							slot="editor"
							@change=${this.#onConstrainChange}
							.checked=${this.value?.constrain ?? false}></uui-toggle>
					</umb-property-layout>
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

			umb-property-layout:first-child {
				padding-top: 0;
			}

			umb-property-layout:last-child {
				padding-bottom: 0;
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
