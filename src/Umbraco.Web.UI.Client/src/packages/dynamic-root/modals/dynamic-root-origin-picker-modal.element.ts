import { UmbDocumentPickerContext } from '../../documents/documents/components/input-document/input-document.context.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, map } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { type UmbTreePickerDynamicRoot } from '@umbraco-cms/backoffice/components';
import { type DocumentItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

@customElement('umb-dynamic-root-origin-picker-modal')
export class UmbDynamicRootOriginPickerModalModalElement extends UmbModalBaseElement {
	constructor() {
		super();

		this.#documentPickerContext.max = 1;

		this.observe(this.#documentPickerContext.selectedItems, (selectedItems) => this.#selectedDocument(selectedItems));
	}

	#choose(alias: string) {
		this.#submit({
			originAlias: alias,
		});
	}

	#close() {
		this.modalContext?.reject();
	}

	#documentPickerContext = new UmbDocumentPickerContext(this);

	#openDocumentPicker() {
		this.#documentPickerContext.openPicker({
			hideTreeRoot: true,
		});
	}

	#selectedDocument(selectedItems: Array<DocumentItemResponseModel>) {
		if (selectedItems.length !== 1) return;
		this.#submit({
			originAlias: 'ByKey',
			originKey: selectedItems[0].id,
		});
	}

	#submit(value: UmbTreePickerDynamicRoot) {
		this.modalContext?.setValue(value);
		this.modalContext?.submit();
	}

	#originButtons = [
		{
			alias: 'Root',
			title: this.localize.term('dynamicRoot_originRootTitle'),
			description: this.localize.term('dynamicRoot_originRootDesc'),
			action: () => this.#choose('Root'),
		},
		{
			alias: 'Parent',
			title: this.localize.term('dynamicRoot_originParentTitle'),
			description: this.localize.term('dynamicRoot_originParentDesc'),
			action: () => this.#choose('Parent'),
		},
		{
			alias: 'Current',
			title: this.localize.term('dynamicRoot_originCurrentTitle'),
			description: this.localize.term('dynamicRoot_originCurrentDesc'),
			action: () => this.#choose('Current'),
		},
		{
			alias: 'Site',
			title: this.localize.term('dynamicRoot_originSiteTitle'),
			description: this.localize.term('dynamicRoot_originSiteDesc'),
			action: () => this.#choose('Site'),
		},
		{
			alias: 'ByKey',
			title: this.localize.term('dynamicRoot_originByKeyTitle'),
			description: this.localize.term('dynamicRoot_originByKeyDesc'),
			action: () => this.#openDocumentPicker(),
		},
	];

	render() {
		return html`
			<umb-body-layout headline="${this.localize.term('dynamicRoot_pickDynamicRootOriginTitle')}">
				<div id="main">
					<uui-box>
						${map(
							this.#originButtons,
							(btn) => html`
								<uui-button @click=${btn.action} look="placeholder" label="${btn.title}">
									<h3>${btn.title}</h3>
									<p>${btn.description}</p>
								</uui-button>
							`,
						)}
					</uui-box>
				</div>
				<div slot="actions">
					<uui-button @click=${this.#close} look="default" label="${this.localize.term('general_close')}"></uui-button>
				</div>
			</umb-body-layout>
		`;
	}

	static styles = [
		UmbTextStyles,
		css`
			uui-box > uui-button {
				display: block;
				--uui-button-content-align: flex-start;
			}

			uui-box > uui-button:not(:last-of-type) {
				margin-bottom: var(--uui-size-space-5);
			}

			h3,
			p {
				text-align: left;
			}
		`,
	];
}

export default UmbDynamicRootOriginPickerModalModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-dynamic-root-origin-picker-modal': UmbDynamicRootOriginPickerModalModalElement;
	}
}
