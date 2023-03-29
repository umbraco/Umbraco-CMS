import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, state } from 'lit/decorators.js';
import { UmbWorkspaceDocumentTypeContext } from '../../document-type-workspace.context';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { DocumentTypeResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UMB_ENTITY_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/context-api';

@customElement('umb-document-type-workspace-view-structure')
export class UmbDocumentTypeWorkspaceViewStructureElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			uui-box {
				margin: var(--uui-size-layout-1);
			}

			uui-label,
			umb-property-editor-ui-number {
				display: block;
			}

			// TODO: is this necessary?
			uui-toggle {
				display: flex;
			}
		`,
	];

	private _workspaceContext?: UmbWorkspaceDocumentTypeContext;

	constructor() {
		super();

		// TODO: Figure out if this is the best way to consume the context or if it can be strongly typed with an UmbContextToken
		this.consumeContext(UMB_ENTITY_WORKSPACE_CONTEXT, (documentTypeContext) => {
			this._workspaceContext = documentTypeContext as UmbWorkspaceDocumentTypeContext;
			this._observeDocumentType();
		});
	}

	private _observeDocumentType() {
		if (!this._workspaceContext) return;
	}

	render() {
		return html`
			<uui-box headline="Structure">
				<umb-workspace-property-layout alias="Root" label="Allow as Root">
					<div slot="description">Allow editors to create content of this type in the root of the content tree.</div>
					<div slot="editor"><uui-toggle label="Allow as root"></uui-toggle></div>
				</umb-workspace-property-layout>
				<umb-workspace-property-layout alias="ChildNodeType" label="Allowed child node types">
					<div slot="description">
						Allow content of the specified types to be created underneath content of this type.
					</div>
					<div slot="editor">
						<!-- TODO: maybe we want to somehow display the hierarchy, but not necessary in the same way as old backoffice? -->
						<umb-input-document-type-picker></umb-input-document-type-picker>
					</div>
				</umb-workspace-property-layout>
			</uui-box>
			<uui-box headline="Presentation">
				<umb-workspace-property-layout alias="Root" label="Collection">
					<div slot="description">
						Use this document as a collection, displaying its children in a Collection View. This could be a list or a
						table.
					</div>
					<div slot="editor"><uui-toggle label="Present as a Collection"></uui-toggle></div>
				</umb-workspace-property-layout>
			</uui-box>
		`;
	}
}

export default UmbDocumentTypeWorkspaceViewStructureElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-type-workspace-view-structure': UmbDocumentTypeWorkspaceViewStructureElement;
	}
}
