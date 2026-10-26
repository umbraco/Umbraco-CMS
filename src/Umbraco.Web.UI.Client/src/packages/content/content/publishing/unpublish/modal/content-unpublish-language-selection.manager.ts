import { UmbDeselectedEvent, UmbSelectedEvent, UmbSelectionChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbSelectionManager } from '@umbraco-cms/backoffice/utils';
import type { UmbEntityVariantOptionModel } from '@umbraco-cms/backoffice/variant';

/**
 * Selection of the languages to unpublish. Unpublishing a mandatory language takes every language offline, so selecting
 * one also selects every other pickable language, and deselecting it releases the languages selected on its behalf.
 * The cascade is applied before the selection is emitted, so every observer receives one consistent value per change.
 */
export class UmbContentUnpublishLanguageSelectionManager extends UmbSelectionManager<string> {
	#options: Array<UmbEntityVariantOptionModel> = [];
	#isPickable: (option: UmbEntityVariantOptionModel) => boolean = () => true;
	#autoSelected: Array<string> = [];

	/**
	 * Sets the language options the cascade selects from.
	 * @param {Array<UmbEntityVariantOptionModel>} options - The options shown for selection.
	 * @param {(option: UmbEntityVariantOptionModel) => boolean} isPickable - Whether the current user may pick an option.
	 */
	public setOptions(
		options: Array<UmbEntityVariantOptionModel>,
		isPickable: (option: UmbEntityVariantOptionModel) => boolean,
	) {
		this.#options = options;
		this.#isPickable = isPickable;
	}

	public override setSelection(value: Array<string>) {
		super.setSelection(this.#applyMandatoryCascade(this.getSelection(), value));
	}

	public override select(unique: string) {
		if (this.getSelectable() === false || this.isSelected(unique)) return;
		this.setSelection([...this.getSelection(), unique]);
		this.getHostElement().dispatchEvent(new UmbSelectedEvent(unique));
		this.getHostElement().dispatchEvent(new UmbSelectionChangeEvent());
	}

	public override deselect(unique: string) {
		if (this.getSelectable() === false) return;
		this.setSelection(this.getSelection().filter((x) => x !== unique));
		this.getHostElement().dispatchEvent(new UmbDeselectedEvent(unique));
		this.getHostElement().dispatchEvent(new UmbSelectionChangeEvent());
	}

	public override clearSelection() {
		this.setSelection([]);
	}

	#applyMandatoryCascade(previous: Array<string>, next: Array<string>): Array<string> {
		const isMandatory = (unique: string) => this.#options.some((o) => o.unique === unique && o.language.isMandatory);

		if (previous.some((unique) => isMandatory(unique) && !next.includes(unique))) {
			const autoSelected = this.#autoSelected;
			this.#autoSelected = [];
			return next.filter((unique) => !autoSelected.includes(unique));
		}

		if (!next.some(isMandatory)) return next;

		const missing = this.#options.filter((o) => this.#isPickable(o) && !next.includes(o.unique)).map((o) => o.unique);
		this.#autoSelected = [...new Set([...this.#autoSelected, ...missing])];
		return [...next, ...missing];
	}
}
