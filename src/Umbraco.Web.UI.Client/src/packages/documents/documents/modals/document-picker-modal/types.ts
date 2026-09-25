import type { UmbDocumentTreeItemModel } from '../../tree/types.js';
import type { UmbTreePickerModalData, UmbTreePickerModalValue } from '@umbraco-cms/backoffice/tree';

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbDocumentPickerModalData extends UmbTreePickerModalData<UmbDocumentTreeItemModel> {}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbDocumentPickerModalValue extends UmbTreePickerModalValue {}
