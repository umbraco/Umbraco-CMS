import type { monaco } from '@umbraco-cms/backoffice/external/monaco-editor';

export const UmbCodeEditorThemeLight: monaco.editor.IStandaloneThemeData = {
	base: 'vs',
	inherit: true, // can also be false to completely replace the builtin rules
	rules: [],
	colors: {},
};
