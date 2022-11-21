import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';

/**
 *  A simple styled box for showing code-based error messages.
 *  max-height is 500px
 *
 *  @slot the message
 *
 */
@customElement('uui-error-box')
export class UUIErrorBox extends LitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				font-family: monospace;
			}

			#container {
				border: 2px solid var(--uui-color-divider-emphasis);
				color: var(--uui-color-text-alt);
				background-color: var(--uui-color-divider-standalone);
				padding: var(--uui-size-space-2);
				border-radius: var(--uui-border-radius);
				line-height: var(--uui-size-10);
			}
			:host uui-scroll-container {
				max-height: 500px;
			}

			pre {
				display: inline-block;
				overflow-wrap: break-word;
				word-wrap: break-word;
				word-break: break-all;
				line-break: strict;
				hyphens: none;
				-webkit-hyphens: none;
				-moz-hyphens: none;
			}
		`,
	];

	render() {
		return html`<div id="container">
			<uui-scroll-container>
				<pre><slot></slot></pre>
			</uui-scroll-container>
		</div> `;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'uui-error-box': UUIErrorBox;
	}
}
