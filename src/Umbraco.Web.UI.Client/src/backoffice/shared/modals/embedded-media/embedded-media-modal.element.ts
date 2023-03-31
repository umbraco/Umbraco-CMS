import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { when } from 'lit-html/directives/when.js';
import { unsafeHTML } from 'lit-html/directives/unsafe-html.js';
import {
	OEmbedResult,
	OEmbedStatus,
	UmbEmbeddedMediaModalData,
	UmbEmbeddedMediaModalResult,
	UmbModalHandler,
} from '@umbraco-cms/backoffice/modal';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

interface UmbEmbeddedMediaModalModel {
	url?: string;
	info?: string;
	a11yInfo?: string;
	originalWidth: number;
	originalHeight: number;
	width: number;
	height: number;
	constrain: boolean;
}

@customElement('umb-embedded-media-modal')
export class UmbEmbeddedMediaModalElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
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

			umb-workspace-property-layout:first-child {
				padding-top: 0;
			}

			umb-workspace-property-layout:last-child {
				padding-bottom: 0;
			}

			p {
				margin-bottom: 0;
			}
		`,
	];

	#loading = false;
	#embedResult!: OEmbedResult;

	@property({ attribute: false })
	modalHandler?: UmbModalHandler<UmbEmbeddedMediaModalData, UmbEmbeddedMediaModalResult>;

	@property({ type: Object })
	data?: UmbEmbeddedMediaModalData;

	#handleConfirm() {
		this.modalHandler?.submit({ selection: this.#embedResult });
	}

	#handleCancel() {
		this.modalHandler?.reject();
	}

	@state()
	private _model: UmbEmbeddedMediaModalModel = {
		url: '',
		width: 360,
		height: 240,
		constrain: true,
		info: '',
		a11yInfo: '',
		originalHeight: 240,
		originalWidth: 360,
	};

	connectedCallback() {
		super.connectedCallback();
		if (this.data?.url) {
			Object.assign(this._model, this.data);
			this.#getPreview();
		}
	}

	async #getPreview() {
		this._model.info = '';
		this._model.a11yInfo = '';

		this.#loading = true;
		this.requestUpdate('_model');

		try {
			// TODO => use backend cli when available
			const result = await fetch(
				umbracoPath('/rteembed?') +
					new URLSearchParams({
						url: this._model.url,
						width: this._model.width?.toString(),
						height: this._model.height?.toString(),
					} as { [key: string]: string })
			);

			this.#embedResult = await result.json();

			switch (this.#embedResult.oEmbedStatus) {
				case 0:
					this.#onPreviewFailed('Not supported');
					break;
				case 1:
					this.#onPreviewFailed('Could not embed media - please ensure the URL is valid');
					break;
				case 2:
					this._model.info = '';
					this._model.a11yInfo = 'Retrieved URL';
					break;
			}
		} catch (e) {
			this.#onPreviewFailed('Could not embed media - please ensure the URL is valid');
		}

		this.#loading = false;
		this.requestUpdate('_model');
	}

	#onPreviewFailed(message: string) {
		this._model.info = message;
		this._model.a11yInfo = message;
	}

	#onUrlChange(e: InputEvent) {
		this._model.url = (e.target as HTMLInputElement).value;
		this.requestUpdate('_model');
	}

	#onWidthChange(e: InputEvent) {
		this._model.width = parseInt((e.target as HTMLInputElement).value, 10);
		this.#changeSize('width');
	}

	#onHeightChange(e: InputEvent) {
		this._model.height = parseInt((e.target as HTMLInputElement).value, 10);
		this.#changeSize('height');
	}

	/**
	 * Calculates the width or height axis dimension when the other is changed.
	 * If constrain is false, axis change independently
	 * @param axis {string}
	 */
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

	#onConstrainChange() {
		this._model.constrain = !this._model.constrain;
	}

	/**
	 * If the embed does not support dimensions, or was not requested successfully
	 * the width, height and constrain controls are disabled
	 * @returns {boolean}
	 */
	#dimensionControlsDisabled() {
		return !this.#embedResult?.supportsDimensions || this.#embedResult?.oEmbedStatus !== OEmbedStatus.Success;
	}

	render() {
		return html`
			<umb-workspace-layout headline="Embed">
				<uui-box>
					<umb-workspace-property-layout label="URL" orientation="vertical">
						<div slot="editor">
							<uui-input .value=${this._model.url} type="text" @change=${this.#onUrlChange} required="true">
								<uui-button
									slot="append"
									look="primary"
									color="positive"
									@click=${this.#getPreview}
									?disabled=${!this._model.url}
									label="Retrieve"></uui-button>
							</uui-input>
						</div>
					</umb-workspace-property-layout>

					${when(
						this.#embedResult?.oEmbedStatus === OEmbedStatus.Success || this._model.a11yInfo,
						() => html` <umb-workspace-property-layout label="Preview" orientation="vertical">
							<div slot="editor">
								${when(this.#loading, () => html`<uui-loader-circle></uui-loader-circle>`)}
								${when(this.#embedResult.markup, () => html`${unsafeHTML(this.#embedResult.markup)}`)}
								${when(this._model.info, () => html` <p aria-hidden="true">${this._model.info}</p>`)}
								${when(this._model.a11yInfo, () => html` <p class="sr-only" role="alert">${this._model.a11yInfo}</p>`)}
							</div>
						</umb-workspace-property-layout>`
					)}

					<umb-workspace-property-layout label="Width" orientation="vertical">
						<uui-input
							slot="editor"
							.value=${this._model.width}
							type="number"
							?disabled=${this.#dimensionControlsDisabled()}
							@change=${this.#onWidthChange}></uui-input>
					</umb-workspace-property-layout>

					<umb-workspace-property-layout label="Height" orientation="vertical">
						<uui-input
							slot="editor"
							.value=${this._model.height}
							type="number"
							?disabled=${this.#dimensionControlsDisabled()}
							@change=${this.#onHeightChange}></uui-input>
					</umb-workspace-property-layout>

					<umb-workspace-property-layout label="Constrain" orientation="vertical">
						<uui-toggle
							slot="editor"
							@change=${this.#onConstrainChange}
							?disabled=${this.#dimensionControlsDisabled()}
							.checked=${this._model.constrain}></uui-toggle>
					</umb-workspace-property-layout>
				</uui-box>

				<uui-button slot="actions" id="cancel" label="Cancel" @click=${this.#handleCancel}>Cancel</uui-button>
				<uui-button
					slot="actions"
					id="submit"
					color="positive"
					look="primary"
					label="Submit"
					@click=${this.#handleConfirm}></uui-button>
			</umb-workspace-layout>
		`;
	}
}

export default UmbEmbeddedMediaModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-embedded-media-modal': UmbEmbeddedMediaModalElement;
	}
}
