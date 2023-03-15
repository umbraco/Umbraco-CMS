import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, query, state } from 'lit/decorators.js';
import { repeat } from 'lit/directives/repeat.js';
import { UmbWorkspaceDocumentTypeContext } from '../../document-type-workspace.context';
import { UmbLitElement } from '@umbraco-cms/element';
import type { DocumentTypeModel } from '@umbraco-cms/backend-api';
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

	@state()
	_documentType?: DocumentTypeModel;

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
		return html`<uui-box headline="Templates">
			<umb-workspace-property-layout alias="Templates" label="Allowed Templates">
				<div slot="description">Choose which templates editors are allowed to use on content of this type</div>
				<div id="templates" slot="editor">
					<umb-template-card-list>
						<umb-template-card value="123"></umb-template-card>
						<umb-template-card value="456"></umb-template-card>
					</umb-template-card-list>
						
					</div>
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
