import { css, customElement, html, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { clamp } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-temporary-file-badge')
export class UmbTemporaryFileBadgeElement extends UmbLitElement {
	private _progress = 0;

	@property({ type: Number })
	public set progress(v: number) {
		const oldVal = this._progress;

		const p = clamp(v, 0, 100);
		this._progress = p;

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
		'umb-temporary-file-badge': UmbTemporaryFileBadgeElement;
	}
}

export default UmbTemporaryFileBadgeElement;
