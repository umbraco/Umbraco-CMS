import { UMB_DATA_TYPE_WORKSPACE_CONTEXT } from './data-type-workspace.context-token.js';
import { css, html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
/**
 * @element umb-data-type-workspace-editor
 * @description - Element for displaying the Data Type Workspace edit route.
 */
@customElement('umb-data-type-workspace-editor')
export class UmbDataTypeWorkspaceEditorElement extends UmbLitElement {
	constructor() {
		super();

		this.consumeContext(UMB_DATA_TYPE_WORKSPACE_CONTEXT, (workspaceContext) => {
			workspaceContext?.createPropertyDatasetContext(this);
		});
	}

	override render() {
		return html`
			<umb-entity-detail-workspace-editor>
				<umb-workspace-header-name-editable slot="header"></umb-workspace-header-name-editable>
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

export default UmbDataTypeWorkspaceEditorElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-data-type-workspace-editor': UmbDataTypeWorkspaceEditorElement;
	}
}
