import { css, customElement, html, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { clamp } from '@umbraco-cms/backoffice/external/uui';

/**
 * @element umb-file-feedback-loader
 * @description A element which renders the progress when uploading a file.
 * @slot default - slot for inserting additional things into this slot.
 * @export
 * @class UmbExtensionSlot
 * @extends {UmbLitElement}
 */

//TODO: Give a better name... umb-file-progress?

@customElement('umb-file-feedback-loader')
export class UmbFileFeedbackLoaderElement extends UmbLitElement {
	private _progress = 0;
	@property({ type: Number })
	public set progress(v: number) {
		const oldVal = this._progress;
		const p = clamp(v, 0, 100);
		this._progress = p;
		if (p === 100) {
			this.dispatchEvent(new CustomEvent('complete', { bubbles: true }));
		}
		this.requestUpdate('progress', oldVal);
	}
	public get progress(): number {
		return this._progress;
	}

	render() {
		return html`<uui-badge>
			<div id="wrapper">
				<uui-loader-circle progress=${this.progress}></uui-loader-circle>
				<uui-icon name="icon-arrow-up"></uui-icon>
			</div>
		</uui-badge>`;
	}

	static styles = css`
		:host {
			display: block;
		}

		#wrapper {
			box-sizing: border-box;
			box-shadow: inset 0px 0px 0px 6px var(--uui-color-surface);
			background-color: var(--uui-color-selected);
			position: relative;
			border-radius: 100%;
			font-size: var(--uui-size-6);
		}

		uui-loader-circle {
			display: absolute;
			inset: 0;
			color: var(--uui-color-focus);
			font-size: var(--uui-size-12);
		}

		uui-badge {
			padding: 0;
			background-color: transparent;
		}

		uui-icon {
			font-size: var(--uui-size-6);
			position: absolute;
			top: 50%;
			left: 50%;
			transform: translate(-50%, -50%);
		}
	`;
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-file-feedback-loader': UmbFileFeedbackLoaderElement;
	}
}
