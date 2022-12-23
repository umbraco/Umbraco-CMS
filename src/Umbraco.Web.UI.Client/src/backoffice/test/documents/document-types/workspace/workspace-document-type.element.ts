import { UUIInputElement, UUIInputEvent } from '@umbraco-ui/uui';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { distinctUntilChanged } from 'rxjs';
import { UmbWorkspaceDocumentTypeContext } from './workspace-document-type.context';
import { UmbObserverMixin } from '@umbraco-cms/observable-api';
import { UmbContextConsumerMixin, UmbContextProviderMixin } from '@umbraco-cms/context-api';
import type { DocumentTypeDetails, ManifestTypes } from '@umbraco-cms/models';
import { umbExtensionsRegistry } from '@umbraco-cms/extensions-registry';
import { UmbModalService } from '@umbraco-cms/services';

import '../../../../property-editors/uis/icon-picker/property-editor-ui-icon-picker.element';

@customElement('umb-workspace-document-type')
export class UmbWorkspaceDocumentTypeElement extends UmbContextProviderMixin(
	UmbContextConsumerMixin(UmbObserverMixin(LitElement))
) {
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
		this._provideWorkspace();
	}

	private _workspaceContext?: UmbWorkspaceDocumentTypeContext;

	@state()
	private _documentType?: DocumentTypeDetails;

	private _modalService?: UmbModalService;

	constructor() {
		super();

		this.consumeContext('umbModalService', (instance) => {
			this._modalService = instance;
		});

		this._registerExtensions();
	}

	connectedCallback(): void {
		super.connectedCallback();
		// TODO: avoid this connection, our own approach on Lit-Controller could be handling this case.
		this._workspaceContext?.connectedCallback();
	}
	disconnectedCallback(): void {
		super.connectedCallback();
		// TODO: avoid this connection, our own approach on Lit-Controller could be handling this case.
		this._workspaceContext?.disconnectedCallback();
	}

	protected _provideWorkspace() {
		if (this._entityKey) {
			this._workspaceContext = new UmbWorkspaceDocumentTypeContext(this, this._entityKey);
			this.provideContext('umbWorkspaceContext', this._workspaceContext);
			this._observeWorkspace();
		}
	}

	private async _observeWorkspace() {
		if (!this._workspaceContext) return;

		this.observe<DocumentTypeDetails>(this._workspaceContext.data.pipe(distinctUntilChanged()), (data) => {
			this._documentType = data;
		});
	}

	private _registerExtensions() {
		const extensions: Array<ManifestTypes> = [
			{
				type: 'workspaceView',
				alias: 'Umb.WorkspaceView.DocumentType.Design',
				name: 'Document Type Workspace Design View',
				loader: () => import('./views/design/workspace-view-document-type-design.element'),
				weight: 100,
				meta: {
					workspaces: ['Umb.Workspace.DocumentType'],
					label: 'Design',
					pathname: 'design',
					icon: 'edit',
				},
			},
			{
				type: 'workspaceAction',
				alias: 'Umb.WorkspaceAction.DocumentType.Save',
				name: 'Save Document Type Workspace Action',
				loader: () => import('../../../core/components/workspace/actions/save/workspace-action-node-save.element'),
				meta: {
					workspaces: ['Umb.Workspace.DocumentType'],
				},
			},
		];

		extensions.forEach((extension) => {
			if (umbExtensionsRegistry.isRegistered(extension.alias)) return;
			umbExtensionsRegistry.register(extension);
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
			<umb-workspace-entity alias="Umb.Workspace.DocumentType">
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
			</umb-workspace-entity>
		`;
	}
}

export default UmbWorkspaceDocumentTypeElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-document-type': UmbWorkspaceDocumentTypeElement;
	}
}
