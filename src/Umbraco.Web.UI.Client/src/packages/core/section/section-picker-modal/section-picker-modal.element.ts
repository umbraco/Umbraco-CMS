import type { UmbSectionPickerModalData, UmbSectionPickerModalValue } from './section-picker-modal.token.js';
import { html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbSelectionManager } from '@umbraco-cms/backoffice/utils';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import type { ManifestSection } from '../extensions/index.js';

@customElement('umb-section-picker-modal')
export class UmbSectionPickerModalElement extends UmbModalBaseElement<
	UmbSectionPickerModalData,
	UmbSectionPickerModalValue
> {
	@state()
	private _sections: Array<ManifestSection> = [];

	@state()
	private _selectable = false;

	#selectionManager = new UmbSelectionManager(this);

	constructor() {
		super();
		this.#selectionManager.setSelectable(true);
		this.observe(this.#selectionManager.selectable, (selectable) => (this._selectable = selectable), null);

		this.observe(
			umbExtensionsRegistry.byType('section'),
			(sections: Array<ManifestSection>) => (this._sections = sections),
			null,
		);
	}

	override connectedCallback(): void {
		super.connectedCallback();
		this.#selectionManager.setMultiple(this.data?.multiple ?? false);
		this.#selectionManager.setSelection(this.value?.selection ?? []);
	}

	#submit() {
		this.value = { selection: this.#selectionManager.getSelection() };
		this._submitModal();
	}

	override render() {
		return html`
			<umb-body-layout headline="Select sections">
				<uui-box>
					${this._sections.map(
						(item) => html`
							<uui-menu-item
								label=${this.localize.string(item.meta.label)}
								?selectable=${this._selectable}
								?selected=${this.#selectionManager.isSelected(item.alias)}
								@selected=${() => this.#selectionManager.select(item.alias)}
								@deselected=${() => this.#selectionManager.deselect(item.alias)}></uui-menu-item>
						`,
					)}
				</uui-box>
				<div slot="actions">
					<uui-button label="Close" @click=${this._rejectModal}></uui-button>
					<uui-button label="Submit" look="primary" color="positive" @click=${this.#submit}></uui-button>
				</div>
			</umb-body-layout>
		`;
	}
}

export default UmbSectionPickerModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-section-picker-modal': UmbSectionPickerModalElement;
	}
}
