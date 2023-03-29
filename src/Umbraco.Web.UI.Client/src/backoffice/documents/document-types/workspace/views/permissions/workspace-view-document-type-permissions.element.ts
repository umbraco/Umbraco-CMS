import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, state } from 'lit/decorators.js';
import { UmbWorkspaceDocumentTypeContext } from '../../document-type-workspace.context';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { DocumentTypeResponseModel } from '@umbraco-cms/backoffice/backend-api';

@customElement('umb-workspace-view-document-type-permissions')
export class UmbWorkspaceViewDocumentTypePermissionsElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				margin: var(--uui-size-layout-1);
			}
			uui-label,
			umb-property-editor-ui-number {
				display: block;
			}

			uui-toggle {
				display: flex;
			}
		`,
	];

	@state()
	_documentType?: DocumentTypeResponseModel;

	private _workspaceContext?: UmbWorkspaceDocumentTypeContext;

	constructor() {
		super();

		// TODO: Figure out if this is the best way to consume the context or if it can be strongly typed with an UmbContextToken
		this.consumeContext<UmbWorkspaceDocumentTypeContext>('umbWorkspaceContext', (documentTypeContext) => {
			this._workspaceContext = documentTypeContext;
			this._observeDocumentType();
		});
	}

	private _observeDocumentType() {
		if (!this._workspaceContext) return;

		this.observe(this._workspaceContext.data, (documentType) => {
			this._documentType = documentType;
		});
	}

	render() {
		return html`
			<uui-box headline="Permissions">
				<umb-workspace-property-layout alias="Root" label="Allow as Root">
					<div slot="description">Allow editors to create content of this type in the root of the content tree.</div>
					<div slot="editor"><uui-toggle label="Allow as root"></uui-toggle></div>
				</umb-workspace-property-layout>
				<umb-workspace-property-layout alias="ChildNodeType" label="Allowed child node types">
					<div slot="description">
						Allow content of the specified types to be created underneath content of this type.
					</div>
					<div slot="editor">
						<umb-input-document-type-picker
							.currentDocumentType="${this._documentType}"></umb-input-document-type-picker>
					</div>
				</umb-workspace-property-layout>
				<umb-workspace-property-layout alias="VaryByNature" label="Allow vary by culture">
					<div slot="description">Allow editors to create content of different languages.</div>
					<div slot="editor"><uui-toggle label="Vary by culture"></uui-toggle></div>
				</umb-workspace-property-layout>
				<umb-workspace-property-layout alias="ElementType" label="Is an Element Type">
					<div slot="description">
						An Element Type is meant to be used for instance in Nested Content, and not in the tree.<br />
						A Document Type cannot be changed to an Element Type once it has been used to create one or more content
						items.
					</div>
					<div slot="editor"><uui-toggle label="Element type"></uui-toggle></div>
				</umb-workspace-property-layout>
				<umb-workspace-property-layout alias="HistoryCleanup" label="History cleanup">
					<div slot="description">Allow overriding the global history cleanup settings.</div>
					<div slot="editor">
						<uui-toggle .checked="${true}" label="Auto cleanup"></uui-toggle>
						<uui-label for="versions-newer-than-days">Keep all versions newer than days</uui-label>
						<umb-property-editor-ui-number id="versions-newer-than-days"></umb-property-editor-ui-number>
						<uui-label for="latest-version-per-day-days">Keep latest version per day for days</uui-label>
						<umb-property-editor-ui-number id="latest-version-per-day-days"></umb-property-editor-ui-number>
					</div>
				</umb-workspace-property-layout>
			</uui-box>
		`;
	}
}

export default UmbWorkspaceViewDocumentTypePermissionsElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-view-document-type-permissions': UmbWorkspaceViewDocumentTypePermissionsElement;
	}
}
