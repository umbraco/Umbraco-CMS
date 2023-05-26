import { UmbTemplateWorkspaceContext } from './template-workspace.context.js';
import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { css, html , customElement, state } from '@umbraco-cms/backoffice/external/lit';
import type { IRoutingInfo, PageComponent, UmbRoute, UmbRouterSlotInitEvent } from '@umbraco-cms/backoffice/router';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

import './template-workspace-edit.element.js';

@customElement('umb-template-workspace')
export class UmbTemplateWorkspaceElement extends UmbLitElement {
	public load(entityId: string) {
		this.#templateWorkspaceContext.load(entityId);
	}

	@state()
	private _name?: string | null = '';

	@state()
	private _content?: string | null = '';

	#templateWorkspaceContext = new UmbTemplateWorkspaceContext(this);

	#routerPath? = '';

	#element = document.createElement('umb-template-workspace-edit');
	#key = '';

	@state()
	_routes: UmbRoute[] = [
		{
			path: 'create/:parentKey',
			component: () => this.#element,
			setup: (component: PageComponent, info: IRoutingInfo) => {
				// const parentKey = info.match.params.parentKey;
				this.#templateWorkspaceContext.createScaffold();
			},
		},
		{
			path: 'edit/:key',
			component: () => this.#element,
			setup: (component: PageComponent, info: IRoutingInfo): void => {
				const key = info.match.params.key;
				this.#templateWorkspaceContext.load(key);
			},
		},
	];

	render() {
		return html`<umb-router-slot
			.routes=${this._routes}
			@init=${(event: UmbRouterSlotInitEvent) => {
				this.#routerPath = event.target.absoluteRouterPath;
			}}></umb-router-slot>`;
	}

	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				width: 100%;
				height: 100%;
			}

			umb-code-editor {
				--editor-height: calc(100vh - 300px);
			}

			uui-box {
				margin: 1em;
				--uui-box-default-padding: 0;
			}

			uui-input {
				width: 100%;
				margin: 1em;
			}
		`,
	];
}

export default UmbTemplateWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-template-workspace': UmbTemplateWorkspaceElement;
	}
}
