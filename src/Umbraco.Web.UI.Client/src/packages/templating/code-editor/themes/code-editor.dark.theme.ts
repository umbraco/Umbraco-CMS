import type { monaco } from '@umbraco-cms/backoffice/external/monaco-editor';

export const UmbCodeEditorThemeDark: monaco.editor.IStandaloneThemeData = {
	base: 'vs-dark',
	inherit: true, // can also be false to completely replace the builtin rules
	rules: [],
	colors: {
		'editor.background': '#21262e',
	},
};
