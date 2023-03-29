import { UUIInputElement, UUIInputEvent } from '@umbraco-ui/uui';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbDictionaryWorkspaceContext } from './dictionary-workspace.context';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UMB_ENTITY_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/context-api';

@customElement('umb-dictionary-workspace-edit')
export class UmbDictionaryWorkspaceEditElement extends UmbLitElement {
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
	private _name?: string | null = '';

	#workspaceContext?: UmbDictionaryWorkspaceContext;

	constructor() {
		super();

		this.consumeContext(UMB_ENTITY_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance as UmbDictionaryWorkspaceContext;
			this.#observeName();
		});
	}

	#observeName() {
		if (!this.#workspaceContext) return;
		this.observe(this.#workspaceContext.name, (name) => (this._name = name));
	}

	// TODO. find a way where we don't have to do this for all workspaces.
	#handleInput(event: UUIInputEvent) {
		if (event instanceof UUIInputEvent) {
			const target = event.composedPath()[0] as UUIInputElement;

			if (typeof target?.value === 'string') {
				this.#workspaceContext?.setName(target.value);
			}
		}
	}

	render() {
		return html`
			<umb-workspace-layout alias="Umb.Workspace.Dictionary">
				<div id="header" slot="header">
					<uui-button href="/section/translation/dashboard" label="Back to list" compact>
						<uui-icon name="umb:arrow-left"></uui-icon>
					</uui-button>
					<uui-input .value=${this._name} @input="${this.#handleInput}" label="Dictionary name"></uui-input>
				</div>
			</umb-workspace-layout>
		`;
	}
}

export default UmbDictionaryWorkspaceEditElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-dictionary-workspace-edit': UmbDictionaryWorkspaceEditElement;
	}
}
