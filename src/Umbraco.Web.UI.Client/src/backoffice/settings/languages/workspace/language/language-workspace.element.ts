import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, nothing } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UUIInputElement, UUIInputEvent } from '@umbraco-ui/uui';
import { UmbWorkspaceEntityElement } from '../../../../shared/components/workspace/workspace-entity-element.interface';
import { UmbLanguageWorkspaceContext } from './language-workspace.context';
import { UmbLitElement } from '@umbraco-cms/element';
import '../../../../shared/components/workspace/workspace-action/save/workspace-action-node-save.element.ts';
import { LanguageModel } from '@umbraco-cms/backend-api';

@customElement('umb-language-workspace')
export class UmbLanguageWorkspaceElement extends UmbLitElement implements UmbWorkspaceEntityElement {
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
		`,
	];

	@state()
	_language?: LanguageModel;

	#languageWorkspaceContext = new UmbLanguageWorkspaceContext(this);

	load(key: string): void {
		this.#languageWorkspaceContext.load(key);
	}

	create(): void {
		this.#languageWorkspaceContext.createScaffold();
	}

	async connectedCallback() {
		super.connectedCallback();

		this.observe(this.#languageWorkspaceContext.data, (data) => {
			this._language = data;
		});
	}

	#handleInput(event: UUIInputEvent) {
		if (event instanceof UUIInputEvent) {
			const target = event.composedPath()[0] as UUIInputElement;

			if (typeof target?.value === 'string') {
				this.#languageWorkspaceContext?.setName(target.value);
			}
		}
	}

	render() {
		if (!this._language) return nothing;

		return html`
			<umb-workspace-layout alias="Umb.Workspace.Language">
				<div id="header" slot="header">
					<uui-button href="/section/settings/language-root" compact>
						<uui-icon name="umb:arrow-left"></uui-icon>
					</uui-button>
					<uui-input .value=${this._language.name} @input="${this.#handleInput}"></uui-input>
				</div>
			</umb-workspace-layout>
		`;
	}
}

export default UmbLanguageWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-language-workspace': UmbLanguageWorkspaceElement;
	}
}
