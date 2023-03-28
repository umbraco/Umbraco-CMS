import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { ifDefined } from 'lit/directives/if-defined.js';
import { UUIInputElement, UUIInputEvent } from '@umbraco-ui/uui';
import { UmbLanguageWorkspaceContext } from './language-workspace.context';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { LanguageResponseModel } from '@umbraco-cms/backoffice/backend-api';

@customElement('umb-language-workspace-edit')
export class UmbLanguageWorkspaceEditElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			#header {
				display: flex;
				padding: 0 var(--uui-size-space-6);
				gap: var(--uui-size-space-4);
				width: 100%;
			}
			uui-input {
				width: 100%;
			}
			#footer {
				padding: 0 var(--uui-size-layout-1);
			}
		`,
	];

	#workspaceContext?: UmbLanguageWorkspaceContext;

	@state()
	_language?: LanguageResponseModel;

	constructor() {
		super();

		this.consumeContext<UmbLanguageWorkspaceContext>('umbWorkspaceContext', (context) => {
			this.#workspaceContext = context;
			this.#observeData();
		});
	}

	#observeData() {
		if (!this.#workspaceContext) return;
		this.observe(this.#workspaceContext.data, (data) => {
			this._language = data;
		});
	}

	#handleInput(event: UUIInputEvent) {
		if (event instanceof UUIInputEvent) {
			const target = event.composedPath()[0] as UUIInputElement;

			if (typeof target?.value === 'string') {
				this.#workspaceContext?.setName(target.value);
			}
		}
	}

	render() {
		return html`<umb-workspace-layout alias="Umb.Workspace.Language">
			<div id="header" slot="header">
				<uui-button label="Navigate back" href="/section/settings/workspace/language-root" compact>
					<uui-icon name="umb:arrow-left"></uui-icon>
				</uui-button>
				<uui-input value=${ifDefined(this._language?.name)} @input="${this.#handleInput}"></uui-input>
			</div>
			<div slot="footer" id="footer">
				<a href="/section/settings/workspace/language-root">Languages</a> / ${this._language?.name}
			</div>
		</umb-workspace-layout>`;
	}
}

export default UmbLanguageWorkspaceEditElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-language-workspace-edit': UmbLanguageWorkspaceEditElement;
	}
}
