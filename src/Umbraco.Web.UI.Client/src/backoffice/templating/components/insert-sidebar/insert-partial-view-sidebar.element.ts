import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbModalBaseElement } from '@umbraco-cms/internal/modal';

@customElement('umb-insert-partial-view-sidebar')
export default class UmbInsertPartialViewSidebarElement extends UmbModalBaseElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				color: var(--uui-color-text);
			}

			#main {
				box-sizing: border-box;
				padding: var(--uui-size-space-5);
				height: calc(100vh - 124px);
			}

			#main uui-button {
				width: 100%;
			}

			h3,
			p {
				text-align: left;
			}

			uui-combobox,
			uui-input {
				width: 100%;
			}
		`,
	];

	private _close() {
		this.modalHandler?.submit();
	}

	render() {
		return html`
			<umb-workspace-layout headline="Insert Partial view">
				<div id="main">
					<uui-box> <umb-tree alias="Umb.Tree.PartialViews"></umb-tree> </uui-box>
				</div>
				<div slot="actions">
					<uui-button @click=${this._close} look="secondary">Close</uui-button>
				</div>
			</umb-workspace-layout>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-insert-partial-view-sidebar': UmbInsertPartialViewSidebarElement;
	}
}
