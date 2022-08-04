import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property } from 'lit/decorators.js';

@customElement('umb-editor-layout')
class UmbEditorLayout extends LitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				width: 100%;
				height: 100%;
			}

			#editor-frame {
				background-color: var(--uui-color-background);
				width: 100%;
				height: 100%;
				display: flex;
				flex-direction: column;
			}

			#header {
				background-color: var(--uui-color-surface);
				width: 100%;
				display: flex;
				flex: none;
				gap: 16px;
				align-items: center;
				border-bottom: 1px solid var(--uui-color-border);
			}

			#main {
				padding: var(--uui-size-6);
				display: flex;
				flex: 1;
				flex-direction: column;
				gap: 16px;
			}

			#footer {
				display: flex;
				flex: none;
				justify-content: end;
				align-items: center;
				height: 70px;
				width: 100%;
				gap: 16px;
				padding-right: 24px;
				border-top: 1px solid var(--uui-color-border);
				background-color: var(--uui-color-surface);
				box-sizing: border-box;
			}
		`,
	];

	render() {
		return html`
			<div id="editor-frame">
				<div id="header">
					<slot name="name"></slot>
					<slot name="views"></slot>
				</div>
				<uui-scroll-container id="main">
					<slot></slot>
				</uui-scroll-container>
				<div id="footer">
					<!-- only show footer if slot has elements -->
					<slot name="actions"></slot>
				</div>
			</div>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-editor-layout': UmbEditorLayout;
	}
}
