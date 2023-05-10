import { Observable } from 'rxjs';
import { UmbArrayState, UmbBooleanState } from '../observable-api';

export interface UmbSelectionManager {
	selection: Observable<Array<string | null>>;
	multiple: Observable<boolean>;

	getSelection(): Array<string | null>;
	setSelection(value: Array<string | null>): void;

	getMultiple(): boolean;
	setMultiple(value: boolean): void;

	toggleSelect(unique: string | null): void;
	select(unique: string | null): void;
	deselect(unique: string | null): void;
	isSelected(unique: string | null): boolean;
}

export class UmbSelectionManagerBase implements UmbSelectionManager {
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
}
