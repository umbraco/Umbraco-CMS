import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement } from 'lit/decorators.js';

@customElement('umb-body-layout')
export class UmbBodyLayout extends LitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: flex;
				background-color: var(--uui-color-background);
				width: 100%;
				height: 100%;
				flex-direction: column;
			}

			#header {
				background-color: var(--uui-color-surface);
				width: 100%;
				border-bottom: 1px solid var(--uui-color-border);
				box-sizing: border-box;
				/* padding: 0 var(--uui-size-6); */
			}

			#main {
				/* padding: 0 var(--uui-size-6); */
				display: flex;
				flex: 1;
				flex-direction: column;
			}

			#footer {
				display: flex;
				align-items: center;
				height: 70px;
				width: 100%;
				/*padding: 0 var(--uui-size-6);*/
				border-top: 1px solid var(--uui-color-border);
				background-color: var(--uui-color-surface);
				box-sizing: border-box;
			}
		`,
	];

	render() {
		return html`
			<div id="header">
				<slot name="header"></slot>
			</div>
			<uui-scroll-container id="main">
				<slot></slot>
			</uui-scroll-container>
			<div id="footer">
				<!-- only show footer if slot has elements -->
				<slot name="footer"></slot>
			</div>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-body-layout': UmbBodyLayout;
	}
}
