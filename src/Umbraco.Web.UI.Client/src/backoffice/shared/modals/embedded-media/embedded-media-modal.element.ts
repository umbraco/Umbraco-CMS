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

	#info?: string;
	#a11yInfo?: string;
	#loading = false;
	#embedResult?: OEmbedResult;

	static defaultWidth = 360;
	static defaultHeight = 240;

	@property({ attribute: false })
	modalHandler?: UmbModalHandler<UmbEmbeddedMediaModalData, UmbEmbeddedMediaModalResult>;

	@property({ type: Object })
	data?: UmbEmbeddedMediaModalData;

	@state()
	private _model: UmbEmbeddedMediaModalResult = {
		url: '',
		width: UmbEmbeddedMediaModalElement.defaultWidth,
		height: UmbEmbeddedMediaModalElement.defaultHeight,
		constrain: true,
		originalHeight: UmbEmbeddedMediaModalElement.defaultHeight,
		originalWidth: UmbEmbeddedMediaModalElement.defaultWidth,
	};

	connectedCallback() {
		super.connectedCallback();
		if (this.data?.url) {
			Object.assign(this._model, this.data);
			this.#getPreview();
		}
	}

	#handleConfirm() {
		this.modalHandler?.submit(this._model);
	}

	#handleCancel() {
		this.modalHandler?.reject();
	}

	async #getPreview() {
		this.#info = '';
		this.#a11yInfo = '';

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

			switch (this.#embedResult?.oEmbedStatus) {
				case 0:
					this.#setMessages('Not supported');
					break;
				case 1:
					this.#setMessages('Could not embed media - please ensure the URL is valid');
					break;
				case 2:
					this.#setMessages('', 'Retrieved URL');
					this._model.preview = this.#embedResult.markup;
					break;
			}
		} catch (e) {
			this.#setMessages('Could not embed media - please ensure the URL is valid');
		}

		this.#loading = false;
		this.requestUpdate('_model');
	}

	#setMessages(info: string, a11yMessage = info) {
		this.#info = info;
		this.#a11yInfo = a11yMessage;
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
	 * TODO => should this regenerate the preview? Currently does, but is it neccessary?
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
						this.#embedResult?.oEmbedStatus === OEmbedStatus.Success || this.#a11yInfo,
						() => html` <umb-workspace-property-layout label="Preview" orientation="vertical">
							<div slot="editor">
								${when(this.#loading, () => html`<uui-loader-circle></uui-loader-circle>`)}
								${when(this.#embedResult?.markup, () => html`${unsafeHTML(this.#embedResult?.markup)}`)}
								${when(this.#info, () => html` <p aria-hidden="true">${this.#info}</p>`)}
								${when(this.#a11yInfo, () => html` <p class="sr-only" role="alert">${this.#a11yInfo}</p>`)}
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
							?checked=${this._model.constrain}></uui-toggle>
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
