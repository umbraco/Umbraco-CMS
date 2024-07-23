import { UMB_DOCUMENT_BLUEPRINT_WORKSPACE_ALIAS } from './manifests.js';
import { html, customElement, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-document-blueprint-root-workspace')
export class UmbDocumentBlueprintRootWorkspaceElement extends UmbLitElement {
	override render() {
		return html`<umb-workspace-editor
			alias=${UMB_DOCUMENT_BLUEPRINT_WORKSPACE_ALIAS}
			headline=${this.localize.term('treeHeaders_contentBlueprints')}>
			<div id="wrapper">
				<uui-box>
					<h2>
						<umb-localize key="contentTemplatesDashboard_whatHeadline"> What are Document Blueprints? </umb-localize>
					</h2>
					<umb-localize key="contentTemplatesDashboard_whatDescription">
						Document Blueprints are pre-defined content that can be selected when creating a new content node.
					</umb-localize>
					<h2>
						<umb-localize key="contentTemplatesDashboard_createHeadline">
							How do I create a Document Blueprint?
						</umb-localize>
					</h2>
					<umb-localize key="contentTemplatesDashboard_createDescription">
						<p>There are two ways to create a Content Blueprint:</p>
						<ul>
							<li>
								Click "Create Document Blueprint" in the action menu on a content node to create a new Document
								Blueprint.
							</li>
							<li>
								Click the "+" button in the Document Blueprints tree in the Settings section and select the Document
								Type you want to create a Document Blueprint for.
							</li>
						</ul>
						<p>Once given a name, editors can start using the Document Blueprint as a foundation for their new page.</p>
					</umb-localize>
					<h2>
						<umb-localize key="contentTemplatesDashboard_manageHeadline">
							How do I manage Document Blueprints?
						</umb-localize>
					</h2>
					<umb-localize key="contentTemplatesDashboard_manageDescription">
						You can edit and delete Document Blueprints from the "Document Blueprints" tree in the Settings section.
						Expand the Document Type which the Document Blueprint is based on and click it to edit or delete it.
					</umb-localize>
				</uui-box>
			</div>
		</umb-workspace-editor> `;
	}

	static override styles = [
		css`
			#wrapper {
				margin: var(--uui-size-layout-1);
			}
		`,
	];
}

export default UmbDocumentBlueprintRootWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-blueprint-root-workspace': UmbDocumentBlueprintRootWorkspaceElement;
	}
}
