import { state } from 'lit/decorators.js';
import { UmbModalLayoutElement } from '../../../core/services/modal/layouts/modal-layout.element';

import { UmbPickerData } from './picker';

export class UmbPickerLayout extends UmbModalLayoutElement<UmbPickerData> {
	@state()
	private _selection: Array<string> = [];

	connectedCallback(): void {
		super.connectedCallback();
		this._selection = this.data?.selection || [];
	}

	protected _submit() {
		this.modalHandler?.close({ selection: this._selection });
	}

	protected _close() {
		this.modalHandler?.close();
	}

	protected _handleKeydown(e: KeyboardEvent, key: string) {
		if (e.key === 'Enter') {
			this._handleItemClick(key);
		}
	}

	protected _handleItemClick(clickedKey: string) {
		if (this.data?.multiple) {
			if (this._isSelected(clickedKey)) {
				this._selection = this._selection.filter((key) => key !== clickedKey);
			} else {
				this._selection.push(clickedKey);
			}
		} else {
			this._selection = [clickedKey];
		}

		this.requestUpdate('_selection');
	}

	protected _isSelected(key: string): boolean {
		return this._selection.includes(key);
	}
}
