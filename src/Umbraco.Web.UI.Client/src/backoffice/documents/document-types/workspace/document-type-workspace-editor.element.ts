import { UUIInputElement, UUIInputEvent } from '@umbraco-ui/uui';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbWorkspaceDocumentTypeContext } from './document-type-workspace.context';
import type { DocumentTypeResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbModalContext, UMB_MODAL_CONTEXT_TOKEN, UMB_ICON_PICKER_MODAL } from '@umbraco-cms/backoffice/modal';
import { UMB_ENTITY_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/context-api';

@customElement('umb-document-type-workspace-editor')
export class UmbDocumentTypeWorkspaceEditorElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				width: 100%;
				height: 100%;
			}

			#header {
				display: flex;
				flex: 1 1 auto;
				margin: 0 var(--uui-size-layout-1);
			}

			#name {
				width: 100%;
				flex: 1 1 auto;
				align-items: center;
			}

			#alias {
				padding: 0 var(--uui-size-space-3);
			}

			#icon {
				font-size: calc(var(--uui-size-layout-3) / 2);
			}
		`,
	];

	// TODO: notice this format is not acceptable:
	@state()
	private _icon = {
		color: '#000000',
		name: 'umb:document-dashed-line',
	};

	#workspaceContext?: UmbWorkspaceDocumentTypeContext;

	//@state()
	//private _documentType?: DocumentTypeResponseModel;
	@state()
	private _name?: string;

	@state()
	private _alias?: string;

	private _modalContext?: UmbModalContext;

	constructor() {
		super();

		this.consumeContext(UMB_ENTITY_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance as UmbWorkspaceDocumentTypeContext;
			this.#observeDocumentType();
		});

		this.consumeContext(UMB_MODAL_CONTEXT_TOKEN, (instance) => {
			this._modalContext = instance;
		});
	}

	#observeDocumentType() {
		if (!this.#workspaceContext) return;
		//this.observe(this.#workspaceContext.data, (data) => (this._documentType = data));
	}

	// TODO. find a way where we don't have to do this for all workspaces.
	private _handleInput(event: UUIInputEvent) {
		if (event instanceof UUIInputEvent) {
			const target = event.composedPath()[0] as UUIInputElement;

			if (typeof target?.value === 'string') {
				this.#workspaceContext?.setName(target.value);
			}
		}
	}

	private async _handleIconClick() {
		const modalHandler = this._modalContext?.open(UMB_ICON_PICKER_MODAL);

		modalHandler?.onSubmit().then((saved) => {
			if (saved.icon) this.#workspaceContext?.setIcon(saved.icon);
			// TODO save color ALIAS as well
		});
	}

	render() {
		return html`
			<umb-workspace-layout alias="Umb.Workspace.DocumentType">
				<div id="header" slot="header">
					<uui-button id="icon" @click=${this._handleIconClick} compact>
						<uui-icon name="${this._icon.name}" style="color: ${this._icon.color}"></uui-icon>
					</uui-button>

					<uui-input id="name" .value=${this._name} @input="${this._handleInput}">
						<div id="alias" slot="append">${this._alias}</div>
					</uui-input>
				</div>

				<div slot="footer">
					<uui-button label="Show keyboard shortcuts">
						Keyboard Shortcuts
						<uui-keyboard-shortcut>
							<uui-key>ALT</uui-key>
							+
							<uui-key>shift</uui-key>
							+
							<uui-key>k</uui-key>
						</uui-keyboard-shortcut>
					</uui-button>
				</div>
			</umb-workspace-layout>
		`;
	}
}

export default UmbDocumentTypeWorkspaceEditorElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-type-workspace-editor': UmbDocumentTypeWorkspaceEditorElement;
	}
}
