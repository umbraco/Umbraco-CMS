import { UmbMemberGroupWorkspaceContext } from './member-group-workspace.context.js';
import { UmbMemberGroupWorkspaceEditorElement } from './member-group-workspace-editor.element.js';
import { UmbTextStyles } from "@umbraco-cms/backoffice/style";
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import type { UmbRoute } from '@umbraco-cms/backoffice/router';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

/**
 * @element umb-member-group-workspace
 * @description - Element for displaying a Member Group Workspace
 */
@customElement('umb-member-group-workspace')
export class UmbMemberGroupWorkspaceElement extends UmbLitElement {
	#workspaceContext = new UmbMemberGroupWorkspaceContext(this);
	#element = new UmbMemberGroupWorkspaceEditorElement();

	@state()
	_routes: UmbRoute[] = [
		{
			path: 'edit/:id',
			component: () => this.#element,
			setup: (_component, info) => {
				const id = info.match.params.id;
				this.#workspaceContext.load(id);
			},
		},
	];

	render() {
		return html`<umb-router-slot .routes=${this._routes}></umb-router-slot> `;
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

export default UmbMemberGroupWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-member-group-workspace': UmbMemberGroupWorkspaceElement;
	}
}
