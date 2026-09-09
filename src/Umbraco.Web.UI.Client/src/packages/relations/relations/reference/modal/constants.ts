import type { UmbEntityReferencesModalData, UmbEntityReferencesModalValue } from './types.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export const UMB_ENTITY_REFERENCES_MODAL_ALIAS = 'Umb.Modal.EntityReferences';

export const UMB_ENTITY_REFERENCES_MODAL = new UmbModalToken<
	UmbEntityReferencesModalData,
	UmbEntityReferencesModalValue
>(UMB_ENTITY_REFERENCES_MODAL_ALIAS, {
	modal: {
		type: 'dialog',
	},
});
