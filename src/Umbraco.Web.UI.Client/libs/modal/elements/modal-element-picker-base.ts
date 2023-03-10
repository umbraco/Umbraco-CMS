import { property } from 'lit/decorators.js';
import { UmbModalBaseElement } from '..';
import './modal-element.element';

export interface UmbPickerModalData<T> {
	multiple: boolean;
	selection: Array<string>;
	filter?: (language: T) => boolean;
}

export interface UmbPickerModalResult<T> {
	selection: Array<string>;
}

// TODO: we should consider moving this into a class/context instead of an element.
// So we don't have to extend an element to get basic picker/selection logic
export class UmbModalElementPickerBase<T> extends UmbModalBaseElement<UmbPickerModalData<T>, UmbPickerModalResult<T>> {
	@property()
	selection: Array<string> = [];

	connectedCallback(): void {
		super.connectedCallback();
		this.selection = this.data?.selection || [];
	}

	submit() {
		this.modalHandler?.submit({ selection: this.selection });
	}

	close() {
		this.modalHandler?.reject();
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
