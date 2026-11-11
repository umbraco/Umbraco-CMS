import { UmbSelectionManager } from '@umbraco-cms/backoffice/utils';
import type { UmbEntityVariantOptionModel } from '@umbraco-cms/backoffice/variant';

/**
 * Selection of the languages to unpublish. Unpublishing a mandatory language takes every language offline, so selecting
 * one also selects every other pickable language, and those stay selected while it is. Deselecting it deselects every
 * mandatory language and releases the languages selected on its behalf.
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

	/**
	 * Sets the selection after applying the mandatory language cascade to it.
	 * @param {Array<string>} value - The requested selection of language uniques.
	 */
	public override setSelection(value: Array<string>) {
		super.setSelection(this.#applyMandatoryCascade(this.getSelection(), value));
	}

	#applyMandatoryCascade(previous: Array<string>, next: Array<string>): Array<string> {
		const isMandatory = (unique: string) => this.#options.some((o) => o.unique === unique && o.language.isMandatory);

		const autoSelected = this.#autoSelected;
		const releasesMandatory = previous.some(
			(unique) => isMandatory(unique) && !autoSelected.includes(unique) && !next.includes(unique),
		);
		if (releasesMandatory) {
			this.#autoSelected = [];
			return next.filter((unique) => !isMandatory(unique) && !autoSelected.includes(unique));
		}

		if (!next.some(isMandatory)) return next;

		const missing = this.#options.filter((o) => this.#isPickable(o) && !next.includes(o.unique)).map((o) => o.unique);
		this.#autoSelected = [...new Set([...this.#autoSelected, ...missing])];
		return [...next, ...missing];
	}
}
