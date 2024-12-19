import { UMB_BLOCK_LIST_PROPERTY_EDITOR_SCHEMA_ALIAS } from '../../property-editors/block-list-editor/constants.js';
import type { UmbBlockListLayoutModel } from '../../types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbClipboardEntryDetailRepository, type UmbClipboardPasteResolver } from '@umbraco-cms/backoffice/clipboard';

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

		if (entry.type === 'block') {
			return this.#transformBlockValue(entry.value);
		}

		return undefined;
	}

	#transformBlockValue(value: any) {
		if (!value) {
			throw new Error('Clipboard entry value is missing.');
		}

		const clone = structuredClone(value);

		const transformedValue = {
			contentData: clone.contentData,
			settingsData: clone.settingsData,
			expose: clone.expose,
			layout: clone.layout
				? {
						[UMB_BLOCK_LIST_PROPERTY_EDITOR_SCHEMA_ALIAS]: clone.layout.map((layout: UmbBlockListLayoutModel) => {
							return {
								...layout,
								$type: 'BlockListLayoutItem',
							};
						}),
					}
				: [],
		};

		return transformedValue;
	}
}

export { UmbBlockListClipboardPasteResolver as api };
