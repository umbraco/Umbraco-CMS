import { html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

@customElement('umb-document-blueprint-root-workspace')
export class UmbDocumentBlueprintRootWorkspaceElement extends UmbLitElement {
	override render() {
		return html`<umb-body-layout .headline=${this.localize.term('treeHeaders_contentBlueprints')}>
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
							Click "Create Document Blueprint" in the action menu on a content node to create a new Document Blueprint.
						</li>
						<li>
							Click the "+" button in the Document Blueprints tree in the Settings section and select the Document Type
							you want to create a Document Blueprint for.
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
		</umb-body-layout> `;
	}

	static override styles = [UmbTextStyles];
}

export default UmbDocumentBlueprintRootWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-blueprint-root-workspace': UmbDocumentBlueprintRootWorkspaceElement;
	}
}
