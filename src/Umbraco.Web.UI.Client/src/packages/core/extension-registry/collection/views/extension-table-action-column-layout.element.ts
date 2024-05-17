import { umbExtensionsRegistry } from '../../index.js';
import type { UmbExtensionCollectionFilterModel } from '../types.js';
import { UMB_COLLECTION_CONTEXT } from '@umbraco-cms/backoffice/collection';
import { html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { umbConfirmModal } from '@umbraco-cms/backoffice/modal';
import type { ManifestBase } from '@umbraco-cms/backoffice/extension-api';
import type { UmbDefaultCollectionContext } from '@umbraco-cms/backoffice/collection';

@customElement('umb-extension-table-action-column-layout')
export class UmbExtensionTableActionColumnLayoutElement extends UmbLitElement {
	@property({ attribute: false })
	value!: ManifestBase;

	#collectionContext?: UmbDefaultCollectionContext<any, UmbExtensionCollectionFilterModel>;

	constructor() {
		super();

		this.consumeContext(UMB_COLLECTION_CONTEXT, (instance) => {
			this.#collectionContext = instance;
		});
	}

	async #removeExtension() {
		await umbConfirmModal(this, {
			headline: 'Unload extension',
			confirmLabel: 'Unload',
			content: html`<p>Are you sure you want to unload the extension <strong>${this.value.alias}</strong>?</p>`,
			color: 'danger',
		});

		umbExtensionsRegistry.unregister(this.value.alias);

		this.#collectionContext?.requestCollection();
	}

	render() {
		return html`
			<uui-button label="Unload" color="danger" look="primary" @click=${this.#removeExtension}>
				<uui-icon name="icon-trash"></uui-icon>
			</uui-button>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-extension-table-action-column-layout': UmbExtensionTableActionColumnLayoutElement;
	}
}
