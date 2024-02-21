import { umbExtensionsRegistry, type ManifestTypes } from '../../index.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbModalManagerContext } from '@umbraco-cms/backoffice/modal';
import { UMB_CONFIRM_MODAL, UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';

@customElement('umb-extension-table-action-column-layout')
export class UmbExtensionTableActionColumnLayoutElement extends UmbLitElement {
	@property({ attribute: false })
	value!: ManifestTypes;

	#modalContext?: UmbModalManagerContext;

	constructor() {
		super();
		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, (instance) => {
			this.#modalContext = instance;
		});
	}

	async #removeExtension() {
		const modalContext = this.#modalContext?.open(UMB_CONFIRM_MODAL, {
			data: {
				headline: 'Unload extension',
				confirmLabel: 'Unload',
				content: html`<p>Are you sure you want to unload the extension <strong>${this.value.alias}</strong>?</p>`,
				color: 'danger',
			},
		});

		await modalContext?.onSubmit();
		umbExtensionsRegistry.unregister(this.value.alias);
	}

	render() {
		return html`
			<uui-button label="Unload" color="danger" look="primary" @click=${this.#removeExtension}>
				<uui-icon name="icon-trash"></uui-icon>
			</uui-button>
		`;
	}

	static styles = [UmbTextStyles];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-extension-table-action-column-layout': UmbExtensionTableActionColumnLayoutElement;
	}
}
