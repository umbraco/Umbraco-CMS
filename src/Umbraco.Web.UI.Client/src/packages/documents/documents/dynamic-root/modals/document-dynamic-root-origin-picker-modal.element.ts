import type { ManifestDynamicRootOrigin } from '../dynamic-root.extension.js';
import type { UmbDynamicRoot } from '../types.js';
import { UmbDocumentPickerInputContext } from '../../components/input-document/input-document.context.js';
import type { UmbDocumentDynamicRootOriginPickerModalData } from './index.js';
import { customElement, html, ifDefined, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';

@customElement('umb-document-dynamic-root-origin-picker-modal')
export class UmbDocumentDynamicRootOriginPickerModalElement extends UmbModalBaseElement<UmbDocumentDynamicRootOriginPickerModalData> {
	@state()
	private _origins: Array<ManifestDynamicRootOrigin> = [];

	#documentPickerContext = new UmbDocumentPickerInputContext(this);

	constructor() {
		super();

		this.#documentPickerContext.max = 1;
	}

	override connectedCallback() {
		super.connectedCallback();

		if (this.data) {
			this._origins = this.data.items;
		}
	}

	#choose(item: ManifestDynamicRootOrigin) {
		switch (item.meta.originAlias) {
			// NOTE: Edge-case. Currently this is the only one that uses a document picker,
			// but other custom origins may want other configuration options. [LK:2024-01-25]
			case 'ByKey':
				this.#openDocumentPicker(item.meta.originAlias);
				break;
			default:
				this.#submit({ originAlias: item.meta.originAlias });
				break;
		}
	}

	#close() {
		this.modalContext?.reject();
	}

	async #openDocumentPicker(originAlias: string) {
		await this.#documentPickerContext.openPicker({
			hideTreeRoot: true,
		});

		const selectedItems = this.#documentPickerContext.getSelection();
		if (selectedItems.length !== 1) return;

		this.#submit({
			originAlias,
			originKey: selectedItems[0],
		});
	}

	#submit(value: UmbDynamicRoot) {
		this.modalContext?.setValue(value);
		this.modalContext?.submit();
	}

	override render() {
		return html`
			<umb-body-layout headline=${this.localize.term('dynamicRoot_pickDynamicRootOriginTitle')}>
				<div id="main">
					<uui-box>
						<uui-ref-list>
							${repeat(
								this._origins,
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

export default UmbDocumentDynamicRootOriginPickerModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-dynamic-root-origin-picker-modal': UmbDocumentDynamicRootOriginPickerModalElement;
	}
}
