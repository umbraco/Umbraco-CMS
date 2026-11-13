import { umbMockManager } from '../mock-manager.js';
import type {
	PagedIReferenceResponseModel,
	PagedReferenceByIdModel,
} from '@umbraco-cms/backoffice/external/backend-api';

function getPaging(request: Request) {
	const query = new URL(request.url).searchParams;
	const skip = query.get('skip') ? parseInt(query.get('skip') as string, 10) : 0;
	const take = query.get('take') ? parseInt(query.get('take') as string, 10) : 100;
	return { skip, take };
}

function getReferencedByMap() {
	return umbMockManager.getDataSet().referencedBy;
}

function pageIds(ids: Array<string>, request: Request): PagedReferenceByIdModel {
	const { skip, take } = getPaging(request);
	return { total: ids.length, items: ids.slice(skip, skip + take).map((id) => ({ id })) };
}

/**
 * Pages the items referencing `id`. Falls back to `legacyItems` when the data set has no `referencedBy` map.
 * @param id
 * @param request
 * @param legacyItems
 */
export function getReferencedByPage(
	id: string,
	request: Request,
	legacyItems: () => PagedIReferenceResponseModel['items'],
): PagedIReferenceResponseModel {
	const map = getReferencedByMap();
	const data: PagedIReferenceResponseModel['items'] = map ? (map[id] ?? []) : legacyItems();
	const { skip, take } = getPaging(request);
	return { total: data.length, items: data.slice(skip, skip + take) };
}

/**
 * Pages the requested `id` query values that are referenced by something.
 * @param request
 */
export function getAreReferencedPage(request: Request): PagedReferenceByIdModel {
	const map = getReferencedByMap();
	const requested = new URL(request.url).searchParams.getAll('id');
	return pageIds(map ? requested.filter((id) => map[id]?.length) : [], request);
}

/**
 * Pages the descendants of `id` that are referenced by something.
 * @param id
 * @param request
 * @param entities The entities of the type being queried, used to follow parent links.
 */
export function getReferencedDescendantsPage(
	id: string,
	request: Request,
	entities: Array<{ id: string; parent?: { id: string } | null }>,
): PagedReferenceByIdModel {
	const map = getReferencedByMap();
	if (!map) return pageIds([], request);

	const parentOf = new Map(entities.map((x) => [x.id, x.parent?.id]));
	const isDescendant = (candidate: string) => {
		const visited = new Set<string>();
		let current = parentOf.get(candidate);
		while (current && !visited.has(current)) {
			if (current === id) return true;
			visited.add(current);
			current = parentOf.get(current);
		}
		return false;
	};

	return pageIds(
		Object.keys(map).filter((key) => map[key].length && isDescendant(key)),
		request,
	);
}
