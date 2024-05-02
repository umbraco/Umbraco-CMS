import { UmbOEmbedRepository } from './repository/oembed.repository.js';
import { css, html, unsafeHTML, when, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbEmbeddedMediaModalData, UmbEmbeddedMediaModalValue } from '@umbraco-cms/backoffice/modal';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import type { UUIButtonState, UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-embedded-media-modal')
export class UmbEmbeddedMediaModalElement extends UmbModalBaseElement<
	UmbEmbeddedMediaModalData,
	UmbEmbeddedMediaModalValue
> {
	#oEmbedRepository = new UmbOEmbedRepository(this);

	@state()
	private _loading?: UUIButtonState;

	@state()
	private _width = 360;

	@state()
	private _height = 240;

	@state()
	private _url = '';

	@state()
	private _constrain = false;

	connectedCallback() {
		super.connectedCallback();
		if (this.data?.width) this._width = this.data.width;
		if (this.data?.height) this._height = this.data.height;
		if (this.data?.constrain) this._constrain = this.data.constrain;

		if (this.data?.url) {
			this._url = this.data.url;
			this.#getPreview();
		}
	}

	async #getPreview() {
		this._loading = 'waiting';

		const { data } = await this.#oEmbedRepository.requestOEmbed({
			url: this._url,
			maxWidth: this._width,
			maxHeight: this._height,
		});

		if (!data) {
			this._loading = 'failed';
			return;
		}

		this.value = { ...this.value, markup: data.markup, url: this._url };
		this._loading = 'success';
	}

	#onUrlChange(e: UUIInputEvent) {
		this._url = e.target.value as string;
	}

	#onWidthChange(e: UUIInputEvent) {
		this._width = parseInt(e.target.value as string, 10);
		//this.#getPreview();
		//this.#changeSize('width');
	}

	#onHeightChange(e: UUIInputEvent) {
		this._height = parseInt(e.target.value as string, 10);
		//this.#getPreview();
		//this.#changeSize('height');
	}

	#onConstrainChange() {
		this._constrain = !this._constrain;
		this.value = { ...this.value, constrain: this._constrain };
	}

	/**
	 * Calculates the width or height axis dimension when the other is changed.
	 * If constrain is false, axis change independently
	 * @param axis {string}
	 */
	/*
	#changeSize(axis: 'width' | 'height') {
		const resize = this._model.originalWidth !== this._model.width || this._model.originalHeight !== this._model.height;

		if (this._model.constrain) {
			if (axis === 'width') {
				this._model.height = Math.round((this._model.width / this._model.originalWidth) * this._model.height);
			} else {
				this._model.width = Math.round((this._model.height / this._model.originalHeight) * this._model.width);
			}
		}

		this._model.originalWidth = this._model.width;
		this._model.originalHeight = this._model.height;

		if (this._model.url !== '' && resize) {
			this.#getPreview();
		}
	}
	*/

	render() {
		return html`
			<umb-body-layout headline="Embed">
				<uui-box>
					<umb-property-layout label="URL" orientation="vertical">
						<div slot="editor">
							<uui-input .value=${this._url} type="text" @input=${this.#onUrlChange} required="true">
								<uui-button
									slot="append"
									look="primary"
									color="positive"
									@click=${this.#getPreview}
									label="Retrieve"></uui-button>
							</uui-input>
						</div>
					</umb-property-layout>

					${when(
						this._loading !== undefined,
						() =>
							html` <umb-property-layout label="Preview" orientation="vertical">
								<div slot="editor">
									${when(this._loading === 'waiting', () => html`<uui-loader-circle></uui-loader-circle>`)}
									${when(this.value?.markup, () => html`${unsafeHTML(this.value.markup)}`)}
								</div>
							</umb-property-layout>`,
					)}

					<umb-property-layout label="Max width" orientation="vertical">
						<uui-input slot="editor" .value=${this._width} type="number" @change=${this.#onWidthChange}></uui-input>
					</umb-property-layout>

					<umb-property-layout label="Max height" orientation="vertical">
						<uui-input slot="editor" .value=${this._height} type="number" @change=${this.#onHeightChange}></uui-input>
					</umb-property-layout>

					<umb-property-layout label="Constrain" orientation="vertical">
						<uui-toggle slot="editor" @change=${this.#onConstrainChange} .checked=${this._constrain}></uui-toggle>
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

	static styles = [
		UmbTextStyles,
		css`
			h3 {
				margin-left: var(--uui-size-space-5);
				margin-right: var(--uui-size-space-5);
			}

			uui-input {
				width: 100%;
				--uui-button-border-radius: 0;
			}

			.sr-only {
				clip: rect(0, 0, 0, 0);
				border: 0;
				height: 1px;
				margin: -1px;
				overflow: hidden;
				padding: 0;
				position: absolute;
				width: 1px;
			}

			umb-property-layout:first-child {
				padding-top: 0;
			}

			umb-property-layout:last-child {
				padding-bottom: 0;
			}

			p {
				margin-bottom: 0;
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
