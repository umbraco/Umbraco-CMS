import { UMB_DATA_TYPE_WORKSPACE_CONTEXT } from './data-type-workspace.context.js';
import { UUIInputElement, UUIInputEvent, UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { css, html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { ManifestWorkspace } from '@umbraco-cms/backoffice/extension-registry';
/**
 * @element umb-data-type-workspace-editor
 * @description - Element for displaying the Data Type Workspace edit route.
 */
@customElement('umb-data-type-workspace-editor')
export class UmbDataTypeWorkspaceEditorElement extends UmbLitElement {

	@property({attribute: false})
	manifest?: ManifestWorkspace;

	@state()
	private _dataTypeName = '';

	#workspaceContext?: typeof UMB_DATA_TYPE_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_DATA_TYPE_WORKSPACE_CONTEXT, (workspaceContext) => {
			this.#workspaceContext = workspaceContext;
			this.#workspaceContext?.createVariantContext(this);
			this.#observeIsNew();
			this.#observeName();
		});
	}

	// TODO: invent some general way for all workspaces, with a name?, to put focus on the name when new.
	#observeIsNew() {
		if (!this.#workspaceContext) return;
		this.observe(
			this.#workspaceContext.isNew,
			(isNew) => {
				if (isNew) {
					// TODO: Make a general way to put focus on a input in a modal. (also make sure it only happens if its the top-most-modal.)
					requestAnimationFrame(() => {
						(this.shadowRoot!.querySelector('#nameInput') as HTMLElement).focus();
					});
				}
				this.removeControllerByAlias('_observeIsNew');
			},
			'_observeIsNew'
		);
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
				<uui-input slot="header" id="nameInput" .value=${this._dataTypeName} @input="${this.#handleInput}"></uui-input>
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

			#nameInput {
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
