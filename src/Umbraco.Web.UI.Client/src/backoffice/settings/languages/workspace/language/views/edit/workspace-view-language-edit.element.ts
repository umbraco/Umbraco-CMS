import { UmbLitElement } from '@umbraco-cms/element';
import { LanguageDetails } from '@umbraco-cms/models';
import { UUIBooleanInputEvent, UUIInputElement, UUIInputEvent, UUIToggleElement } from '@umbraco-ui/uui';
import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, LitElement, nothing } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { UmbWorkspaceLanguageContext } from '../../language-workspace.context';

@customElement('umb-workspace-view-language-edit')
export class UmbWorkspaceViewLanguageEditElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
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

	private _languageWorkspaceContext?: UmbWorkspaceLanguageContext;

	constructor() {
		super();

		this.consumeContext('umbWorkspaceContext', (instance) => {
			this._languageWorkspaceContext = instance;

			if (!this._languageWorkspaceContext) return;

			this._languageWorkspaceContext.data.subscribe((language) => {
				this.language = language;
			});
		});
	}

	private _handleDefaultChange(event: any) {
		if (event instanceof UUIBooleanInputEvent) {
			const target = event.composedPath()[0] as UUIToggleElement;

			this._languageWorkspaceContext?.update({ isDefault: target.checked });
		}
	}

	private _handleMandatoryChange(event: any) {
		if (event instanceof UUIBooleanInputEvent) {
			const target = event.composedPath()[0] as UUIToggleElement;

			this._languageWorkspaceContext?.update({ isMandatory: target.checked });
		}
	}

	render() {
		if (!this.language) return nothing;

		return html`
			<uui-box>
				<umb-workspace-property-layout label="Language">
					<uui-input .value=${this.language.name} slot="editor"></uui-input>
				</umb-workspace-property-layout>
				<umb-workspace-property-layout label="ISO Code">
					<div slot="editor">${this.language.isoCode}</div>
				</umb-workspace-property-layout>
				<umb-workspace-property-layout label="Settings">
					<div slot="editor">
						<uui-toggle ?checked=${this.language.isDefault || false} @change=${this._handleDefaultChange}>
							<div>
								<b>Default language</b>
								<div>An Umbraco site can only have one default language set.</div>
							</div>
						</uui-toggle>
						<hr />
						<uui-toggle ?checked=${this.language.isMandatory || false} @change=${this._handleMandatoryChange}>
							<div>
								<b>Mandatory language</b>
								<div>Properties on this language have to be filled out before the node can be published.</div>
							</div>
						</uui-toggle>
						<div id="default-language-warning">Switching default language may result in default content missing.</div>
					</div>
				</umb-workspace-property-layout>
				<umb-workspace-property-layout
					label="Fall back language"
					description="To allow multi-lingual content to fall back to another language if not present in the requested language, select it here.">
					<uui-combobox slot="editor">
						<uui-combobox-list>
							<uui-combobox-list-option value="">No fall back language</uui-combobox-list-option>
						</uui-combobox-list>
					</uui-combobox>
				</umb-workspace-property-layout>
			</uui-box>
		`;
	}
}

export default UmbWorkspaceViewLanguageEditElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-view-language-edit': UmbWorkspaceViewLanguageEditElement;
	}
}
