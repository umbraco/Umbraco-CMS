import { UmbModalToken } from '@umbraco-cms/backoffice/modal';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

export interface UmbElementCreateOptionsModalData {
	parent: UmbEntityModel;
	documentType: { unique: string } | null;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbElementCreateOptionsModalValue {}

export const UMB_ELEMENT_CREATE_OPTIONS_MODAL = new UmbModalToken<
	UmbElementCreateOptionsModalData,
	UmbElementCreateOptionsModalValue
>('Umb.Modal.Element.CreateOptions', { modal: { type: 'dialog' } });
