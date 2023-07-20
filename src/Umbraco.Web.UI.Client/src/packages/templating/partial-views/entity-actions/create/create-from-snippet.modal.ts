import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbPartialViewPickerModalData, UmbPartialViewPickerModalResult } from '@umbraco-cms/backoffice/modal';
import { UmbModalBaseElement } from '@umbraco-cms/internal/modal';
import { UmbTreeElement } from '@umbraco-cms/backoffice/tree';
import { UmbCreateFromSnippetPartialViewModalData } from './create-from-snippet.action.js';
import { SnippetItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

@customElement('umb-partial-view-create-from-snippet-modal')
export default class UmbPartialViewCreateFromSnippetModalElement extends UmbModalBaseElement<
	UmbCreateFromSnippetPartialViewModalData,
	string
> {
	@state()
	_snippets: Array<SnippetItemResponseModel> = [];

	connectedCallback() {
		super.connectedCallback();
		this._snippets = this.data?.snippets ?? [];
	}

	private _submit(snippetName: string) {
		this.modalContext?.submit(snippetName);
	}

	private _close() {
		this.modalContext?.reject();
	}

	render() {
		return html`
			<umb-body-layout headline="Create partial view from snippet">
				<div id="main">
					<uui-box>
						${this._snippets.map(
							(snippet) =>
								html`<uui-menu-item label="${snippet.name ?? ''}" @click-label=${() => this._submit(snippet.name ?? '')}
									><uui-icon name="umb:article" slot="icon"></uui-icon
								></uui-menu-item>`
						)}
					</uui-box>
				</div>
				<div slot="actions">
					<uui-button @click=${this._close} look="secondary">Close</uui-button>
				</div>
			</umb-body-layout>
		`;
	}

	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				color: var(--uui-color-text);
				--umb-header-layout-height: 70px;
			}
			#main {
				box-sizing: border-box;
				height: calc(
					100dvh - var(--umb-header-layout-height) - var(--umb-footer-layout-height) - 2 * var(--uui-size-layout-1)
				);
			}

			#main uui-button {
				width: 100%;
			}

			h3,
			p {
				text-align: left;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-partial-view-create-from-snippet-modal': UmbPartialViewCreateFromSnippetModalElement;
	}
}
