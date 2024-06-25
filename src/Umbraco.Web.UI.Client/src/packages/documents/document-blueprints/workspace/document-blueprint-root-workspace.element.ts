import { UMB_DOCUMENT_BLUEPRINT_WORKSPACE_ALIAS } from './manifests.js';
import { html, customElement, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-document-blueprint-root-workspace')
export class UmbDocumentBlueprintRootWorkspaceElement extends UmbLitElement {
	override render() {
		return html`<umb-workspace-editor alias=${UMB_DOCUMENT_BLUEPRINT_WORKSPACE_ALIAS} headline="Document Blueprints">
			<div id="wrapper">
				<uui-box>
					<h2>
						<umb-localize key="contentTemplatesDashboard_whatHeadline"> What are Content Blueprints? </umb-localize>
					</h2>
					<umb-localize key="contentTemplatesDashboard_whatDescription">
						Content Blueprints are pre-defined content that can be selected when creating a new content node.
					</umb-localize>
					<h2>
						<umb-localize key="contentTemplatesDashboard_createHeadline">
							How do I create a Content Blueprint?
						</umb-localize>
					</h2>
					<umb-localize key="contentTemplatesDashboard_createDescription">
						<p>There are two ways to create a Content Blueprint:</p>
						<ul>
							<li>
								Right-click a content node and select "Create Content Blueprint" to create a new Content Blueprint.
							</li>
							<li>
								Right-click the Content Blueprints tree in the Settings section and select the Document Type you want to
								create a Content Blueprint for.
							</li>
						</ul>
						<p>Once given a name, editors can start using the Content Blueprint as a foundation for their new page.</p>
					</umb-localize>
					<h2>
						<umb-localize key="contentTemplatesDashboard_manageHeadline">
							How do I manage Content Blueprints?
						</umb-localize>
					</h2>
					<umb-localize key="contentTemplatesDashboard_manageDescription">
						You can edit and delete Content Blueprints from the "Content Blueprints" tree in the Settings section.
						Expand the Document Type which the Content Blueprint is based on and click it to edit or delete it.
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
