import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, nothing } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { UmbLanguageStore, UmbLanguageStoreItemType, UMB_LANGUAGE_STORE_CONTEXT_TOKEN } from '../../language.store';
import { UmbModalService, UMB_MODAL_SERVICE_CONTEXT_TOKEN } from '../../../../../core/modal';
import { UmbLitElement } from '@umbraco-cms/element';

@customElement('umb-language-root-table-delete-column-layout')
export class UmbLanguageRootTableDeleteColumnLayoutElement extends UmbLitElement {
	static styles = [UUITextStyles, css``];

	@property({ attribute: false })
	value!: UmbLanguageStoreItemType;

	#languageStore?: UmbLanguageStore;
	#modalService?: UmbModalService;

	constructor() {
		super();
		this.consumeContext(UMB_LANGUAGE_STORE_CONTEXT_TOKEN, (instance) => {
			this.#languageStore = instance;
		});

		this.consumeContext(UMB_MODAL_SERVICE_CONTEXT_TOKEN, (instance) => {
			this.#modalService = instance;
		});
	}

	#handleDelete(event: MouseEvent) {
		event.stopImmediatePropagation();
		if (!this.#languageStore) return;

		const modalHandler = this.#modalService?.confirm({
			headline: 'Delete language',
			content: html`
				<div
					style="padding: var(--uui-size-space-4); background-color: var(--uui-color-danger); color: var(--uui-color-danger-contrast); border: 1px solid var(--uui-color-danger-standalone); border-radius: var(--uui-border-radius)">
					This will delete language <b>${this.value.name}</b>.
				</div>
				Are you sure you want to delete?
			`,
			color: 'danger',
			confirmLabel: 'Delete',
		});

		modalHandler?.onClose().then(({ confirmed }) => {
			if (confirmed) {
				this.#languageStore?.delete([this.value.isoCode!]);
			}
		});
	}

	render() {
		if (this.value.isDefault) return nothing;

		return html`<uui-button
			@click=${this.#handleDelete}
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
