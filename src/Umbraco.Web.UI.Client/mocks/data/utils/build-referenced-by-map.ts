import type {
	UmbMockDocumentModel,
	UmbMockDocumentTypeModel,
	UmbMockElementModel,
	UmbMockMediaModel,
	UmbMockMediaTypeModel,
	UmbMockTrackedReferenceItemModel,
} from '../mock-data-set.types.js';

export interface UmbBuildReferencedByMapArgs {
	document?: Array<UmbMockDocumentModel>;
	element?: Array<UmbMockElementModel>;
	media?: Array<UmbMockMediaModel>;
	documentType?: Array<UmbMockDocumentTypeModel>;
	mediaType?: Array<UmbMockMediaTypeModel>;
}

function collectMatchingIds(value: unknown, knownIds: ReadonlySet<string>, found: Set<string>): void {
	if (typeof value === 'string') {
		if (knownIds.has(value)) found.add(value);
	} else if (Array.isArray(value)) {
		for (const item of value) collectMatchingIds(item, knownIds, found);
	} else if (value && typeof value === 'object') {
		for (const item of Object.values(value)) collectMatchingIds(item, knownIds, found);
	}
}

/**
 * Builds a map of target id to the items referencing it, by scanning every document property value
 * for strings that equal the id of a known document or media item.
 * Nested values (for example block data) are scanned too.
 * @param args The mock entities to scan and to look type information up in.
 * @returns The referencing items, keyed by the id of the item they reference.
 */
export function buildReferencedByMap(
	args: UmbBuildReferencedByMapArgs,
): Record<string, Array<UmbMockTrackedReferenceItemModel>> {
	const documents = args.document ?? [];
	const elements = (args.element ?? []).filter((x) => !x.isFolder && !x.isTrashed);
	const media = args.media ?? [];
	const documentTypes = new Map((args.documentType ?? []).map((x) => [x.id, x]));
	const mediaTypes = new Map((args.mediaType ?? []).map((x) => [x.id, x]));

	const knownIds = new Set<string>([
		...documents.map((x) => x.id),
		...elements.map((x) => x.id),
		...media.map((x) => x.id),
	]);
	const map: Record<string, Array<UmbMockTrackedReferenceItemModel>> = {};

	for (const doc of documents) {
		const targetIds = new Set<string>();
		collectMatchingIds(doc.values, knownIds, targetIds);
		targetIds.delete(doc.id);
		if (!targetIds.size) continue;

		const type = documentTypes.get(doc.documentType.id);
		const item: UmbMockTrackedReferenceItemModel = {
			$type: 'DocumentReferenceResponseModel',
			id: doc.id,
			name: doc.variants[0]?.name,
			published: doc.variants.some((v) => v.state === 'Published'),
			documentType: { id: doc.documentType.id, icon: doc.documentType.icon, alias: type?.alias, name: type?.name },
			variants: doc.variants.map((v) => ({
				id: v.id,
				flags: v.flags,
				state: v.state,
				name: v.name,
				culture: v.culture,
			})),
		};

		for (const targetId of targetIds) (map[targetId] ??= []).push(item);
	}

	for (const el of elements) {
		const targetIds = new Set<string>();
		collectMatchingIds(el.values, knownIds, targetIds);
		targetIds.delete(el.id);
		if (!targetIds.size || !el.documentType) continue;

		const type = documentTypes.get(el.documentType.id);
		const item: UmbMockTrackedReferenceItemModel = {
			$type: 'ElementReferenceResponseModel',
			id: el.id,
			name: el.name,
			published: el.variants.some((v) => v.state === 'Published'),
			documentType: { id: el.documentType.id, icon: el.documentType.icon, alias: type?.alias, name: type?.name },
			variants: el.variants.map((v) => ({
				id: v.id,
				flags: v.flags,
				state: v.state,
				name: v.name,
				culture: v.culture,
			})),
		};

		for (const targetId of targetIds) (map[targetId] ??= []).push(item);
	}

	for (const item of media) {
		const targetIds = new Set<string>();
		collectMatchingIds(item.values, knownIds, targetIds);
		targetIds.delete(item.id);
		if (!targetIds.size) continue;

		const type = mediaTypes.get(item.mediaType.id);
		const reference: UmbMockTrackedReferenceItemModel = {
			$type: 'MediaReferenceResponseModel',
			id: item.id,
			name: item.variants[0]?.name,
			mediaType: { id: item.mediaType.id, icon: item.mediaType.icon, alias: type?.alias, name: type?.name },
		};

		for (const targetId of targetIds) (map[targetId] ??= []).push(reference);
	}

	for (const items of Object.values(map)) items.sort((a, b) => (a.name ?? '').localeCompare(b.name ?? ''));

	return map;
}
