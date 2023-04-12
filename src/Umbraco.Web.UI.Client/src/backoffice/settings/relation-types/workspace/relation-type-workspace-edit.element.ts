import { UUIInputElement, UUIInputEvent } from '@umbraco-ui/uui';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbRelationTypeWorkspaceContext } from './relation-type-workspace.context';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { RelationTypeResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UMB_ENTITY_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/context-api';

/**
 * @element umb-relation-type-workspace-edit-element
 * @description - Element for displaying a Relation Type Workspace
 */
@customElement('umb-relation-type-workspace-edit-element')
export class UmbRelationTypeWorkspaceEditElement extends UmbLitElement {
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

	#workspaceContext?: UmbRelationTypeWorkspaceContext;

	@state()
	private _relationType?: RelationTypeResponseModel;

	constructor() {
		super();

		this.consumeContext(UMB_ENTITY_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance as UmbRelationTypeWorkspaceContext;
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
			<umb-workspace-layout alias="Umb.Workspace.RelationType">
				<div id="header" slot="header">
					<uui-input id="name" .value=${this._relationType?.name} @input="${this._handleInput}">
						<div id="alias" slot="append">${this._relationType?.alias}</div>
					</uui-input>
				</div>
			</umb-workspace-layout>
		`;
	}
}

export default UmbRelationTypeWorkspaceEditElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-relation-type-workspace-edit-element': UmbRelationTypeWorkspaceEditElement;
	}
}
