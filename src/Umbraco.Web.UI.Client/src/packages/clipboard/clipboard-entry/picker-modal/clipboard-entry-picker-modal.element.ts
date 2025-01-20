import type {
	UmbClipboardEntryPickerModalValue,
	UmbClipboardEntryPickerModalData,
} from './clipboard-entry-picker-modal.token.js';
import type { UmbSelectionChangeEvent } from '@umbraco-cms/backoffice/event';
import { html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';

@customElement('umb-clipboard-entry-picker-modal')
export class UmbClipboardEntryPickerModalElement extends UmbModalBaseElement<
	UmbClipboardEntryPickerModalData,
	UmbClipboardEntryPickerModalValue
> {
	#onSelectionChange(event: UmbSelectionChangeEvent) {
		// TODO: make interface for picker element
		const target = event.target as any;
		this.updateValue({ selection: target.selection });
	}

	#submit() {
		this.modalContext?.submit();
	}

	#close() {
		this.modalContext?.reject();
	}

	override render() {
		return html`<umb-body-layout headline="Clipboard">
			<uui-box>
				<umb-clipboard-entry-picker
					.selection=${this.value?.selection}
					.config=${this.data}
					@selection-change=${this.#onSelectionChange}></umb-clipboard-entry-picker>
			</uui-box>
			<div slot="actions">
				<uui-button label="Close" @click=${this.#close}></uui-button>
				<uui-button label="Submit" look="primary" color="positive" @click=${this.#submit}></uui-button>
			</div>
		</umb-body-layout> `;
	}
}

export { UmbClipboardEntryPickerModalElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-clipboard-entry-picker-modal': UmbClipboardEntryPickerModalElement;
	}
}
