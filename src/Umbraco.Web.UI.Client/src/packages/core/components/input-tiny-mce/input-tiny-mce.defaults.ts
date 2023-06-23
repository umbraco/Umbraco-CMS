import type { tinymce } from '@umbraco-cms/backoffice/external/tinymce';

export type TinyStyleSheet = tinymce.RawEditorOptions['style_formats'];

export const defaultStyleFormats: TinyStyleSheet = [
	{
		title: 'Headers',
		items: [
			{ title: 'Page header', block: 'h2' },
			{ title: 'Section header', block: 'h3' },
			{ title: 'Paragraph header', block: 'h4' },
		],
	},
	{
		title: 'Blocks',
		items: [{ title: 'Normal', block: 'p' }],
	},
	{
		title: 'Containers',
		items: [
			{ title: 'Quote', block: 'blockquote' },
			{ title: 'Code', block: 'code' },
		],
	},
];

//These are absolutely required in order for the macros to render inline
//we put these as extended elements because they get merged on top of the normal allowed elements by tiny mce
export const defaultExtendedValidElements =
	'@[id|class|style],-div[id|dir|class|align|style],ins[datetime|cite],-ul[class|style],-li[class|style],-h1[id|dir|class|align|style],-h2[id|dir|class|align|style],-h3[id|dir|class|align|style],-h4[id|dir|class|align|style],-h5[id|dir|class|align|style],-h6[id|style|dir|class|align],span[id|class|style|lang],figure,figcaption';

export const defaultFallbackConfig: tinymce.RawEditorOptions = {
	toolbar: [
		'styles',
		'bold',
		'italic',
		'alignleft',
		'aligncenter',
		'alignright',
		'bullist',
		'numlist',
		'outdent',
		'indent',
		'link',
		'umbmediapicker',
		'umbmacro',
		'umbembeddialog',
	],
	mode: 'classic',
	stylesheets: [],
	maxImageSize: 500,
};
