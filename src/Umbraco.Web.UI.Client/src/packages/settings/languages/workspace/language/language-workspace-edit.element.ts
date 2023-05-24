import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { ifDefined } from 'lit/directives/if-defined.js';
import { UUIInputElement, UUIInputEvent } from '@umbraco-ui/uui';
import { UmbLanguageWorkspaceContext } from './language-workspace.context.js';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { LanguageResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UMB_ENTITY_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
@customElement('umb-language-workspace-edit')
export class UmbLanguageWorkspaceEditElement extends UmbLitElement {
	#workspaceContext?: UmbLanguageWorkspaceContext;

	@state()
	_language?: LanguageResponseModel;

	@state()
	_isNew?: boolean;

	constructor() {
		super();

		this.consumeContext(UMB_ENTITY_WORKSPACE_CONTEXT, (context) => {
			this.#workspaceContext = context as UmbLanguageWorkspaceContext;
			this.#observeData();
		});
	}

	#observeData() {
		if (!this.#workspaceContext) return;
		this.observe(this.#workspaceContext.data, (data) => {
			this._language = data;
		});
		this.observe(this.#workspaceContext.isNew, (isNew) => {
			this._isNew = isNew;
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
		return html`<umb-workspace-editor alias="Umb.Workspace.Language">
			<div id="header" slot="header">
				<uui-button label="Navigate back" href="section/settings/workspace/language-root" compact>
					<uui-icon name="umb:arrow-left"></uui-icon>
				</uui-button>
				${this._isNew
					? html`<strong>Add language</strong>`
					: html`<uui-input
							label="Language name"
							value=${ifDefined(this._language?.name)}
							@input="${this.#handleInput}"></uui-input>`}
			</div>
			<div slot="footer" id="footer">
				<a href="section/settings/workspace/language-root">Languages</a> /
				${this._isNew ? 'Create' : this._language?.name}
			</div>
		</umb-workspace-editor>`;
	}

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

			strong {
				display: flex;
				align-items: center;
			}

			#footer {
				padding: 0 var(--uui-size-layout-1);
			}

			uui-input:not(:focus) {
				border: 1px solid transparent;
			}
		`,
	];
}

export default UmbLanguageWorkspaceEditElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-language-workspace-edit': UmbLanguageWorkspaceEditElement;
	}
}
