import { Language } from '@umbraco-cms/backend-api';
import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, LitElement } from 'lit';
import { customElement, property } from 'lit/decorators.js';

@customElement('umb-language-workspace')
export class UmbLanguageWorkspaceElement extends LitElement {
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
		`,
	];

	@property()
	language: Language = {
		id: 1,
		name: 'English',
		isoCode: 'en-us',
		isDefault: true,
		isMandatory: true,
	};

	render() {
		return html`<umb-body-layout>
			<div id="header" slot="header">
				<uui-input .value=${this.language.name}></uui-input>
			</div>
			<div id="main">
				<uui-box>
					<umb-workspace-property-layout label="Language">
						<uui-input .value=${this.language.name} slot="editor"></uui-input>
					</umb-workspace-property-layout>
					<umb-workspace-property-layout label="ISO Code">
						<div slot="editor">
							${this.language.isoCode}
						</div>
					</umb-workspace-property-layout>
					<umb-workspace-property-layout label="Settings">
						<div slot="editor">
							<uui-toggle ?checked=${this.language.isDefault || false}>
								<div>
									<b>Default language</b>
									<div>An Umbraco site can only have one default language set.</div>
								</div>
							</uui-toggle ?checked=${this.language.isMandatory || false}>
							<hr />
							<uui-toggle>
								<div>
									<b>Mandatory language</b>
									<div>Properties on this language have to be filled out before the node can be published.</div>
								</div>
							</uui-toggle>
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
			</div>
		</umb-body-layout>`;
	}
}

export default UmbLanguageWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-language-workspace': UmbLanguageWorkspaceElement;
	}
}
