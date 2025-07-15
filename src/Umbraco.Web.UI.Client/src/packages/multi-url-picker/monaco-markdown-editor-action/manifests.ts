export const manifests = [
	{
		type: 'monacoMarkdownEditorAction',
		alias: 'Umb.MonacoMarkdownEditorAction.MultiUrlPicker',
		name: 'Multi Url Picker Monaco Markdown Editor Action',
		api: () => import('./url-picker-monaco-markdown-editor-action.js'),
		meta: {
			label: '#buttons_linkInsert',
			icon: 'icon-link',
		},
	},
];
