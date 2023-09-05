import { UmbTextStyles } from "@umbraco-cms/backoffice/style";
import {
	css,
	html,
	LitElement,
	customElement,
	property,
	queryAssignedNodes,
	state,
} from '@umbraco-cms/backoffice/external/lit';

//TODO consider adding a highlight prop to the code block, that could spin up/access monaco instance and highlight the code

/**
 *  A simple styled box for showing code-based error messages or blocks od code.
 *  @slot the full message
 *
 */
@customElement('umb-code-block')
export class UmbCodeBlockElement extends LitElement {
	@property()
	language = '';

	@property({ type: Boolean })
	copy = false;

	@queryAssignedNodes()
	nodes!: NodeListOf<ChildNode>;

	@state()
	copyState: 'idle' | 'success' = 'idle';

	async copyCode() {
		const text = this.textContent;
		if (text) {
			await navigator.clipboard.writeText(text);
			this.copyState = 'success';
			setTimeout(() => {
				this.copyState = 'idle';
			}, 2000);
		}
	}

	render() {
		return html`
			${this.language
				? html`<div id="header">
						<span id="lang">${this.language}</span> ${this.copy
							? html`<button @click=${this.copyCode}>
									${this.copyState === 'idle'
										? html`<uui-icon name="copy"></uui-icon>Copy`
										: html`<uui-icon name="check"></uui-icon>Copied!`}
							  </button>`
							: ''}
				  </div>`
				: ''}
			<pre style="${this.language ? 'border-top: 1px solid var(--uui-color-divider-emphasis);' : ''}">
				<uui-scroll-container>
					<code>
						<slot></slot>
					</code>
					</uui-scroll-container>
				</pre>
		`;
	}

	static styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				border: 1px solid var(--uui-color-divider-emphasis);
				border-radius: var(--uui-border-radius);
			}

			uui-scroll-container {
				max-height: 500px;
				overflow-y: auto;
				overflow-wrap: anywhere;
			}

			pre {
				font-family: monospace;
				background-color: var(--uui-color-surface-alt);
				color: #303033;
				display: block;
				font-family: Lato, Helvetica Neue, Helvetica, Arial, sans-serif;
				margin: 0;
				overflow-x: auto;
				padding: 9.5px;
			}

			pre,
			code {
				word-wrap: normal;
				white-space: pre-line;
			}

			#header {
				display: flex;
				justify-content: space-between;
				align-items: center;
				background-color: var(--uui-color-surface-alt);
			}

			#lang {
				margin-left: 16px;
				font-weight: bold;
				text-transform: uppercase;
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
