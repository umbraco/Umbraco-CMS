import { UMB_BLOCK_TYPE_WORKSPACE_CONTEXT } from './block-type-workspace.context.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { customElement, css, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
@customElement('umb-block-type-workspace-editor')
export class UmbBlockTypeWorkspaceEditorElement extends UmbLitElement {
	//
	#workspaceContext?: typeof UMB_BLOCK_TYPE_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_BLOCK_TYPE_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance;
			this.#workspaceContext?.createPropertyDatasetContext(this);
		});
	}

	render() {
		return html`
			<umb-workspace-editor
				alias="Umb.Workspace.DataType"
				headline="Block Type (TODO: maybe insert name of Content Type here? or Label?)">
			</umb-workspace-editor>
		`;
	}

	static styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				width: 100%;
				height: 100%;
			}
		`,
	];
}

export default UmbBlockTypeWorkspaceEditorElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-block-type-workspace-editor': UmbBlockTypeWorkspaceEditorElement;
	}
}
