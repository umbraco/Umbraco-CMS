import { css, customElement, html, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { clamp } from '@umbraco-cms/backoffice/utils';

@customElement('umb-temporary-file-badge')
export class UmbTemporaryFileBadgeElement extends UmbLitElement {
	#progress = 0;

	@property({ type: Number })
	public set progress(v: number) {
		const p = clamp(Math.ceil(v), 0, 100);
		this.#progress = p;
	}
	public get progress(): number {
		return this.#progress;
	}

	@property({ type: Boolean, reflect: true })
	public complete = false;

	@property({ type: Boolean, reflect: true })
	public error = false;

	override render() {
		return html` <div id="wrapper">
			<uui-loader-circle .progress=${this.complete || this.error ? 100 : this.progress}></uui-loader-circle>
			<div id="icon">${this.#renderIcon()}</div>
		</div>`;
	}

	#renderIcon() {
		if (this.error) {
			return html`<uui-icon name="icon-alert"></uui-icon>`;
		}

		if (this.complete) {
			return html`<uui-icon name="icon-check"></uui-icon>`;
		}

		return `${this.progress}%`;
	}

	static override readonly styles = css`
		#wrapper {
			position: relative;
			height: 75%;
		}

		:host([complete]) {
			uui-loader-circle,
			#icon {
				color: var(--uui-color-positive);
			}
		}
		:host([error]) {
			uui-loader-circle,
			#icon {
				color: var(--uui-color-danger);
			}
		}

		uui-loader-circle {
			z-index: 2;
			inset: 0;
			color: var(--uui-color-focus);
			font-size: var(--uui-size-12);
			width: 100%;
			height: 100%;
		}

		#icon {
			color: var(--uui-color-text);
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
