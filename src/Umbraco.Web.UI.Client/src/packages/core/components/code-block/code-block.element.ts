import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';

/**
 *  A simple styled box for showing code-based error messages.
 *  @slot the full message
 *
 */
@customElement('umb-code-block')
export class UmbCodeBlockElement extends LitElement {
	render() {
		return html`<div id="container">
			<uui-scroll-container>
				<pre>
					<code>
						<slot></slot>
					</code>
				</pre>
			</uui-scroll-container>
		</div> `;
	}

	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				font-family: monospace;
			}

			#container {
				border: 1px solid var(--uui-color-divider-emphasis);
				color: var(--uui-color-text-alt);
				background-color: var(--uui-color-divider-standalone);
				padding: var(--uui-size-space-2);
				border-radius: var(--uui-border-radius);
				line-height: var(--uui-size-10);
			}
			:host uui-scroll-container {
				max-height: 500px;
				overflow-y: auto;
				overflow-wrap: anywhere;
			}
			pre {
				max-width: 100%;
				white-space: pre-line;
				word-break: break-word;
				overflow-wrap: break-word;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-code-block': UmbCodeBlockElement;
	}
}
