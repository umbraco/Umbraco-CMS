import { UmbArrayState, UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';

export class UmbSelectionManager {
	#selection = new UmbArrayState(<Array<string | null>>[]);
	public readonly selection = this.#selection.asObservable();

	#multiple = new UmbBooleanState(false);
	public readonly multiple = this.#multiple.asObservable();

	public getSelection() {
		return this.#selection.getValue();
	}

	public setSelection(value: Array<string | null>) {
		if (value === undefined) throw new Error('Value cannot be undefined');
		this.#selection.next(value);
	}

	public getMultiple() {
		return this.#multiple.getValue();
	}

	public setMultiple(value: boolean) {
		this.#multiple.next(value);
	}

	public toggleSelect(unique: string | null) {
		this.isSelected(unique) ? this.deselect(unique) : this.select(unique);
	}

	public select(unique: string | null) {
		const newSelection = this.getMultiple() ? [...this.getSelection(), unique] : [unique];
		this.#selection.next(newSelection);
	}

	public deselect(unique: string | null) {
		const newSelection = this.getSelection().filter((x) => x !== unique);
		this.#selection.next(newSelection);
	}

	public isSelected(unique: string | null) {
		return this.getSelection().includes(unique);
	}

	public clearSelection() {
		this.#selection.next([]);
	}
}
