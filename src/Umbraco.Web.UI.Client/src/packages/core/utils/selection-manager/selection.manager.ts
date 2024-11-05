import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbDeselectedEvent, UmbSelectedEvent, UmbSelectionChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbArrayState, UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';

/**
 * Manages the selection of items.
 * @class UmbSelectionManager
 */
export class UmbSelectionManager<ValueType extends string | null = string | null> extends UmbControllerBase {
	#selectable = new UmbBooleanState(true);
	public readonly selectable = this.#selectable.asObservable();

	#selection = new UmbArrayState(<Array<ValueType>>[], (x) => x);
	public readonly selection = this.#selection.asObservable();
	public readonly hasSelection = this.#selection.asObservablePart((x) => x.length > 0);

	#multiple = new UmbBooleanState(false);
	public readonly multiple = this.#multiple.asObservable();

	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	#allow = (unique: ValueType) => true;

	constructor(host: UmbControllerHost) {
		super(host);
	}

	/**
	 * Returns whether items can be selected.
	 * @returns {*}
	 * @memberof UmbSelectionManager
	 */
	public getSelectable() {
		return this.#selectable.getValue();
	}

	/**
	 * Sets whether items can be selected.
	 * @param {boolean} value
	 * @memberof UmbSelectionManager
	 */
	public setSelectable(value: boolean) {
		this.#selectable.setValue(value);
	}

	/**
	 * Returns the current selection.
	 * @returns {*}
	 * @memberof UmbSelectionManager
	 */
	public getSelection() {
		return this.#selection.getValue();
	}

	/**
	 * Sets the current selection.
	 * @param {Array<ValueType>} value
	 * @memberof UmbSelectionManager
	 */
	public setSelection(value: Array<ValueType>) {
		if (this.getSelectable() === false) return;
		if (value === undefined) throw new Error('Value cannot be undefined');

		value.forEach((unique) => {
			if (this.#allow(unique) === false) {
				throw new Error(`${unique} is now allowed to be selected`);
			}
		});

		const newSelection = this.getMultiple() ? value : value.slice(0, 1);
		this.#selection.setValue(newSelection);
	}

	/**
	 * Returns whether multiple items can be selected.
	 * @returns {*}
	 * @memberof UmbSelectionManager
	 */
	public getMultiple() {
		return this.#multiple.getValue();
	}

	/**
	 * Sets whether multiple items can be selected.
	 * @param {boolean} value
	 * @memberof UmbSelectionManager
	 */
	public setMultiple(value: boolean) {
		this.#multiple.setValue(value);

		/* If multiple is set to false, and the current selection is more than one,
		then we need to set the selection to the first item. */
		if (value === false && this.getSelection().length > 1) {
			const first = this.getSelection()[0];
			this.clearSelection();
			this.select(first);
		}
	}

	/**
	 * Toggles the given unique id in the current selection.
	 * @param {(ValueType)} unique
	 * @memberof UmbSelectionManager
	 */
	public toggleSelect(unique: ValueType) {
		if (this.getSelectable() === false) return;
		if (this.isSelected(unique)) {
			this.deselect(unique);
		} else {
			this.select(unique);
		}
	}

	/**
	 * Appends the given unique id to the current selection.
	 * @param {(ValueType)} unique
	 * @memberof UmbSelectionManager
	 */
	public select(unique: ValueType) {
		if (this.getSelectable() === false) return;
		if (this.isSelected(unique)) return;
		if (this.#allow(unique) === false) {
			throw new Error('The item is now allowed to be selected');
		}
		const newSelection = this.getMultiple() ? [...this.getSelection(), unique] : [unique];
		this.#selection.setValue(newSelection);
		this.getHostElement().dispatchEvent(new UmbSelectedEvent(unique));
		this.getHostElement().dispatchEvent(new UmbSelectionChangeEvent());
	}

	/**
	 * Removes the given unique id from the current selection.
	 * @param {(ValueType)} unique
	 * @memberof UmbSelectionManager
	 */
	public deselect(unique: ValueType) {
		if (this.getSelectable() === false) return;
		const newSelection = this.getSelection().filter((x) => x !== unique);
		this.#selection.setValue(newSelection);
		this.getHostElement().dispatchEvent(new UmbDeselectedEvent(unique));
		this.getHostElement().dispatchEvent(new UmbSelectionChangeEvent());
	}

	/**
	 * Returns true if the given unique id is selected.
	 * @param {(ValueType)} unique
	 * @returns {*}
	 * @memberof UmbSelectionManager
	 */
	public isSelected(unique: ValueType) {
		return this.getSelection().includes(unique);
	}

	/**
	 * Clears the current selection.
	 * @memberof UmbSelectionManager
	 */
	public clearSelection() {
		if (this.getSelectable() === false) return;
		this.#selection.setValue([]);
	}

	/**
	 * Sets a function that determines if an item is selectable or not.
	 * @param compareFn A function that determines if an item is selectable or not.
	 * @memberof UmbSelectionManager
	 */
	public setAllowLimitation(compareFn: (unique: ValueType) => boolean): void {
		this.#allow = compareFn;
	}

	/**
	 * Returns the function that determines if an item is selectable or not.
	 * @returns {*}
	 * @memberof UmbSelectionManager
	 */
	public getAllowLimitation() {
		return this.#allow;
	}
}
