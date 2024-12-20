import { UMB_BLOCK_LIST_PROPERTY_EDITOR_SCHEMA_ALIAS } from '../../property-editors/block-list-editor/constants.js';
import { UmbPropertyValueCloneController } from '@umbraco-cms/backoffice/property';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import {
	UmbClipboardEntryDetailRepository,
	UmbPasteClipboardEntryTranslateController,
	type UmbClipboardPasteResolver,
} from '@umbraco-cms/backoffice/clipboard';

export class UmbBlockListClipboardPasteResolver extends UmbControllerBase implements UmbClipboardPasteResolver {
	#detailRepository = new UmbClipboardEntryDetailRepository(this);

	async getAcceptedTypes(): Promise<string[]> {
		return ['block'];
	}

	async resolve(unique: string) {
		const { data: entry } = await this.#detailRepository.requestByUnique(unique);

		if (!entry) {
			throw new Error(`Could not find clipboard entry with unique id: ${unique}`);
		}

		const acceptedTypes = await this.getAcceptedTypes();

		if (acceptedTypes.includes(entry.type) === false) {
			throw new Error(`Clipboard entry type "${entry.type}" is not supported by this resolver.`);
		}

		let propertyValue = undefined;

		if (entry.type === 'block') {
			const translator = new UmbPasteClipboardEntryTranslateController(this);
			propertyValue = await translator.translate(entry);
		}

		const cloner = new UmbPropertyValueCloneController(this);
		const clonedValue = await cloner.clone({
			editorAlias: UMB_BLOCK_LIST_PROPERTY_EDITOR_SCHEMA_ALIAS,
			value: propertyValue,
		});

		propertyValue = clonedValue.value;

		return propertyValue;
	}
}

export { UmbBlockListClipboardPasteResolver as api };
