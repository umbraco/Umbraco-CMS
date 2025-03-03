import { UMB_PARTIAL_VIEW_FOLDER_WORKSPACE_CONTEXT } from './partial-view-folder-workspace.context-token.js';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

const elementName = 'umb-partial-view-folder-workspace-editor';
@customElement(elementName)
export class UmbPartialViewFolderWorkspaceEditorElement extends UmbLitElement {
	@state()
	private _name = '';

	#workspaceContext?: typeof UMB_PARTIAL_VIEW_FOLDER_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_PARTIAL_VIEW_FOLDER_WORKSPACE_CONTEXT, (workspaceContext) => {
			this.#workspaceContext = workspaceContext;
			this.#observeName();
		});
	}

	#observeName() {
		if (!this.#workspaceContext) return;
		this.observe(
			this.#workspaceContext.name,
			(name) => {
				if (name !== this._name) {
					this._name = name ?? '';
				}
			},
			'observeName',
		);
	}

	override render() {
		return html`<umb-workspace-editor headline=${this._name}> </umb-workspace-editor>`;
	}

	static override styles = [UmbTextStyles, css``];
}

export { UmbPartialViewFolderWorkspaceEditorElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbPartialViewFolderWorkspaceEditorElement;
	}
}
