import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbClipboardEntryDetailRepository, type UmbClipboardPasteResolver } from '@umbraco-cms/backoffice/clipboard';

export class UmbBlockListClipboardCopyResolver extends UmbControllerBase implements UmbClipboardPasteResolver {
	#detailRepository = new UmbClipboardEntryDetailRepository(this);

	async getAcceptedTypes(): Promise<string[]> {
		return ['blockList'];
	}

	async resolve(unique: string) {
		const { data: entry } = await this.#detailRepository.requestByUnique(unique);
		return entry;
	}
}

export { UmbBlockListClipboardCopyResolver as api };
