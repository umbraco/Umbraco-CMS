import { UMB_RELATION_TYPE_WORKSPACE_CONTEXT } from './relation-type-workspace.context-token.js';
import { observeMultiple } from '@umbraco-cms/backoffice/observable-api';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_RELATIONS_ROOT_WORKSPACE_PATH } from 'src/packages/relations/workspace/paths.js';

/**
 * @element umb-relation-type-workspace-editor
 * @description - Element for displaying a Relation Type Workspace
 */
@customElement('umb-relation-type-workspace-editor')
export class UmbRelationTypeWorkspaceEditorElement extends UmbLitElement {
	#workspaceContext?: typeof UMB_RELATION_TYPE_WORKSPACE_CONTEXT.TYPE;

	@state()
	private _name?: string = '';

	@state()
	private _alias?: string = '';

	constructor() {
		super();

		this.consumeContext(UMB_RELATION_TYPE_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance;
			this.#observeRelationType();
		});
	}

	#observeRelationType() {
		if (!this.#workspaceContext) return;
		this.observe(observeMultiple([this.#workspaceContext.name, this.#workspaceContext.alias]), ([name, alias]) => {
			this._name = name;
			this._alias = alias;
		});
	}

	override render() {
		return html`
			<umb-workspace-editor alias="Umb.Workspace.RelationType" back-path="${UMB_RELATIONS_ROOT_WORKSPACE_PATH}">
				<div id="header" slot="header">
					<uui-input id="name" .value=${this._name} readonly>
						<div id="alias" slot="append">${this._alias}</div>
					</uui-input>
				</div>
			</umb-workspace-editor>
		`;
	}

	static override styles = [
		css`
			:host {
				display: block;
				width: 100%;
				height: 100%;
			}

			#header {
				display: flex;
				flex: 1 1 auto;
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
