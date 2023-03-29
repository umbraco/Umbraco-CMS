import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, state } from 'lit/decorators.js';
import { UmbWorkspaceDocumentTypeContext } from '../../document-type-workspace.context';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { DocumentTypeResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UMB_ENTITY_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/context-api';

@customElement('umb-document-type-workspace-view-details')
export class UmbDocumentTypeWorkspaceViewDetailsElement extends UmbLitElement {
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
			<uui-box headline="Data configuration">
				<umb-workspace-property-layout alias="VaryByCulture" label="Allow vary by culture">
					<div slot="description">Allow editors to create content of different languages.</div>
					<div slot="editor"><uui-toggle label="Vary by culture"></uui-toggle></div>
				</umb-workspace-property-layout>
				<umb-workspace-property-layout alias="VaryBySegments" label="Allow segmentation">
					<div slot="description">Allow editors to segment their content.</div>
					<div slot="editor"><uui-toggle label="Vary by segments"></uui-toggle></div>
				</umb-workspace-property-layout>
				<umb-workspace-property-layout alias="ElementType" label="Is an Element Type">
					<div slot="description">
						An Element Type is used for content instances in Property Editors, like the Block Editors.
					</div>
					<div slot="editor"><uui-toggle label="Element type"></uui-toggle></div>
				</umb-workspace-property-layout>
			</uui-box>
			<uui-box headline="History cleanup">
				<umb-workspace-property-layout alias="HistoryCleanup" label="History cleanup">
					<div slot="description">
						Allow overriding the global history cleanup settings. (TODO: this ui is not working.. )
					</div>
					<div slot="editor">
						<uui-toggle .checked="${true}" label="Auto cleanup"></uui-toggle>
						<uui-label for="versions-newer-than-days">Keep all versions newer than X days</uui-label>
						<umb-property-editor-ui-number id="versions-newer-than-days"></umb-property-editor-ui-number>
						<uui-label for="latest-version-per-day-days">Keep latest version per day for X days</uui-label>
						<umb-property-editor-ui-number id="latest-version-per-day-days"></umb-property-editor-ui-number>
					</div>
				</umb-workspace-property-layout>
			</uui-box>
		`;
	}
}

export default UmbDocumentTypeWorkspaceViewDetailsElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-type-workspace-view-details': UmbDocumentTypeWorkspaceViewDetailsElement;
	}
}
