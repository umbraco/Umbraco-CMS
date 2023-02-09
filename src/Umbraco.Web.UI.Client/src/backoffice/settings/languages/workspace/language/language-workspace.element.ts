import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, nothing } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { UUIInputElement, UUIInputEvent } from '@umbraco-ui/uui';
import { UmbLanguageStoreItemType } from '../../language.store';
import { UmbWorkspaceEntityElement } from '../../../../shared/components/workspace/workspace-entity-element.interface';
import { UmbWorkspaceLanguageContext } from './language-workspace.context';
import { UmbLitElement } from '@umbraco-cms/element';
import '../../../../shared/components/workspace/actions/save/workspace-action-node-save.element.ts';

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

	@property()
	language?: UmbLanguageStoreItemType;

	#languageWorkspaceContext?: UmbWorkspaceLanguageContext;

	load(key: string): void {
		this.provideLanguageWorkspaceContext(key);
	}

	create(parentKey: string | null): void {
		this.provideLanguageWorkspaceContext(parentKey);
	}

	public provideLanguageWorkspaceContext(entityKey: string | null) {
		this.#languageWorkspaceContext = new UmbWorkspaceLanguageContext(this, entityKey);
		this.provideContext('umbWorkspaceContext', this.#languageWorkspaceContext);
		this.#languageWorkspaceContext.data.subscribe((language) => {
			this.language = language;
		});
	}

	#handleInput(event: UUIInputEvent) {
		if (event instanceof UUIInputEvent) {
			const target = event.composedPath()[0] as UUIInputElement;

			if (typeof target?.value === 'string') {
				this.#languageWorkspaceContext?.update({ name: target.value });
			}
		}
	}

	render() {
		if (!this.language) return nothing;

		return html`
			<umb-workspace-layout alias="Umb.Workspace.Language">
				<div id="header" slot="header">
					<uui-button href="/section/settings/language-root" compact>
						<uui-icon name="umb:arrow-left"></uui-icon>
					</uui-button>
					<uui-input .value=${this.language.name} @input="${this.#handleInput}"></uui-input>
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
