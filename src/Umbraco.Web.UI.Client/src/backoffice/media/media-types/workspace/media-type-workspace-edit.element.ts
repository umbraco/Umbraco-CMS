import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UUIInputElement, UUIInputEvent } from '@umbraco-ui/uui';
import { UmbWorkspaceMediaTypeContext } from './media-type-workspace.context';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UMB_ENTITY_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/context-api';

@customElement('umb-media-type-workspace-edit')
export class UmbMediaTypeWorkspaceEditElement extends UmbLitElement {
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

	@state()
	private _mediaTypeName?: string | null = '';
	#workspaceContext?: UmbWorkspaceMediaTypeContext;

	constructor() {
		super();

		this.consumeContext(UMB_ENTITY_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance as UmbWorkspaceMediaTypeContext;
			this.#observeName();
		});
	}

	#observeName() {
		if (!this.#workspaceContext) return;
		this.observe(this.#workspaceContext.name, (name) => {
			this._mediaTypeName = name;
		});
	}

	// TODO. find a way where we don't have to do this for all Workspaces.
	#handleInput(event: UUIInputEvent) {
		if (event instanceof UUIInputEvent) {
			const target = event.composedPath()[0] as UUIInputElement;

			if (typeof target?.value === 'string') {
				this.#workspaceContext?.setName(target.value);
			}
		}
	}

	render() {
		return html`<umb-workspace-layout alias="Umb.Workspace.MediaType">
			<uui-input id="header" slot="header" .value=${this._mediaTypeName} @input="${this.#handleInput}"></uui-input>
		</umb-workspace-layout>`;
	}
}

export default UmbMediaTypeWorkspaceEditElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-type-workspace-edit': UmbMediaTypeWorkspaceEditElement;
	}
}
