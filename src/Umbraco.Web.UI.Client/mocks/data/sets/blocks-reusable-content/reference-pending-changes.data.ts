import type { UmbMockReferencedElementModel } from '../../mock-data-set.types.js';

// Direct references to elements that are not fully published, keyed by the referencing document/element's id.
// See `element.data.ts` for `library-element-three-id` (Draft) and `library-element-four-id` (PendingChanges) —
// the mock handlers derive each element's `state` from that data rather than repeating it here.
export const data: Record<string, Array<UmbMockReferencedElementModel>> = {
	// Block Grid document
	'17cd53f2-93b3-4e34-ade2-916e7a6639ed': [
		{ id: 'library-element-three-id', isScheduled: true },
		{ id: 'library-element-four-id', isScheduled: false },
	],
	// Block List document
	'39842212-489e-46ec-a63b-6eeff36c7156': [{ id: 'library-element-three-id', isScheduled: true }],
	// Element Two (Library) references Element Three — the element→element case
	'library-element-two-id': [{ id: 'library-element-three-id', isScheduled: true }],
};
