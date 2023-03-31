import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement } from 'lit/decorators.js';
import { UmbWorkspaceDocumentTypeContext } from '../../document-type-workspace.context';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UMB_ENTITY_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/context-api';

@customElement('umb-document-type-workspace-view-templates')
export class UmbDocumentTypeWorkspaceViewTemplatesElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				margin: var(--uui-size-layout-1);
			}

			#templates {
				text-align: center;
			}

			#template-card-wrapper {
				display: flex;
				gap: var(--uui-size-space-4);
				align-items: stretch;
			}

			umb-workspace-property-layout {
				border-top: 1px solid var(--uui-color-border);
			}
			umb-workspace-property-layout:first-child {
				padding-top: 0;
				border: none;
			}
		`,
	];

	private _workspaceContext?: UmbWorkspaceDocumentTypeContext;

	constructor() {
		super();
		this.consumeContext(UMB_ENTITY_WORKSPACE_CONTEXT, (documentTypeContext) => {
			this._workspaceContext = documentTypeContext as UmbWorkspaceDocumentTypeContext;
			this._observeDocumentType();
		});
	}

	private _observeDocumentType() {
		if (!this._workspaceContext) return;
	}

	async #changeDefaultKey(e: CustomEvent) {
		// save new default key
		console.log('workspace: default template key', e);
	}

	#changeAllowedKeys(e: CustomEvent) {
		// save new allowed keys
		console.log('workspace: allowed templates changed', e);
	}

	render() {
		return html`<uui-box headline="Templates">
			<umb-workspace-property-layout alias="Templates" label="Allowed Templates">
				<div slot="description">Choose which templates editors are allowed to use on content of this type</div>
				<div id="templates" slot="editor">
					<umb-input-template-picker
						.defaultKey="${/*this._documentType?.defaultTemplateKey ??*/ ''}"
						.allowedKeys="${/*this._documentType?.allowedTemplateKeys ??*/ []}"
						@change-default="${this.#changeDefaultKey}"
						@change-allowed="${this.#changeAllowedKeys}"></umb-input-template-picker>
				</div>
			</umb-workspace-property-layout>
		</uui-box>`;
	}
}

export default UmbDocumentTypeWorkspaceViewTemplatesElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-type-workspace-view-templates': UmbDocumentTypeWorkspaceViewTemplatesElement;
	}
}
