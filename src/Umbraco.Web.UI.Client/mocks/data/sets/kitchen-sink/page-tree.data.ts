import type { UmbMockDocumentModel } from '../../mock-data-set.types.js';
import { EMPTY_PAGE_DOCUMENT_TYPE_ID } from './document-type.data.js';
import type { DocumentVariantResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

type UmbDocumentVariantState = DocumentVariantResponseModel['state'];

// A large document tree for exercising tree pagination and deep expansion:
// "Page 1" sits in the root with 1000 children, and along the first branch (Page 1 > Page 1.1 >
// Page 1.1.1 > ...) the ten first children of each page get 500 children of their own, down to
// level 5. Names follow the position in the tree: Page 1.2.10.
const ROOT_CHILD_COUNT = 1000;
const BRANCH_CHILD_COUNT = 500;
const BRANCHING_SIBLING_COUNT = 10;
const MAX_DEPTH = 5;

const DATE = '2026-01-05 09:00:00';

const documents: Array<UmbMockDocumentModel> = [];

const pageId = (path: Array<number>) => `empty-page-${path.join('-')}`;

const addPage = (path: Array<number>, ancestorIds: Array<string>, hasChildren: boolean) => {
	const id = pageId(path);
	const name = `Page ${path.join('.')}`;
	const parentId = ancestorIds[ancestorIds.length - 1];

	documents.push({
		ancestors: ancestorIds.map((ancestorId) => ({ id: ancestorId })),
		template: null,
		id,
		createDate: DATE,
		parent: parentId ? { id: parentId } : null,
		documentType: {
			id: EMPTY_PAGE_DOCUMENT_TYPE_ID,
			icon: 'icon-document',
		},
		hasChildren,
		noAccess: false,
		isProtected: false,
		isTrashed: false,
		variants: [
			{
				state: 'Published' as UmbDocumentVariantState,
				publishDate: DATE,
				culture: null,
				segment: null,
				name,
				createDate: DATE,
				updateDate: DATE,
				id,
				flags: [],
			},
		],
		values: [],
		flags: [],
	});
};

const addChildren = (parentPath: Array<number>, parentAncestorIds: Array<string>, count: number) => {
	const ancestorIds = [...parentAncestorIds, pageId(parentPath)];
	const depth = parentPath.length + 1;
	// Only the first branch keeps nesting, so a page has children of its own when its parent is on
	// that branch, it is one of the first siblings, and we are not at the deepest level yet.
	const parentIsOnFirstBranch = parentPath.every((segment) => segment === 1);
	const childrenBranch = parentIsOnFirstBranch && depth < MAX_DEPTH;

	for (let index = 1; index <= count; index++) {
		const path = [...parentPath, index];
		const hasChildren = childrenBranch && index <= BRANCHING_SIBLING_COUNT;

		addPage(path, ancestorIds, hasChildren);

		if (hasChildren) {
			addChildren(path, ancestorIds, BRANCH_CHILD_COUNT);
		}
	}
};

addPage([1], [], true);
addChildren([1], [], ROOT_CHILD_COUNT);

export const data: Array<UmbMockDocumentModel> = documents;
