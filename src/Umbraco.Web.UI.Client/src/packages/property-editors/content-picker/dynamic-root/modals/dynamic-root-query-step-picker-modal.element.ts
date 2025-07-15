import type { ManifestDynamicRootQueryStep } from '../dynamic-root.extension.js';
import type { UmbContentPickerDocumentRootQueryStepModalData } from './index.js';
import { customElement, html, ifDefined, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbDocumentTypePickerInputContext } from '@umbraco-cms/backoffice/document-type';
import { UmbId } from '@umbraco-cms/backoffice/id';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';

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

	async #choose(item: ManifestDynamicRootQueryStep) {
		await this.#documentTypePickerContext.openPicker({
			hideTreeRoot: true,
			pickableFilter: (x) => x.isElement === false,
		});

		const selectedItems = this.#documentTypePickerContext.getSelection();

		this.modalContext?.setValue({
			unique: UmbId.new(),
			alias: item.meta.queryStepAlias,
			anyOfDocTypeKeys: selectedItems,
		});

		this.modalContext?.submit();
	}

	#close() {
		this.modalContext?.reject();
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
										@open=${() => this.#choose(item)}>
									</umb-ref-item>
								`,
							)}
						</uui-ref-list>
					</uui-box>
				</div>
				<div slot="actions">
					<uui-button look="default" label=${this.localize.term('general_close')} @click=${this.#close}></uui-button>
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
