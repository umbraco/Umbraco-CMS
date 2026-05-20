import type { UmbValueSummaryResolveResult, UmbValueSummaryResolver } from '@umbraco-cms/backoffice/value-summary';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbReferenceByUniqueAndType } from '@umbraco-cms/backoffice/models';
import { UmbDocumentItemRepository, UMB_DOCUMENT_ENTITY_TYPE } from '@umbraco-cms/backoffice/document';
import type { UmbDocumentItemModel } from '@umbraco-cms/backoffice/document';
import { UmbMediaItemRepository, UMB_MEDIA_ENTITY_TYPE } from '@umbraco-cms/backoffice/media';
import type { UmbMediaItemModel } from '@umbraco-cms/backoffice/media';
import { UmbMemberItemRepository, UMB_MEMBER_ENTITY_TYPE } from '@umbraco-cms/backoffice/member';
import type { UmbMemberItemModel } from '@umbraco-cms/backoffice/member';

type ContentPickerValue = Array<UmbReferenceByUniqueAndType> | undefined;

export type UmbContentPickerResolvedItem =
	| { entityType: typeof UMB_DOCUMENT_ENTITY_TYPE; item: UmbDocumentItemModel }
	| { entityType: typeof UMB_MEDIA_ENTITY_TYPE; item: UmbMediaItemModel }
	| { entityType: typeof UMB_MEMBER_ENTITY_TYPE; item: UmbMemberItemModel };

export class UmbContentPickerValueSummaryResolver
	extends UmbControllerBase
	implements UmbValueSummaryResolver<ContentPickerValue, Array<UmbContentPickerResolvedItem>>
{
	#documentRepo = new UmbDocumentItemRepository(this);
	#mediaRepo = new UmbMediaItemRepository(this);
	#memberRepo = new UmbMemberItemRepository(this);

	async resolveValues(
		values: ReadonlyArray<ContentPickerValue>,
	): Promise<UmbValueSummaryResolveResult<Array<UmbContentPickerResolvedItem>>> {
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

		const docsByKey = new Map(
			(Array.isArray(docResult.data) ? (docResult.data as Array<UmbDocumentItemModel>) : []).map((item) => [
				item.unique,
				item,
			]),
		);
		const mediaByKey = new Map(
			(Array.isArray(mediaResult.data) ? (mediaResult.data as Array<UmbMediaItemModel>) : []).map((item) => [
				item.unique,
				item,
			]),
		);
		const memberByKey = new Map(
			(Array.isArray(memberResult.data) ? (memberResult.data as Array<UmbMemberItemModel>) : []).map((item) => [
				item.unique,
				item,
			]),
		);

		return {
			data: values.map((v) =>
				(v ?? []).flatMap((e): Array<UmbContentPickerResolvedItem> => {
					if (e.type === UMB_DOCUMENT_ENTITY_TYPE) {
						const item = docsByKey.get(e.unique);
						return item ? [{ entityType: UMB_DOCUMENT_ENTITY_TYPE, item }] : [];
					}
					if (e.type === UMB_MEDIA_ENTITY_TYPE) {
						const item = mediaByKey.get(e.unique);
						return item ? [{ entityType: UMB_MEDIA_ENTITY_TYPE, item }] : [];
					}
					if (e.type === UMB_MEMBER_ENTITY_TYPE) {
						const item = memberByKey.get(e.unique);
						return item ? [{ entityType: UMB_MEMBER_ENTITY_TYPE, item }] : [];
					}
					return [];
				}),
			),
		};
	}
}

export { UmbContentPickerValueSummaryResolver as valueResolver };
