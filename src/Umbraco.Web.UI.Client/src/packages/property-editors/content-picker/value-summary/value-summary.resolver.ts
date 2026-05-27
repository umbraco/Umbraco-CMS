import type { UmbValueSummaryResolveResult, UmbValueSummaryResolver } from '@umbraco-cms/backoffice/value-summary';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbReferenceByUniqueAndType } from '@umbraco-cms/backoffice/models';
import { UmbDocumentItemRepository, UMB_DOCUMENT_ENTITY_TYPE } from '@umbraco-cms/backoffice/document';
import type { UmbDocumentItemModel } from '@umbraco-cms/backoffice/document';
import { UmbMediaItemRepository, UMB_MEDIA_ENTITY_TYPE } from '@umbraco-cms/backoffice/media';
import type { UmbMediaItemModel } from '@umbraco-cms/backoffice/media';
import { UmbMemberItemRepository, UMB_MEMBER_ENTITY_TYPE } from '@umbraco-cms/backoffice/member';
import type { UmbMemberItemModel } from '@umbraco-cms/backoffice/member';
import { combineLatest, map, of, type Observable } from '@umbraco-cms/backoffice/external/rxjs';

type ContentPickerValue = Array<UmbReferenceByUniqueAndType> | undefined;

export type UmbContentPickerResolvedItem = UmbDocumentItemModel | UmbMediaItemModel | UmbMemberItemModel;

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
			docKeys.length ? this.#documentRepo.requestItems(docKeys) : Promise.resolve(undefined),
			mediaKeys.length ? this.#mediaRepo.requestItems(mediaKeys) : Promise.resolve(undefined),
			memberKeys.length ? this.#memberRepo.requestItems(memberKeys) : Promise.resolve(undefined),
		]);

		const docs = (Array.isArray(docResult?.data) ? docResult?.data : []) as Array<UmbDocumentItemModel>;
		const media = (Array.isArray(mediaResult?.data) ? mediaResult?.data : []) as Array<UmbMediaItemModel>;
		const members = (Array.isArray(memberResult?.data) ? memberResult?.data : []) as Array<UmbMemberItemModel>;

		const docObs: Observable<Array<UmbDocumentItemModel>> = docResult?.asObservable?.() ?? of(docs);
		const mediaObs: Observable<Array<UmbMediaItemModel>> = mediaResult?.asObservable?.() ?? of(media);
		const memberObs: Observable<Array<UmbMemberItemModel>> = memberResult?.asObservable?.() ?? of(members);

		return {
			data: this.#map(values, docs, media, members),
			asObservable: () =>
				combineLatest([docObs, mediaObs, memberObs]).pipe(
					map(([documents, mediaItems, memberItems]) => this.#map(values, documents, mediaItems, memberItems)),
				),
		};
	}

	#map(
		values: ReadonlyArray<ContentPickerValue>,
		docs: ReadonlyArray<UmbDocumentItemModel>,
		media: ReadonlyArray<UmbMediaItemModel>,
		members: ReadonlyArray<UmbMemberItemModel>,
	): ReadonlyArray<Array<UmbContentPickerResolvedItem>> {
		const docsByKey = new Map(docs.map((item) => [item.unique, item]));
		const mediaByKey = new Map(media.map((item) => [item.unique, item]));
		const memberByKey = new Map(members.map((item) => [item.unique, item]));

		return values.map((value) =>
			(value ?? []).flatMap((entry): Array<UmbContentPickerResolvedItem> => {
				if (entry.type === UMB_DOCUMENT_ENTITY_TYPE) {
					const item = docsByKey.get(entry.unique);
					return item ? [item] : [];
				}
				if (entry.type === UMB_MEDIA_ENTITY_TYPE) {
					const item = mediaByKey.get(entry.unique);
					return item ? [item] : [];
				}
				if (entry.type === UMB_MEMBER_ENTITY_TYPE) {
					const item = memberByKey.get(entry.unique);
					return item ? [item] : [];
				}
				return [];
			}),
		);
	}
}
