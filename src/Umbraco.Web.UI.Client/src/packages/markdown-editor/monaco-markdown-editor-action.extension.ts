import type { ManifestApi } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestMonacoMarkdownEditorAction extends ManifestApi<any> {
	type: 'monacoMarkdownEditorAction';
	meta?: MetaMonacoMarkdownEditorAction;
}

export type MetaMonacoMarkdownEditorAction = {
	icon?: string | null;
	label?: string | null;
};

declare global {
	interface UmbExtensionManifestMap {
		umbMonacoMarkdownEditorAction: ManifestMonacoMarkdownEditorAction;
	}
}
