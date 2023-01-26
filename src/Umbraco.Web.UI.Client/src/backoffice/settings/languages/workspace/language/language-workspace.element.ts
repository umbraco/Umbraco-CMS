import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, nothing } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { UUIInputElement, UUIInputEvent } from '@umbraco-ui/uui';
import { UmbWorkspaceLanguageContext } from './language-workspace.context';
import { UmbLitElement } from '@umbraco-cms/element';
import type { LanguageDetails } from '@umbraco-cms/models';
import '../../../../shared/components/workspace/actions/save/workspace-action-node-save.element.ts';

@customElement('umb-language-workspace')
export class UmbLanguageWorkspaceElement extends UmbLitElement {
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
	language?: LanguageDetails;

	private _entityKey!: string;
	@property()
	public get entityKey(): string {
		return this._entityKey;
	}
	public set entityKey(value: string) {
		this._entityKey = value;
		if (this._entityKey && !this._languageWorkspaceContext) {
			this.provideLanguageWorkspaceContext();
		}
	}

	private _languageWorkspaceContext?: UmbWorkspaceLanguageContext;

	public provideLanguageWorkspaceContext() {
		this._languageWorkspaceContext = new UmbWorkspaceLanguageContext(this, this._entityKey);
		this.provideContext('umbWorkspaceContext', this._languageWorkspaceContext);
		this._languageWorkspaceContext.data.subscribe((language) => {
			this.language = language;
		});
	}

	private _handleInput(event: UUIInputEvent) {
		if (event instanceof UUIInputEvent) {
			const target = event.composedPath()[0] as UUIInputElement;

			if (typeof target?.value === 'string') {
				this._languageWorkspaceContext?.update({ name: target.value });
			}
		}
	}

	render() {
		if (!this.language) return nothing;

		return html`
			<umb-workspace-layout alias="Umb.Workspace.Language">
				<div id="header" slot="header">
					<a href="/section/settings/language-root">
						<uui-button compact>
							<uui-icon name="umb:arrow-left"></uui-icon>
						</uui-button>
					</a>
					<uui-input .value=${this.language.name} @input="${this._handleInput}"></uui-input>
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
