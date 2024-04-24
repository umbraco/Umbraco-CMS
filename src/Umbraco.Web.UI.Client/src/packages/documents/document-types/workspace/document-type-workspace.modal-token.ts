import type { UmbWorkspaceModalData, UmbWorkspaceModalValue } from '@umbraco-cms/backoffice/modal';
import { UMB_WORKSPACE_MODAL, UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbDocumentTypeWorkspaceData extends UmbWorkspaceModalData {}

export const UMB_DOCUMENT_TYPE_WORKSPACE_MODAL = new UmbModalToken<
	UmbDocumentTypeWorkspaceData,
	UmbWorkspaceModalValue
>('Umb.Modal.Workspace', {
	modal: UMB_WORKSPACE_MODAL.getDefaultModal(),
	data: { entityType: 'block', preset: {} },
	// Recast the type, so the entityType data prop is not required:
}) as UmbModalToken<Omit<UmbWorkspaceModalData, 'entityType'>, UmbWorkspaceModalValue>;
