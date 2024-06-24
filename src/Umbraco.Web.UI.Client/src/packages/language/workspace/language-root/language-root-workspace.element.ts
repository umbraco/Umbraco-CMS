import { UMB_LANGUAGE_COLLECTION_ALIAS } from '../../collection/index.js';
import { html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-language-root-workspace')
export class UmbLanguageRootWorkspaceElement extends UmbLitElement {
	override render() {
		return html` <umb-body-layout main-no-padding headline="Languages">
			<umb-collection alias=${UMB_LANGUAGE_COLLECTION_ALIAS}></umb-collection>;
		</umb-body-layout>`;
	}
}

export { UmbLanguageRootWorkspaceElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-language-root-workspace': UmbLanguageRootWorkspaceElement;
	}
}
