import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbWorkspaceMediaTypeContext } from './media-type-workspace.context';
import { UmbMediaTypeWorkspaceEditElement } from './media-type-workspace-edit.element';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { IRoutingInfo } from '@umbraco-cms/internal/router';

@customElement('umb-media-type-workspace')
export class UmbMediaTypeWorkspaceElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			#header {
				display: flex;
				padding: 0 var(--uui-size-layout-1);
				gap: var(--uui-size-space-4);
				width: 100%;
			}
			uui-input {
				width: 100%;
			}
		`,
	];

	#workspaceContext = new UmbWorkspaceMediaTypeContext(this);
	#element = new UmbMediaTypeWorkspaceEditElement();

	@state()
	_routes: any[] = [
		{
			path: 'edit/:key',
			component: () => this.#element,
			setup: (component: HTMLElement, info: IRoutingInfo) => {
				const key = info.match.params.key;
				this.#workspaceContext.load(key);
			},
		},
	];

	render() {
		return html`<umb-router-slot .routes=${this._routes}></umb-router-slot>`;
	}
}

export default UmbMediaTypeWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-type-workspace-': UmbMediaTypeWorkspaceElement;
	}
}
