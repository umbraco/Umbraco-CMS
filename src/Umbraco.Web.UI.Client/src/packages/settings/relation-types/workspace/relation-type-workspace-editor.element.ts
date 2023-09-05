import { UMB_RELATION_TYPE_WORKSPACE_CONTEXT } from './relation-type-workspace.context.js';
import { UUIInputElement, UUIInputEvent, UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { RelationTypeResponseModel } from '@umbraco-cms/backoffice/backend-api';
/**
 * @element umb-relation-type-workspace-editor
 * @description - Element for displaying a Relation Type Workspace
 */
@customElement('umb-relation-type-workspace-editor')
export class UmbRelationTypeWorkspaceEditorElement extends UmbLitElement {
	#workspaceContext?: typeof UMB_RELATION_TYPE_WORKSPACE_CONTEXT.TYPE;

	@state()
	private _relationType?: RelationTypeResponseModel;

	constructor() {
		super();

		this.consumeContext(UMB_RELATION_TYPE_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance;
			this.#observeRelationType();
		});
	}

	#observeRelationType() {
		if (!this.#workspaceContext) return;
		this.observe(this.#workspaceContext.data, (data) => (this._relationType = data));
	}

	// TODO. find a way where we don't have to do this for all workspaces.
	private _handleInput(event: UUIInputEvent) {
		if (event instanceof UUIInputEvent) {
			const target = event.composedPath()[0] as UUIInputElement;

			if (typeof target?.value === 'string') {
				this.#workspaceContext?.setName(target.value);
			}
		}
	}

	render() {
		return html`
			<umb-workspace-editor alias="Umb.Workspace.RelationType">
				<div id="header" slot="header">
					<uui-input id="name" .value=${this._relationType?.name} @input="${this._handleInput}">
						<div id="alias" slot="append">${this._relationType?.alias}</div>
					</uui-input>
				</div>
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
				display: flex;
				flex: 1 1 auto;
				margin: 0 var(--uui-size-layout-1);
			}

			#name {
				width: 100%;
				flex: 1 1 auto;
				align-items: center;
			}

			#alias {
				padding: 0 var(--uui-size-space-3);
				opacity: 0.5;
			}
		`,
	];
}

export default UmbRelationTypeWorkspaceEditorElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-relation-type-workspace-editor': UmbRelationTypeWorkspaceEditorElement;
	}
}
