import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, customElement, html, property, state, when, LitElement } from '@umbraco-cms/backoffice/external/lit';

//TODO consider adding a highlight prop to the code block, that could spin up/access monaco instance and highlight the code

/**
 *  A simple styled box for showing code-based error messages or blocks od code.
 *  @slot - the default slot where the full message resides
 */
@customElement('umb-code-block')
export class UmbCodeBlockElement extends LitElement {
	@property()
	language = '';

	@property({ type: Boolean })
	copy = false;

	@state()
	private _copyState: 'idle' | 'success' = 'idle';

	async copyCode() {
		const text = this.textContent;
		if (text) {
			await navigator.clipboard.writeText(text);
			this._copyState = 'success';
			setTimeout(() => {
				this._copyState = 'idle';
			}, 2000);
		}
	}

	override render() {
		return html`
			${this.#renderHeader()}
			<pre><uui-scroll-container><code><slot></slot></code></uui-scroll-container></pre>
		`; // Avoid breaks between elements of <pre></pre>
	}

	#renderHeader() {
		if (!this.language && !this.copy) return;
		return html`
			<div id="header">
				<span id="lang">${this.language}</span>
				${when(
					this.copy,
					() => html`
						<uui-button color=${this._copyState === 'idle' ? 'default' : 'positive'} @click=${this.copyCode}>
							${when(
								this._copyState === 'idle',
								() => html`<uui-icon name="copy"></uui-icon> <umb-localize key="general_copy">Copy</umb-localize>`,
								() =>
									html`<uui-icon name="check"></uui-icon> <umb-localize key="general_copied">Copied!</umb-localize>`,
							)}
						</uui-button>
					`,
				)}
			</div>
		`;
	}

	static override readonly styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				border: 1px solid var(--uui-color-divider-emphasis);
				border-radius: var(--uui-border-radius);
			}

			uui-scroll-container {
				overflow-y: auto;
				overflow-wrap: anywhere;
			}

			pre {
				font-family: monospace;
				background-color: var(--uui-color-surface-alt);
				color: #303033;
				display: block;
				margin: 0;
				overflow-x: auto;
				padding: 9.5px;
			}

			pre,
			code {
				word-wrap: normal;
				white-space: pre;
			}

			#header {
				display: flex;
				justify-content: space-between;
				align-items: center;
				background-color: var(--uui-color-surface-alt);
				border-bottom: 1px solid var(--uui-color-divider-emphasis);
			}

			#lang {
				margin-left: 16px;
				font-weight: bold;
			}

			button {
				font-family: inherit;
				padding: 6px 16px;
				background-color: transparent;
				border: none;
				border-left: 1px solid var(--uui-color-divider-emphasis);
				border-radius: 0;
				color: #000;
				display: flex;
				align-items: center;
				gap: 8px;
			}

			button:hover {
				background-color: var(--uui-color-surface-emphasis);
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-code-block': UmbCodeBlockElement;
	}
}
