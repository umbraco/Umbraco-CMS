import type { UmbTemplateDetailModel } from '../types.js';
import type { UmbWorkspaceModalData, UmbWorkspaceModalValue } from '@umbraco-cms/backoffice/workspace';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

/**
 * Modal token that opens the template workspace in a sidebar modal.
 * The modal is marked as a reference (`isReference`), so the workspace is presented as a
 * reference to the template rather than as the primary editing context.
 * Only `preset` and other workspace data are required; the entity type is already set.
 */
export const UMB_TEMPLATE_WORKSPACE_MODAL = new UmbModalToken<
	UmbWorkspaceModalData<UmbTemplateDetailModel>,
	UmbWorkspaceModalValue
>('Umb.Modal.Workspace', {
	modal: {
		type: 'sidebar',
		size: 'large',
		isReference: true,
	},
	data: { entityType: 'template', preset: {} },
	// Recast the type, so the entityType data prop is not required:
}) as UmbModalToken<Omit<UmbWorkspaceModalData<UmbTemplateDetailModel>, 'entityType'>, UmbWorkspaceModalValue>;
