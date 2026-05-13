import type { UmbValueSummaryResolveResult, UmbValueSummaryResolver } from '@umbraco-cms/backoffice/value-summary';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbReferenceByUniqueAndType } from '@umbraco-cms/backoffice/models';
import { UmbDocumentItemRepository, UMB_DOCUMENT_ENTITY_TYPE } from '@umbraco-cms/backoffice/document';
import type { UmbDocumentItemModel } from '@umbraco-cms/backoffice/document';
import { UmbMediaItemRepository, UMB_MEDIA_ENTITY_TYPE } from '@umbraco-cms/backoffice/media';
import { UmbMemberItemRepository, UMB_MEMBER_ENTITY_TYPE } from '@umbraco-cms/backoffice/member';

type ContentPickerValue = Array<UmbReferenceByUniqueAndType> | undefined;

export class UmbContentPickerValueSummaryResolver
	extends UmbControllerBase
	implements UmbValueSummaryResolver<ContentPickerValue, Array<string>>
{
	#documentRepo = new UmbDocumentItemRepository(this);
	#mediaRepo = new UmbMediaItemRepository(this);
	#memberRepo = new UmbMemberItemRepository(this);

	async resolveValues(values: ReadonlyArray<ContentPickerValue>): Promise<UmbValueSummaryResolveResult<Array<string>>> {
		const allItems = values.flatMap((v) => v ?? []);

		const docKeys = [...new Set(allItems.filter((e) => e.type === UMB_DOCUMENT_ENTITY_TYPE).map((e) => e.unique))];
		const mediaKeys = [...new Set(allItems.filter((e) => e.type === UMB_MEDIA_ENTITY_TYPE).map((e) => e.unique))];
		const memberKeys = [...new Set(allItems.filter((e) => e.type === UMB_MEMBER_ENTITY_TYPE).map((e) => e.unique))];
		if (!docKeys.length && !mediaKeys.length && !memberKeys.length) {
			return { data: values.map(() => []) };
		}

		const [docResult, mediaResult, memberResult] = await Promise.all([
			docKeys.length ? this.#documentRepo.requestItems(docKeys) : Promise.resolve({ data: [] }),
			mediaKeys.length ? this.#mediaRepo.requestItems(mediaKeys) : Promise.resolve({ data: [] }),
			memberKeys.length ? this.#memberRepo.requestItems(memberKeys) : Promise.resolve({ data: [] }),
		]);

		const nameByKey = new Map<string, string>();

		for (const item of (Array.isArray(docResult.data) ? docResult.data : []) as Array<UmbDocumentItemModel>) {
			nameByKey.set(item.unique, item.variants[0]?.name ?? '');
		}
		for (const item of Array.isArray(mediaResult.data) ? mediaResult.data : []) {
			nameByKey.set((item as { unique: string }).unique, (item as { name: string }).name);
		}
		for (const item of Array.isArray(memberResult.data) ? memberResult.data : []) {
			nameByKey.set((item as { unique: string }).unique, (item as { name: string }).name);
		}

		return {
			data: values.map((v) => (v ?? []).map((e) => nameByKey.get(e.unique)).filter((n): n is string => !!n)),
		};
	}
}

export { UmbContentPickerValueSummaryResolver as valueResolver };
