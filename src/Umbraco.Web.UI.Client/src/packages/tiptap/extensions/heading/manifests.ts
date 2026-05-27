import UmbTiptapHeadingExtensionApi from './heading.tiptap-api.js';
import UmbTiptapToolbarHeading1ExtensionApi from './heading1.tiptap-toolbar-api.js';
import UmbTiptapToolbarHeading2ExtensionApi from './heading2.tiptap-toolbar-api.js';
import UmbTiptapToolbarHeading3ExtensionApi from './heading3.tiptap-toolbar-api.js';
import UmbTiptapToolbarHeading4ExtensionApi from './heading4.tiptap-toolbar-api.js';
import UmbTiptapToolbarHeading5ExtensionApi from './heading5.tiptap-toolbar-api.js';
import UmbTiptapToolbarHeading6ExtensionApi from './heading6.tiptap-toolbar-api.js';
export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapExtension',
		alias: 'Umb.Tiptap.Heading',
		name: 'Headings Tiptap Extension',
		api: UmbTiptapHeadingExtensionApi,
		meta: {
			icon: 'icon-heading',
			label: 'Headings',
			group: '#tiptap_extGroup_formatting',
		},
	},
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.Heading1',
		name: 'Heading 1 Tiptap Toolbar Extension',
		api: UmbTiptapToolbarHeading1ExtensionApi,
		forExtensions: ['Umb.Tiptap.Heading'],
		meta: {
			alias: 'heading1',
			icon: 'icon-heading-1',
			label: 'Heading 1',
		},
	},
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.Heading2',
		name: 'Heading 2 Tiptap Toolbar Extension',
		api: UmbTiptapToolbarHeading2ExtensionApi,
		forExtensions: ['Umb.Tiptap.Heading'],
		meta: {
			alias: 'heading2',
			icon: 'icon-heading-2',
			label: 'Heading 2',
		},
	},
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.Heading3',
		name: 'Heading 3 Tiptap Toolbar Extension',
		api: UmbTiptapToolbarHeading3ExtensionApi,
		forExtensions: ['Umb.Tiptap.Heading'],
		meta: {
			alias: 'heading3',
			icon: 'icon-heading-3',
			label: 'Heading 3',
		},
	},
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.Heading4',
		name: 'Heading 4 Tiptap Toolbar Extension',
		api: UmbTiptapToolbarHeading4ExtensionApi,
		forExtensions: ['Umb.Tiptap.Heading'],
		meta: {
			alias: 'heading4',
			icon: 'icon-heading-4',
			label: 'Heading 4',
		},
	},
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.Heading5',
		name: 'Heading 5 Tiptap Toolbar Extension',
		api: UmbTiptapToolbarHeading5ExtensionApi,
		forExtensions: ['Umb.Tiptap.Heading'],
		meta: {
			alias: 'heading5',
			icon: 'icon-heading-5',
			label: 'Heading 5',
		},
	},
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.Heading6',
		name: 'Heading 6 Tiptap Toolbar Extension',
		api: UmbTiptapToolbarHeading6ExtensionApi,
		forExtensions: ['Umb.Tiptap.Heading'],
		meta: {
			alias: 'heading6',
			icon: 'icon-heading-6',
			label: 'Heading 6',
		},
	},
];
