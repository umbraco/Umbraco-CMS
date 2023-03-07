import { property } from 'lit/decorators.js';
import { UmbModalLayoutElement } from '..';

export interface UmbPickerModalData<T> {
	multiple: boolean;
	selection: Array<string>;
	filter?: (language: T) => boolean;
}

// TODO: we should consider moving this into a class/context instead of an element.
// So we don't have to extend an element to get basic picker/selection logic
export class UmbModalLayoutPickerBase<T> extends UmbModalLayoutElement<UmbPickerModalData<T>> {
	@property()
	selection: Array<string> = [];

	connectedCallback(): void {
		super.connectedCallback();
		this.selection = this.data?.selection || [];
	}

	submit() {
		this.modalHandler?.close({ selection: this.selection });
	}

	close() {
		this.modalHandler?.close();
	}

	protected _handleKeydown(e: KeyboardEvent, key: string) {
		if (e.key === 'Enter') {
			this.handleSelection(key);
		}
	}

	/* TODO: Write test for this select/deselect method. */
	handleSelection(key: string) {
		if (this.data?.multiple) {
			if (this.isSelected(key)) {
				this.selection = this.selection.filter((selectedKey) => selectedKey !== key);
			} else {
				this.selection.push(key);
			}
		} else {
			this.selection = [key];
		}

		this.requestUpdate('_selection');
	}

	isSelected(key: string): boolean {
		return this.selection.includes(key);
	}
}
