import type { ManifestElement } from '@umbraco-cms/backoffice/extension-api';
import type { UmbBlockEditorCustomViewElement } from '../interfaces/index.js';

export interface ManifestBlockEditorCustomView extends ManifestElement<UmbBlockEditorCustomViewElement> {
	type: 'blockEditorCustomView';
	forContentTypeAlias?: Array<string>;
	forBlockType?: Array<string>;
}
