import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, nothing, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-language-root-table-boolean-column-layout')
export class UmbLanguageRootTableBooleanColumnLayoutElement extends UmbLitElement {
	@property({ attribute: false })
	value = false;

	render() {
		return this.value ? html`<uui-icon name="icon-check"></uui-icon>` : nothing;
	}

	static styles = [UmbTextStyles, css``];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-language-root-table-boolean-column-layout': UmbLanguageRootTableBooleanColumnLayoutElement;
	}
}
