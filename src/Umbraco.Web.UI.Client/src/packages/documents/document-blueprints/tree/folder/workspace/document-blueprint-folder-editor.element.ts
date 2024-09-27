import { UMB_DOCUMENT_BLUEPRINT_FOLDER_WORKSPACE_CONTEXT } from './document-blueprint-folder.workspace.context-token.js';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import type { UUIInputElement } from '@umbraco-cms/backoffice/external/uui';
import { UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';
import { umbFocus, UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { umbBindToValidation } from '@umbraco-cms/backoffice/validation';

const elementName = 'umb-document-blueprint-folder-workspace-editor';
@customElement(elementName)
export class UmbDocumentBlueprintFolderWorkspaceEditorElement extends UmbLitElement {
	@state()
	private _name = '';

	#workspaceContext?: typeof UMB_DOCUMENT_BLUEPRINT_FOLDER_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_DOCUMENT_BLUEPRINT_FOLDER_WORKSPACE_CONTEXT, (workspaceContext) => {
			this.#workspaceContext = workspaceContext;
			this.#observeName();
		});
	}

	#observeName() {
		if (!this.#workspaceContext) return;
		this.observe(this.#workspaceContext.name, (name) => {
			if (name !== this._name) {
				this._name = name ?? '';
			}
		});
	}

	#handleInput(event: UUIInputEvent) {
		if (event instanceof UUIInputEvent) {
			const target = event.composedPath()[0] as UUIInputElement;

			if (typeof target?.value === 'string') {
				this.#workspaceContext?.setName(target.value);
			}
		}
	}

	override render() {
		return html`<umb-workspace-editor>
			<uui-input
				slot="header"
				id="nameInput"
				.value=${this._name ?? ''}
				@input="${this.#handleInput}"
				required
				${umbBindToValidation(this, `$.name`, this._name)}
				${umbFocus()}></uui-input>
		</umb-workspace-editor>`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			#nameInput {
				flex: 1 1 auto;
			}
		`,
	];
}

export { UmbDocumentBlueprintFolderWorkspaceEditorElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbDocumentBlueprintFolderWorkspaceEditorElement;
	}
}
