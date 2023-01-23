import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, nothing } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { UmbLanguageStore, UMB_LANGUAGE_STORE_CONTEXT_TOKEN } from '../../language.store';
import type { LanguageDetails } from '@umbraco-cms/models';
import { UmbLitElement } from '@umbraco-cms/element';

@customElement('umb-language-root-table-delete-column-layout')
export class UmbLanguageRootTableDeleteColumnLayoutElement extends UmbLitElement {
	static styles = [UUITextStyles, css``];

	@property({ attribute: false })
	value!: LanguageDetails;

	private _languageStore?: UmbLanguageStore;

	constructor() {
		super();
		this.consumeContext(UMB_LANGUAGE_STORE_CONTEXT_TOKEN, (instance) => {
			this._languageStore = instance;
		});
	}

	private _handleDelete() {
		if (!this._languageStore) return;
		this._languageStore.deleteItems([this.value.key]);
	}

	render() {
		if (this.value.isDefault) return nothing;

		return html`<uui-button
			@click=${this._handleDelete}
			color="danger"
			look="default"
			compact
			label="delete"></uui-button>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-language-root-table-delete-column-layout': UmbLanguageRootTableDeleteColumnLayoutElement;
	}
}
