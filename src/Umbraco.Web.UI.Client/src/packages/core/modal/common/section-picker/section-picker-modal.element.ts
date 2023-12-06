import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbSelectionManager } from '@umbraco-cms/backoffice/utils';
import { ManifestSection, umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import {
	UmbSectionPickerModalData,
	UmbSectionPickerModalValue,
	UmbModalBaseElement,
} from '@umbraco-cms/backoffice/modal';

@customElement('umb-section-picker-modal')
export class UmbSectionPickerModalElement extends UmbModalBaseElement<
	UmbSectionPickerModalData,
	UmbSectionPickerModalValue
> {
	@state()
	private _sections: Array<ManifestSection> = [];

	#selectionManager = new UmbSelectionManager();

	connectedCallback(): void {
		super.connectedCallback();

		// TODO: in theory this config could change during the lifetime of the modal, so we could observe it
		this.#selectionManager.setMultiple(this.data?.multiple ?? false);
		this.#selectionManager.setSelection(this.data?.selection ?? []);

		this.observe(
			umbExtensionsRegistry.extensionsOfType('section'),
			(sections: Array<ManifestSection>) => (this._sections = sections),
		),
			'umbSectionsObserver';
	}

	#submit() {
		this.value = {
			selection: this.#selectionManager.getSelection(),
		};
		this.modalContext?.submit();
	}

	#close() {
		this.modalContext?.reject();
	}

	render() {
		return html`
			<umb-body-layout headline="Select sections">
				<uui-box>
					${this._sections.map(
						(item) => html`
							<uui-menu-item
								label=${item.meta.label}
								selectable
								?selected=${this.#selectionManager.isSelected(item.alias)}
								@selected=${() => this.#selectionManager.select(item.alias)}
								@deselected=${() => this.#selectionManager.deselect(item.alias)}></uui-menu-item>
						`,
					)}
				</uui-box>
				<div slot="actions">
					<uui-button label="Close" @click=${this.#close}></uui-button>
					<uui-button label="Submit" look="primary" color="positive" @click=${this.#submit}></uui-button>
				</div>
			</umb-body-layout>
		`;
	}

	static styles = [UmbTextStyles, css``];
}

export default UmbSectionPickerModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-section-picker-modal': UmbSectionPickerModalElement;
	}
}
