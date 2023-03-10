import { UUIInputElement, UUIInputEvent } from '@umbraco-ui/uui';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import type { UmbWorkspaceEntityElement } from '../../../shared/components/workspace/workspace-entity-element.interface';
import { UMB_ICON_PICKER_MODAL_TOKEN } from '../../../shared/modals/icon-picker';
import { UmbWorkspaceDocumentTypeContext } from './document-type-workspace.context';
import type { DocumentTypeModel } from '@umbraco-cms/backend-api';
import { UmbLitElement } from '@umbraco-cms/element';
import { UmbModalContext, UMB_MODAL_CONTEXT_TOKEN } from '@umbraco-cms/modal';

@customElement('umb-document-type-workspace')
export class UmbDocumentTypeWorkspaceElement extends UmbLitElement implements UmbWorkspaceEntityElement {
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

	private _icon = {
		color: '#000000',
		name: 'umb:document-dashed-line',
	};

	private _workspaceContext: UmbWorkspaceDocumentTypeContext = new UmbWorkspaceDocumentTypeContext(this);

	@state()
	private _documentType?: DocumentTypeModel;

	private _modalContext?: UmbModalContext;

	constructor() {
		super();

		this.consumeContext(UMB_MODAL_CONTEXT_TOKEN, (instance) => {
			this._modalContext = instance;
		});

		this.observe(this._workspaceContext.data, (data) => {
			// TODO: make method to identify if data is of type DocumentType
			this._documentType = data as DocumentType;
		});
	}

	public load(entityKey: string) {
		this._workspaceContext.load(entityKey);
	}

	public create(parentKey: string | null) {
		this._workspaceContext.createScaffold(parentKey);
	}

	// TODO. find a way where we don't have to do this for all workspaces.
	private _handleInput(event: UUIInputEvent) {
		if (event instanceof UUIInputEvent) {
			const target = event.composedPath()[0] as UUIInputElement;

			if (typeof target?.value === 'string') {
				this._workspaceContext?.setName(target.value);
			}
		}
	}

	private async _handleIconClick() {
		const modalHandler = this._modalContext?.open(UMB_ICON_PICKER_MODAL_TOKEN);

		modalHandler?.onSubmit().then((saved) => {
			if (saved.icon) this._workspaceContext?.setIcon(saved.icon);
			// TODO save color ALIAS as well
		});
	}

	render() {
		return html`
			<umb-workspace-layout alias="Umb.Workspace.DocumentType">
				<div id="header" slot="header">
					<uui-button id="icon" @click=${this._handleIconClick} compact>
						<uui-icon
							name="${this._documentType?.icon || this._icon.name}"
							style="color: ${this._icon.color}"></uui-icon>
					</uui-button>

					<uui-input id="name" .value=${this._documentType?.name} @input="${this._handleInput}">
						<div id="alias" slot="append">${this._documentType?.alias}</div>
					</uui-input>
				</div>

				<div slot="footer">Keyboard Shortcuts</div>
			</umb-workspace-layout>
		`;
	}
}

export default UmbDocumentTypeWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-type-workspace': UmbDocumentTypeWorkspaceElement;
	}
}
