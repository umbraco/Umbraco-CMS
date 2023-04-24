import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbRouterSlotInitEvent } from '@umbraco-cms/internal/router';
import type { IRoute, IRoutingInfo, PageComponent } from '@umbraco-cms/backoffice/router';

import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

import './template-workspace-edit.element';
import { UmbTemplateWorkspaceContext } from './template-workspace.context';

@customElement('umb-template-workspace')
export class UmbTemplateWorkspaceElement extends UmbLitElement {

	public load(entityId: string) {
		this.#templateWorkspaceContext.load(entityId);
	}

	public create(parentId: string | null) {
		this.#isNew = true;
		this.#templateWorkspaceContext.createScaffold(parentId);
	}

	@state()
	private _name?: string | null = '';

	@state()
	private _content?: string | null = '';

	@query('umb-code-editor')
	private _codeEditor?: UmbCodeEditorElement;

	#templateWorkspaceContext = new UmbTemplateWorkspaceContext(this);

	#routerPath? = '';

	#element = document.createElement('umb-template-workspace-edit');
	#key = '';

	@state()
	_routes: IRoute[] = [
		{
			path: 'create/:parentKey',
			component: () => this.#element,
			setup: (component: PageComponent, info: IRoutingInfo) => {
				const parentKey = info.match.params.parentKey;
				this.#templateWorkspaceContext.createScaffold(parentKey);
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
