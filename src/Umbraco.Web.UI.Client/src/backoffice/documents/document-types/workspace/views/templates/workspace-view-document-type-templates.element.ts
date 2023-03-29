import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, state } from 'lit/decorators.js';
import { UmbWorkspaceDocumentTypeContext } from '../../document-type-workspace.context';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { DocumentTypeResponseModel } from '@umbraco-cms/backoffice/backend-api';

@customElement('umb-workspace-view-document-type-templates')
export class UmbWorkspaceViewDocumentTypeTemplatesElement extends UmbLitElement {
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

	@state()
	_documentType?: DocumentTypeResponseModel;

	private _workspaceContext?: UmbWorkspaceDocumentTypeContext;

	constructor() {
		super();
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
						.defaultKey="${this._documentType?.defaultTemplateKey ?? ''}"
						.allowedKeys="${this._documentType?.allowedTemplateKeys ?? []}"
						@change-default="${this.#changeDefaultKey}"
						@change-allowed="${this.#changeAllowedKeys}"></umb-input-template-picker>
				</div>
			</umb-workspace-property-layout>
		</uui-box>`;
	}
}

export default UmbWorkspaceViewDocumentTypeTemplatesElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-view-document-type-templates': UmbWorkspaceViewDocumentTypeTemplatesElement;
	}
}
