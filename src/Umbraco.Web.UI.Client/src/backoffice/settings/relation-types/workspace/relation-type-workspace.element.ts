import { UUIInputElement, UUIInputEvent } from '@umbraco-ui/uui';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbRelationTypeWorkspaceContext } from './relation-type-workspace.context';
import { UmbLitElement } from '@umbraco-cms/element';

/**
 * @element umb-relation-type-workspace
 * @description - Element for displaying a Data Type Workspace
 */
@customElement('umb-relation-type-workspace')
export class UmbRelationTypeWorkspaceElement extends UmbLitElement {
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

	#workspaceContext = new UmbRelationTypeWorkspaceContext(this);

	@state()
	private _RelationTypeName = '';

	public load(value: string) {
		this.#workspaceContext?.load(value);
		//this._unique = entityKey;
	}

	public create(parentKey: string | null) {
		this.#workspaceContext.createScaffold(parentKey);
	}

	constructor() {
		super();
		this.observe(this.#workspaceContext.name, (RelationTypeName) => {
			if (RelationTypeName !== this._RelationTypeName) {
				this._RelationTypeName = RelationTypeName ?? '';
			}
		});
	}

	// TODO. find a way where we don't have to do this for all Workspaces.
	#handleInput(event: UUIInputEvent) {
		if (event instanceof UUIInputEvent) {
			const target = event.composedPath()[0] as UUIInputElement;

			if (typeof target?.value === 'string') {
				this.#workspaceContext.setName(target.value);
			}
		}
	}

	render() {
		return html`
			<umb-workspace-layout alias="Umb.Workspace.RelationType">
				<uui-input id="header" slot="header" .value=${this._RelationTypeName} @input="${this.#handleInput}"></uui-input>
			</umb-workspace-layout>
		`;
	}
}

export default UmbRelationTypeWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-relation-type-workspace': UmbRelationTypeWorkspaceElement;
	}
}
