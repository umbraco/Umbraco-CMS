import { UmbDataTypeWorkspaceContext } from './data-type-workspace.context.js';
import { UUIInputElement, UUIInputEvent, UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { css, html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { ManifestWorkspace } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_ENTITY_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
/**
 * @element umb-data-type-workspace-editor
 * @description - Element for displaying the Data Type Workspace edit route.
 */
@customElement('umb-data-type-workspace-editor')
export class UmbDataTypeWorkspaceEditorElement extends UmbLitElement {
	@property()
	manifest?: ManifestWorkspace;

	@state()
	private _dataTypeName = '';

	#workspaceContext?: UmbDataTypeWorkspaceContext;

	constructor() {
		super();

		this.consumeContext(UMB_ENTITY_WORKSPACE_CONTEXT, (workspaceContext) => {
			this.#workspaceContext = workspaceContext as UmbDataTypeWorkspaceContext;
			this.#observeName();
		});
	}

	#observeName() {
		if (!this.#workspaceContext) return;
		this.observe(this.#workspaceContext.name, (dataTypeName) => {
			if (dataTypeName !== this._dataTypeName) {
				this._dataTypeName = dataTypeName ?? '';
			}
		});
	}

	// TODO. find a way where we don't have to do this for all Workspaces.
	#handleInput(event: UUIInputEvent) {
		if (event instanceof UUIInputEvent) {
			const target = event.composedPath()[0] as UUIInputElement;

			if (typeof target?.value === 'string') {
				this.#workspaceContext?.setName(target.value);
			}
		}
	}

	render() {
		return html`
			<umb-workspace-editor alias="Umb.Workspace.DataType">
				<uui-input slot="header" id="header" .value=${this._dataTypeName} @input="${this.#handleInput}"></uui-input>
			</umb-workspace-editor>
		`;
	}

	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				width: 100%;
				height: 100%;
			}

			#header {
				/* TODO: can this be applied from layout slot CSS? */
				margin: 0 var(--uui-size-layout-1);
				flex: 1 1 auto;
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
