import { UMB_DOCUMENT_TYPE_ENTITY_TYPE } from '../../entity.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';
import { UMB_WORKSPACE_MODAL } from '@umbraco-cms/backoffice/workspace';
import type { UmbDeepPartialObject } from '@umbraco-cms/backoffice/utils';
import type { UmbWorkspaceModalData, UmbWorkspaceModalValue } from '@umbraco-cms/backoffice/workspace';

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbDocumentTypeWorkspaceData extends UmbWorkspaceModalData {}

export const UMB_DOCUMENT_TYPE_WORKSPACE_MODAL = new UmbModalToken<
	UmbDocumentTypeWorkspaceData,
	UmbWorkspaceModalValue
>('Umb.Modal.Workspace', {
	modal: UMB_WORKSPACE_MODAL.getDefaultModal(),
	data: { entityType: UMB_DOCUMENT_TYPE_ENTITY_TYPE, preset: {} },
	// Recast the type, so the entityType data prop is not required:
}) as UmbModalToken<UmbDeepPartialObject<UmbDocumentTypeWorkspaceData>, UmbWorkspaceModalValue>;
