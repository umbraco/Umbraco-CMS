import { UUIInputElement, UUIInputEvent } from '@umbraco-ui/uui';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { UmbObserverMixin } from '@umbraco-cms/observable-api';
import { UmbContextConsumerMixin, UmbContextProviderMixin } from '@umbraco-cms/context-api';
import type { DocumentTypeDetails, ManifestTypes } from '@umbraco-cms/models';
import { umbExtensionsRegistry } from '@umbraco-cms/extensions-registry';

import '../../property-editor-uis/icon-picker/property-editor-ui-icon-picker.element';
import { UmbWorkspaceDocumentTypeContext } from './workspace-document-type.context';
import { distinctUntilChanged } from 'rxjs';

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
		`,
	];

	private _entityKey!: string;
	@property()
	public get entityKey(): string {
		return this._entityKey;
	}
	public set entityKey(value: string) {
		this._entityKey = value;
		this._provideWorkspace();
	}

	private _entityType = '';
	@property()
	public get entityType(): string {
		return this._entityType;
	}
	public set entityType(value: string) {
		// TODO: Make sure that a change of the entity type actually gives extension slot a hint to change/update.
		const oldValue = this._entityType;
		this._entityType = value;
		this._provideWorkspace();
		this.requestUpdate('entityType', oldValue);
	}

	private _workspaceContext?:UmbWorkspaceDocumentTypeContext;

	@state()
	private _documentType?:DocumentTypeDetails;

	constructor() {
		super();

		this._registerExtensions();
	}

	connectedCallback(): void {
		super.connectedCallback();
		// TODO: avoid this connection, our own approach on Lit-Controller could be handling this case.
		this._workspaceContext?.connectedCallback();
	}
	disconnectedCallback(): void {
		super.connectedCallback()
		// TODO: avoid this connection, our own approach on Lit-Controller could be handling this case.
		this._workspaceContext?.disconnectedCallback();
	}

	protected _provideWorkspace() {
		if(this._entityType && this._entityKey) {
			this._workspaceContext = new UmbWorkspaceDocumentTypeContext(this, this._entityType, this._entityKey);
			this.provideContext('umbWorkspaceContext', this._workspaceContext);
			this._observeWorkspace()
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
				loader: () => import('./actions/save/workspace-action-document-type-save.element'),
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

	render() {
		return html`
			<umb-workspace-entity alias="Umb.Workspace.DocumentType">
				<div id="header" slot="header">
					<umb-property-editor-ui-icon-picker></umb-property-editor-ui-icon-picker>
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
