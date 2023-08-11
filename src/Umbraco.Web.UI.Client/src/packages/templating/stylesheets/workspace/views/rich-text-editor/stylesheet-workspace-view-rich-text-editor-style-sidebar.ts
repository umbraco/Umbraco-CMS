import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { css, html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/internal/modal';
import { RichTextRuleModel } from '@umbraco-cms/backoffice/backend-api';

export interface StylesheetRichTextEditorStyleModalData {
	rule?: RichTextRuleModel;
}

export interface StylesheetRichTextEditorStyleModalResult {
	rule: RichTextRuleModel;
}

@customElement('umb-stylesheet-rich-text-editor-style-modal')
export default class UmbStylesheetRichTextEditorStyleModalElement extends UmbModalBaseElement<
	StylesheetRichTextEditorStyleModalData,
	StylesheetRichTextEditorStyleModalResult
> {
	private _close() {
		this.modalContext?.reject();
	}

	render() {
		return html`
			<umb-body-layout headline="Insert">
				<div id="main">
					<uui-box>
						<h3>Rule</h3>
						<p>${this.data?.rule?.name}</p>
						<p>${this.data?.rule?.selector}</p>
						<p>${this.data?.rule?.styles}</p>
					</uui-box>
				</div>
				<div slot="actions">
					<uui-button @click=${this._close} look="secondary" label="Close">Close</uui-button>
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

			#main uui-button:not(:last-of-type) {
				display: block;
				margin-bottom: var(--uui-size-space-5);
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
		'umb-stylesheet-rich-text-editor-style-modal': UmbStylesheetRichTextEditorStyleModalElement;
	}
}
