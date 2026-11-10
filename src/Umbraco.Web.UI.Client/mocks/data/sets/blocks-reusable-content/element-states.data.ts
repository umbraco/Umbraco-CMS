import type { UmbMockElementModel } from '../../mock-data-set.types.js';
import { UmbElementVariantState } from '@umbraco-cms/backoffice/element';

const ELEMENT_ONE_TYPE = { id: 'b818bb55-31e1-4537-9c42-17471a176089', icon: 'icon-attachment color-deep-purple' };
const VARIANT_ELEMENT_TYPE = { id: 'a9c5e3f7-6b12-4d84-9e0f-2b7a4d1c8e65', icon: 'icon-science color-deep-purple' };
const ORPHAN_ELEMENT_TYPE = { id: 'deleted-element-type-id', icon: 'icon-science color-deep-purple' };

const CREATE_DATE = '2024-01-15T10:00:00.000Z';
const PUBLISH_DATE = '2024-02-01T10:00:00.000Z';

const STATES_FOLDER_ID = 'library-folder-states-id';

const statesFolder: UmbMockElementModel = {
	ancestors: [],
	id: STATES_FOLDER_ID,
	createDate: CREATE_DATE,
	parent: null,
	documentType: null,
	hasChildren: true,
	isTrashed: false,
	isFolder: true,
	name: 'States',
	variants: [],
	values: [],
	flags: [],
	noAccess: false,
};

type UmbStateVariant = { culture: string | null; state: UmbElementVariantState; publishDate: string | null };

const createStateElement = ({
	id,
	name,
	documentType = ELEMENT_ONE_TYPE,
	variants,
	values,
	isTrashed = false,
	noAccess = false,
}: {
	id: string;
	name: string;
	documentType?: { id: string; icon: string };
	variants: Array<UmbStateVariant>;
	values: Array<{ culture: string | null; value: string }>;
	isTrashed?: boolean;
	noAccess?: boolean;
}): UmbMockElementModel => ({
	ancestors: isTrashed ? [] : [{ id: STATES_FOLDER_ID }],
	id,
	createDate: CREATE_DATE,
	parent: isTrashed ? null : { id: STATES_FOLDER_ID },
	documentType,
	hasChildren: false,
	isTrashed,
	isFolder: false,
	name,
	variants: variants.map((variant) => ({
		state: variant.state,
		culture: variant.culture,
		name,
		createDate: CREATE_DATE,
		updateDate: CREATE_DATE,
		publishDate: variant.publishDate,
		id: `${id}-${variant.culture ?? 'invariant'}`,
		flags: [],
	})),
	values: values.map((entry) => ({
		editorAlias: 'Umbraco.TextBox',
		alias: 'title',
		culture: entry.culture,
		segment: null,
		value: entry.value,
	})),
	flags: [],
	noAccess,
});

const invariant = (state: UmbElementVariantState, publishDate: string | null): Array<UmbStateVariant> => [
	{ culture: null, state, publishDate },
];

const published = createStateElement({
	id: 'library-element-state-published-id',
	name: 'Published (Library)',
	variants: invariant(UmbElementVariantState.PUBLISHED, PUBLISH_DATE),
	values: [{ culture: null, value: 'Published' }],
});

const draft = createStateElement({
	id: 'library-element-state-draft-id',
	name: 'Draft (Library)',
	variants: invariant(UmbElementVariantState.DRAFT, null),
	values: [{ culture: null, value: 'Draft, never published' }],
});

const pendingChanges = createStateElement({
	id: 'library-element-state-pending-id',
	name: 'Pending changes (Library)',
	variants: invariant(UmbElementVariantState.PUBLISHED_PENDING_CHANGES, PUBLISH_DATE),
	values: [{ culture: null, value: 'Published with newer saved changes' }],
});

const unpublished = createStateElement({
	id: 'library-element-state-unpublished-id',
	name: 'Unpublished (Library)',
	variants: invariant(UmbElementVariantState.DRAFT, PUBLISH_DATE),
	values: [{ culture: null, value: 'Previously published, now unpublished' }],
});

const trashed = createStateElement({
	id: 'library-element-state-trashed-id',
	name: 'Trashed (Library)',
	variants: invariant(UmbElementVariantState.TRASHED, PUBLISH_DATE),
	values: [{ culture: null, value: 'In the recycle bin' }],
	isTrashed: true,
});

const noAccess = createStateElement({
	id: 'library-element-state-no-access-id',
	name: 'No access (Library)',
	variants: invariant(UmbElementVariantState.PUBLISHED, PUBLISH_DATE),
	values: [{ culture: null, value: 'The user has no access' }],
	noAccess: true,
});

const orphanType = createStateElement({
	id: 'library-element-state-orphan-type-id',
	name: 'Deleted Element Type (Library)',
	documentType: ORPHAN_ELEMENT_TYPE,
	variants: invariant(UmbElementVariantState.PUBLISHED, PUBLISH_DATE),
	values: [{ culture: null, value: 'Its Element Type no longer exists' }],
});

const variantEnPublishedDaDraft = createStateElement({
	id: 'library-variant-element-en-published-da-draft-id',
	name: 'English published, Danish draft (Library)',
	documentType: VARIANT_ELEMENT_TYPE,
	variants: [
		{ culture: 'en-US', state: UmbElementVariantState.PUBLISHED, publishDate: PUBLISH_DATE },
		{ culture: 'da', state: UmbElementVariantState.DRAFT, publishDate: null },
	],
	values: [
		{ culture: 'en-US', value: 'Published (English)' },
		{ culture: 'da', value: 'Kladde (dansk)' },
	],
});

const variantEnPendingDaPublished = createStateElement({
	id: 'library-variant-element-en-pending-da-published-id',
	name: 'English pending changes, Danish published (Library)',
	documentType: VARIANT_ELEMENT_TYPE,
	variants: [
		{ culture: 'en-US', state: UmbElementVariantState.PUBLISHED_PENDING_CHANGES, publishDate: PUBLISH_DATE },
		{ culture: 'da', state: UmbElementVariantState.PUBLISHED, publishDate: PUBLISH_DATE },
	],
	values: [
		{ culture: 'en-US', value: 'Published with pending changes (English)' },
		{ culture: 'da', value: 'Udgivet (dansk)' },
	],
});

const variantEnPublishedDaUnpublished = createStateElement({
	id: 'library-variant-element-en-published-da-unpublished-id',
	name: 'English published, Danish unpublished (Library)',
	documentType: VARIANT_ELEMENT_TYPE,
	variants: [
		{ culture: 'en-US', state: UmbElementVariantState.PUBLISHED, publishDate: PUBLISH_DATE },
		{ culture: 'da', state: UmbElementVariantState.DRAFT, publishDate: PUBLISH_DATE },
	],
	values: [
		{ culture: 'en-US', value: 'Published (English)' },
		{ culture: 'da', value: 'Afpubliceret (dansk)' },
	],
});

const variantTrashed = createStateElement({
	id: 'library-variant-element-trashed-id',
	name: 'Trashed variant (Library)',
	documentType: VARIANT_ELEMENT_TYPE,
	variants: [
		{ culture: 'en-US', state: UmbElementVariantState.TRASHED, publishDate: PUBLISH_DATE },
		{ culture: 'da', state: UmbElementVariantState.TRASHED, publishDate: PUBLISH_DATE },
	],
	values: [
		{ culture: 'en-US', value: 'In the recycle bin (English)' },
		{ culture: 'da', value: 'I papirkurven (dansk)' },
	],
	isTrashed: true,
});

export const data: Array<UmbMockElementModel> = [
	statesFolder,
	published,
	draft,
	pendingChanges,
	unpublished,
	trashed,
	noAccess,
	orphanType,
	variantEnPublishedDaDraft,
	variantEnPendingDaPublished,
	variantEnPublishedDaUnpublished,
	variantTrashed,
];
