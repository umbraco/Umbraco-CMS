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
	#ratio?: number;

	@state()
	private _loading?: UUIButtonState;

	@state()
	private _width = 360;

	@state()
	private _height = 240;

	@state()
	private _constrain = false;

	@state()
	private _url = '';

	override connectedCallback() {
		super.connectedCallback();

		if (this.data?.width) this._width = this.data.width;
		if (this.data?.height) this._height = this.data.height;
		if (this.data?.constrain) this._constrain = this.data.constrain;

		if (this._width && this._height) {
			this.#ratio = this._width / this._height;
		}

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

	#onWidthChange(e: UUIInputEvent) {
		const width = parseInt(e.target.value as string, 10);
		if (isNaN(width)) return;

		this._width = width;

		if (this._constrain && this.#ratio) {
			this._height = Math.round(width / this.#ratio);
		}

		this.value = { ...this.value, width: this._width, height: this._height };
		this.#getPreview();
	}

	#onHeightChange(e: UUIInputEvent) {
		const height = parseInt(e.target.value as string, 10);
		if (isNaN(height)) return;

		this._height = height;

		if (this._constrain && this.#ratio) {
			this._width = Math.round(height * this.#ratio);
		}

		this.value = { ...this.value, width: this._width, height: this._height };
		this.#getPreview();
	}

	#onToggleConstrain() {
		this._constrain = !this._constrain;

		// Update ratio when locking, based on current dimensions
		if (this._constrain && this._width && this._height) {
			this.#ratio = this._width / this._height;
		}

		this.value = { ...this.value, constrain: this._constrain };
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

					<umb-property-layout label=${this.localize.term('general_dimensions')} orientation="vertical">
						<div id="dimensions" slot="editor">
							<div class="dimension-field">
								<uui-label for="width">${this.localize.term('general_width')}</uui-label>
								<uui-input
									id="width"
									type="number"
									label=${this.localize.term('general_width')}
									placeholder=${this.localize.term('general_width')}
									min="0"
									.value=${this._width.toString()}
									@change=${this.#onWidthChange}
									?disabled=${isDisabled}>
									<span class="extra" slot="append">px</span>
								</uui-input>
							</div>
							<uui-button
								compact
								label=${this.localize.term('general_constrainProportions')}
								title=${this.localize.term('general_constrainProportions')}
								look=${this._constrain ? 'primary' : 'default'}
								?disabled=${isDisabled}
								@click=${this.#onToggleConstrain}>
								<uui-icon name=${this._constrain ? 'icon-lock' : 'icon-unlocked'}></uui-icon>
							</uui-button>
							<div class="dimension-field">
								<uui-label for="height">${this.localize.term('general_height')}</uui-label>
								<uui-input
									id="height"
									type="number"
									label=${this.localize.term('general_height')}
									placeholder=${this.localize.term('general_height')}
									min="0"
									.value=${this._height.toString()}
									@change=${this.#onHeightChange}
									?disabled=${isDisabled}>
									<span class="extra" slot="append">px</span>
								</uui-input>
							</div>
						</div>
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

			#dimensions {
				display: flex;
				align-items: end;
				gap: var(--uui-size-space-3);

				.dimension-field {
					flex: 1;
					display: flex;
					flex-direction: column;
					gap: var(--uui-size-space-1);

					uui-input {
						display: block;
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
		`,
	];
}

export default UmbEmbeddedMediaModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-embedded-media-modal': UmbEmbeddedMediaModalElement;
	}
}
