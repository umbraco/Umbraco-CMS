import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { css, html, customElement } from '@umbraco-cms/backoffice/external/lit';

@customElement('umb-document-type-workspace-editor')
export class UmbDocumentTypeWorkspaceEditorElement extends UmbLitElement {
	override render() {
		return html`
			<umb-entity-detail-workspace-editor>
				<umb-content-type-workspace-editor-header slot="header"></umb-content-type-workspace-editor-header>
			</umb-entity-detail-workspace-editor>
		`;
	}

	static override styles = [
		css`
			:host {
				display: block;
				width: 100%;
				height: 100%;
			}
		`,
	];
}

export default UmbDocumentTypeWorkspaceEditorElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-type-workspace-editor': UmbDocumentTypeWorkspaceEditorElement;
	}
}
