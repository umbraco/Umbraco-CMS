import { UMB_RELATIONS_ROOT_WORKSPACE_PATH } from '../../../workspace/paths.js';
import { UMB_RELATION_TYPE_WORKSPACE_CONTEXT } from './relation-type-workspace.context-token.js';
import { observeMultiple } from '@umbraco-cms/backoffice/observable-api';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

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
			<umb-workspace-editor back-path="${UMB_RELATIONS_ROOT_WORKSPACE_PATH}">
				<umb-input-with-alias
					id="name"
					slot="header"
					label=${this.localize.term('placeholders_entername')}
					.value=${this._name ?? ''}
					.alias=${this._alias ?? ''}
					readonly>
				</umb-input-with-alias>
			</umb-workspace-editor>
		`;
	}

	static override readonly styles = [
		css`
			:host {
				display: block;
				width: 100%;
				height: 100%;
			}

			#name {
				width: 100%;
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
