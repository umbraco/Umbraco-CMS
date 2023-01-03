import { state } from 'lit/decorators.js';
import { UmbModalLayoutElement } from '..';

export interface UmbPickerData<selectType = string> {
	multiple: boolean;
	selection: Array<selectType>;
}

export class UmbModalLayoutPickerBase<selectType = string> extends UmbModalLayoutElement<UmbPickerData<selectType>> {
	@state()
	private _selection: Array<selectType> = [];

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

	protected _handleKeydown(e: KeyboardEvent, key: selectType) {
		if (e.key === 'Enter') {
			this._handleItemClick(key);
		}
	}

	/* TODO: Write test for this select/deselect method. */
	protected _handleItemClick(key: selectType) {
		if (this.data?.multiple) {
			if (this._isSelected(key)) {
				this._selection = this._selection.filter((selectedKey) => selectedKey !== key);
			} else {
				this._selection.push(key);
			}
		} else {
			this._selection = [key];
		}

		this.requestUpdate('_selection');
	}

	protected _isSelected(key: selectType): boolean {
		return this._selection.includes(key);
	}
}
