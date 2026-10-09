import type { UmbMockTrackedReferenceItemModel } from '../data/mock-data-set.types.js';
import { umbDocumentMockDb } from './document.db.js';
import { umbDocumentTypeMockDb } from './document-type.db.js';
import { umbMediaMockDb } from './media.db.js';
import { umbMediaTypeMockDb } from './media-type.db.js';
import { umbMemberMockDb } from './member.db.js';
import { umbMemberTypeMockDb } from './member-type.db.js';
import type { PagedIReferenceResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

type UmbMockReferenceResponse = PagedIReferenceResponseModel['items'][number];

const toTypeReference = (type: { id: string; icon: string; alias: string; name: string }) => ({
	id: type.id,
	icon: type.icon,
	alias: type.alias,
	name: type.name,
});

/** References to entities that no longer exist are left out, as the server does. */
export const resolveTrackedReferences = (
	references: Array<UmbMockTrackedReferenceItemModel>,
): Array<UmbMockReferenceResponse> => {
	return references.flatMap((reference): Array<UmbMockReferenceResponse> => {
		switch (reference.$type) {
			case 'DocumentReferenceResponseModel': {
				const document = umbDocumentMockDb.read(reference.id);
				const documentType = document ? umbDocumentTypeMockDb.read(document.documentType.id) : undefined;
				if (!document || !documentType) return [];
				return [
					{
						$type: reference.$type,
						id: document.id,
						name: document.variants[0]?.name,
						published: document.variants.some((variant) => variant.state === 'Published'),
						documentType: toTypeReference(documentType),
						variants: document.variants.map((variant) => ({
							id: variant.id,
							name: variant.name,
							culture: variant.culture,
							state: variant.state,
							flags: variant.flags,
						})),
					},
				];
			}
			case 'MediaReferenceResponseModel': {
				const media = umbMediaMockDb.read(reference.id);
				const mediaType = media ? umbMediaTypeMockDb.read(media.mediaType.id) : undefined;
				if (!media || !mediaType) return [];
				return [
					{
						$type: reference.$type,
						id: media.id,
						name: media.variants[0]?.name,
						mediaType: toTypeReference(mediaType),
					},
				];
			}
			case 'MemberReferenceResponseModel': {
				const member = umbMemberMockDb.read(reference.id);
				const memberType = member ? umbMemberTypeMockDb.read(member.memberType.id) : undefined;
				if (!member || !memberType) return [];
				return [
					{
						$type: reference.$type,
						id: member.id,
						name: member.variants[0]?.name,
						memberType: toTypeReference(memberType),
					},
				];
			}
			default:
				return [reference];
		}
	});
};
