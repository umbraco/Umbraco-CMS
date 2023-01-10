import { UUIInputElement, UUIInputEvent } from '@umbraco-ui/uui';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { distinctUntilChanged } from 'rxjs';
import { UmbWorkspaceDocumentTypeContext } from './document-type-workspace.context';
import type { DocumentTypeDetails } from '@umbraco-cms/models';
import { UmbModalService } from 'src/core/modal';
import { UmbLitElement } from '@umbraco-cms/element';
import type { UmbWorkspaceEntityElement } from 'src/backoffice/shared/components/workspace/workspace-entity-element.interface';

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

	private _entityKey!: string;
	@property()
	public get entityKey(): string {
		return this._entityKey;
	}
	public set entityKey(value: string) {
		this._entityKey = value;
		if (this._entityKey) {
			this._workspaceContext?.load(this._entityKey);
		}
	}

	@property()
	public set create(parentKey: string | null) {
		this._workspaceContext?.create(parentKey);
	}

	private _workspaceContext: UmbWorkspaceDocumentTypeContext = new UmbWorkspaceDocumentTypeContext(this);

	@state()
	private _documentType?: DocumentTypeDetails;

	private _modalService?: UmbModalService;

	constructor() {
		super();

		this.consumeContext('umbModalService', (instance) => {
			this._modalService = instance;
		});

		this.observe(this._workspaceContext.data.pipe(distinctUntilChanged()), (data) => {
			// TODO: make method to identify if data is of type DocumentTypeDetails
			this._documentType = (data as DocumentTypeDetails);
		});
	}

	// TODO. find a way where we don't have to do this for all workspaces.
	private _handleInput(event: UUIInputEvent) {
		if (event instanceof UUIInputEvent) {
			const target = event.composedPath()[0] as UUIInputElement;

			if (typeof target?.value === 'string') {
				this._workspaceContext?.update({ name: target.value });
			}
		}
	}

	private async _handleIconClick() {
		const modalHandler = this._modalService?.iconPicker();

		modalHandler?.onClose().then((saved) => {
			if (saved) this._workspaceContext?.update({ icon: saved.icon });
			console.log(saved);
			// TODO save color ALIAS as well
		});
	}

	render() {
		return html`
			<umb-workspace-layout alias="Umb.Workspace.DocumentType">
				<div id="header" slot="header">
					<uui-button id="icon" @click=${this._handleIconClick} compact>
						<uui-icon
							name="${this._documentType?.icon || 'umb:document-dashed-line'}"
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
