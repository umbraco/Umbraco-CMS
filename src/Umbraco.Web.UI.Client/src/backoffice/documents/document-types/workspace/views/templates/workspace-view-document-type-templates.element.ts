import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { DocumentTypeResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbWorkspaceDocumentTypeContext } from '../../document-type-workspace.context';

import '../../../../../shared/property-creator/property-creator.element.ts';

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

	@property()
	defaultTemplateKey?: string = '123';

	@state()
	_documentType?: DocumentTypeResponseModel;

	@state()
	_templates = [
		{ key: '123', name: 'Blog Post Page' },
		{ key: '456', name: 'Blog Entry Page' },
	];

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

	#changeDefaultTemplate(e: CustomEvent) {
		//this.defaultTemplateKey = (e.target as UmbTemplateCardElement).value as string;
		console.log('default template key', this.defaultTemplateKey);
	}

	#removeTemplate(key: string) {
		console.log('remove template', key);
	}

	render() {
		return html`<uui-box headline="Templates">
			<umb-workspace-property-layout alias="Templates" label="Allowed Templates">
				<div slot="description">Choose which templates editors are allowed to use on content of this type</div>
				<div id="templates" slot="editor">
					<umb-input-template-picker umb-input-template-picker></umb-input-template-picker>
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
