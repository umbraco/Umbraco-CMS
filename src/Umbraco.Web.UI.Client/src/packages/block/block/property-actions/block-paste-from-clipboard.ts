import type { UmbBlockExposeModel, UmbBlockValueDataPropertiesBaseType } from '../types.js';
import { UmbPasteFromClipboardPropertyAction } from '@umbraco-cms/backoffice/clipboard';

/**
 * Paste From Clipboard Property Action for Block property editors.
 *
 * A clipboard entry carries no `expose` entries, because whether a block is created is a property of
 * the variant it is pasted into, not of the copied block. Without them the pasted blocks land
 * uncreated and each one has to be created by hand before it can be published (#20327, #21855).
 * @exports
 * @class UmbBlockPasteFromClipboardPropertyAction
 * @augments UmbPasteFromClipboardPropertyAction
 */
export class UmbBlockPasteFromClipboardPropertyAction extends UmbPasteFromClipboardPropertyAction {
	/**
	 * Exposes every pasted block for the variant being edited, mirroring what creating the block by
	 * hand would have done. Other variants are deliberately left untouched, so a paste never creates
	 * content in a language the editor is not working in.
	 * @param {UmbBlockValueDataPropertiesBaseType} value The translated block property value.
	 * @returns {Promise<UmbBlockValueDataPropertiesBaseType>} The value with `expose` populated.
	 * @protected
	 * @memberof UmbBlockPasteFromClipboardPropertyAction
	 */
	protected override async _prepareValue(
		value: UmbBlockValueDataPropertiesBaseType,
	): Promise<UmbBlockValueDataPropertiesBaseType> {
		if (!value?.contentData?.length) return value;

		const variantId = this._propertyContext?.getVariantId();
		const culture = variantId?.culture ?? null;
		const segment = variantId?.segment ?? null;

		const expose: Array<UmbBlockExposeModel> = value.contentData.map((content) => ({
			contentKey: content.key,
			culture,
			segment,
		}));

		return { ...value, expose };
	}
}

export { UmbBlockPasteFromClipboardPropertyAction as api };
