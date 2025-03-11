import type { UmbContentPickerDynamicRootQueryStep } from '../../types.js';
import type { ManifestDynamicRootQueryStep } from '../dynamic-root.extension.js';
import type { UmbContentPickerDocumentRootQueryStepModalData } from './index.js';
import { UmbDocumentTypePickerInputContext } from '@umbraco-cms/backoffice/document-type';
import { UmbId } from '@umbraco-cms/backoffice/id';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { html, customElement, state, ifDefined, repeat } from '@umbraco-cms/backoffice/external/lit';

@customElement('umb-dynamic-root-query-step-picker-modal')
export class UmbDynamicRootQueryStepPickerModalModalElement extends UmbModalBaseElement<UmbContentPickerDocumentRootQueryStepModalData> {
	@state()
	private _querySteps: Array<ManifestDynamicRootQueryStep> = [];

	#documentTypePickerContext = new UmbDocumentTypePickerInputContext(this);

	override connectedCallback() {
		super.connectedCallback();

		if (this.data) {
			this._querySteps = this.data.items;
		}
	}

	#choose(item: ManifestDynamicRootQueryStep) {
		this.#openDocumentTypePicker(item.meta.queryStepAlias);
	}

	#close() {
		this.modalContext?.reject();
	}

	async #openDocumentTypePicker(alias: string) {
		await this.#documentTypePickerContext.openPicker({
			hideTreeRoot: true,
			pickableFilter: (x) => x.isElement === false,
		});

		const selectedItems = this.#documentTypePickerContext.getSelection();

		this.#submit({
			unique: UmbId.new(),
			alias: alias,
			anyOfDocTypeKeys: selectedItems,
		});
	}

	#submit(value: UmbContentPickerDynamicRootQueryStep) {
		this.modalContext?.setValue(value);
		this.modalContext?.submit();
	}

	override render() {
		return html`
			<umb-body-layout headline=${this.localize.term('dynamicRoot_pickDynamicRootQueryStepTitle')}>
				<div id="main">
					<uui-box>
						<uui-ref-list>
							${repeat(
								this._querySteps,
								(item) => item.alias,
								(item) => html`
									<umb-ref-item
										name=${ifDefined(item.meta.label)}
										detail=${ifDefined(item.meta.description)}
										icon=${ifDefined(item.meta.icon)}
										@open=${() => this.#choose(item)}></umb-ref-item>
								`,
							)}
						</uui-ref-list>
					</uui-box>
				</div>
				<div slot="actions">
					<uui-button @click=${this.#close} look="default" label="${this.localize.term('general_close')}"></uui-button>
				</div>
			</umb-body-layout>
		`;
	}
}

export default UmbDynamicRootQueryStepPickerModalModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-dynamic-root-query-step-picker-modal': UmbDynamicRootQueryStepPickerModalModalElement;
	}
}
