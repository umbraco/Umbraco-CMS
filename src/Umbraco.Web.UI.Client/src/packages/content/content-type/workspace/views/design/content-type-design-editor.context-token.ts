import type { UmbContentTypeDesignEditorContext } from './content-type-design-editor.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_CONTENT_TYPE_DESIGN_EDITOR_CONTEXT = new UmbContextToken<
	UmbContentTypeDesignEditorContext,
	UmbContentTypeDesignEditorContext
>('UmbContentTypeDesignEditorContext');
