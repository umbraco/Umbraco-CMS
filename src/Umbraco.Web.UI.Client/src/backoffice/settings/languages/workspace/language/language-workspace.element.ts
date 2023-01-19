import { Language } from '@umbraco-cms/backend-api';
import { UmbLitElement } from '@umbraco-cms/element';
import { LanguageDetails } from '@umbraco-cms/models';
import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, LitElement, nothing } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { UmbWorkspaceLanguageContext } from './language-workspace.context';
import 'src/backoffice/shared/components/workspace/actions/save/workspace-action-node-save.element.ts';

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
			#main {
				padding: var(--uui-size-space-6);
			}
			uui-input {
				width: 100%;
			}
			hr {
				border: none;
				border-bottom: 1px solid var(--uui-color-divider);
			}
			#default-language-warning {
				background-color: var(--uui-color-warning);
				color: var(--uui-color-warning-contrast);
				padding: var(--uui-size-space-4) var(--uui-size-space-5);
				border: 1px solid var(--uui-color-warning-standalone);
				margin-top: var(--uui-size-space-4);
				border-radius: var(--uui-border-radius);
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

	constructor() {
		super();
	}

	public provideLanguageWorkspaceContext() {
		this._languageWorkspaceContext = new UmbWorkspaceLanguageContext(this, this._entityKey);
		this.provideContext('umbWorkspaceContext', this._languageWorkspaceContext);
		this._languageWorkspaceContext.data.subscribe((language) => {
			this.language = language;
		});
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
					<uui-input .value=${this.language.name}></uui-input>
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
