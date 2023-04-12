import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { serverFilePathFromUrlFriendlyPath } from '../../utils';
import { UmbStylesheetWorkspaceEditElement } from './stylesheet-workspace-edit.element';
import { UmbStylesheetWorkspaceContext } from './stylesheet-workspace.context';
import type { IRoute } from '@umbraco-cms/backoffice/router';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-stylesheet-workspace')
export class UmbStylesheetWorkspaceElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				width: 100%;
				height: 100%;
			}
		`,
	];

	#workspaceContext = new UmbStylesheetWorkspaceContext(this);
	#element = new UmbStylesheetWorkspaceEditElement();

	@state()
	_routes: IRoute[] = [
		{
			path: 'edit/:path',
			component: () => this.#element,
			setup: (_component, info) => {
				const path = info.match.params.path;
				const serverPath = serverFilePathFromUrlFriendlyPath(path);
				this.#workspaceContext.load(serverPath);
			},
		},
	];

	render() {
		return html` <umb-router-slot .routes=${this._routes}></umb-router-slot> `;
	}
}

export default UmbStylesheetWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-stylesheet-workspace': UmbStylesheetWorkspaceElement;
	}
}
