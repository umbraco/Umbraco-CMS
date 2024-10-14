import type { ManifestTiptapExtension } from './tiptap-extension.js';
import type { ManifestTiptapToolbarExtension } from './tiptap-toolbar-extension.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

const kinds: Array<UmbExtensionManifestKind> = [
	{
		type: 'kind',
		alias: 'Umb.Kind.Button',
		matchKind: 'button',
		matchType: 'tiptapToolbarExtension',
		manifest: {
			element: () => import('../components/toolbar/tiptap-toolbar-button.element.js'),
		},
	},
];

const coreExtensions: Array<ManifestTiptapExtension> = [
	{
		type: 'tiptapExtension',
		alias: 'Umb.Tiptap.Embed',
		name: 'Embed Tiptap Extension',
		api: () => import('./core/embedded-media.extension.js'),
		meta: {
			icon: 'icon-embed',
			label: '#general_embed',
			group: '#tiptap_extGroup_media',
		},
	},
	{
		type: 'tiptapExtension',
		alias: 'Umb.Tiptap.Link',
		name: 'Link Tiptap Extension',
		api: () => import('./core/link.extension.js'),
		meta: {
			icon: 'icon-link',
			label: '#defaultdialogs_urlLinkPicker',
			group: '#tiptap_extGroup_interactive',
		},
	},
	{
		type: 'tiptapExtension',
		alias: 'Umb.Tiptap.Figure',
		name: 'Figure Tiptap Extension',
		api: () => import('./core/figure.extension.js'),
		meta: {
			icon: 'icon-frame',
			label: 'Figure',
			group: '#tiptap_extGroup_media',
		},
	},
	{
		type: 'tiptapExtension',
		alias: 'Umb.Tiptap.Image',
		name: 'Image Tiptap Extension',
		api: () => import('./core/image.extension.js'),
		meta: {
			icon: 'icon-picture',
			label: 'Image',
			group: '#tiptap_extGroup_media',
		},
	},
	{
		type: 'tiptapExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Subscript',
		name: 'Subscript Tiptap Extension',
		api: () => import('./core/subscript.extension.js'),
		meta: {
			icon: 'icon-subscript',
			label: 'Subscript',
			group: '#tiptap_extGroup_formatting',
		},
	},
	{
		type: 'tiptapExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Superscript',
		name: 'Superscript Tiptap Extension',
		api: () => import('./core/superscript.extension.js'),
		meta: {
			icon: 'icon-superscript',
			label: 'Superscript',
			group: '#tiptap_extGroup_formatting',
		},
	},
	{
		type: 'tiptapExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Table',
		name: 'Table Tiptap Extension',
		api: () => import('./core/table.extension.js'),
		meta: {
			icon: 'icon-table',
			label: 'Table',
			group: '#tiptap_extGroup_interactive',
		},
	},
	{
		type: 'tiptapExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Underline',
		name: 'Underline Tiptap Extension',
		api: () => import('./core/underline.extension.js'),
		meta: {
			icon: 'icon-underline',
			label: 'Underline',
			group: '#tiptap_extGroup_formatting',
		},
	},
	{
		type: 'tiptapExtension',
		alias: 'Umb.Tiptap.TextAlign',
		name: 'Text Align Tiptap Extension',
		api: () => import('./core/text-align.extension.js'),
		meta: {
			icon: 'icon-text-align-justify',
			label: 'Text Align',
			group: '#tiptap_extGroup_formatting',
		},
	},
	{
		type: 'tiptapExtension',
		alias: 'Umb.Tiptap.MediaUpload',
		name: 'Media Upload Tiptap Extension',
		api: () => import('./core/media-upload.extension.js'),
		meta: {
			icon: 'icon-image-up',
			label: 'Media Upload',
			group: '#tiptap_extGroup_media',
		},
	},
];

const toolbarExtensions: Array<ManifestTiptapToolbarExtension> = [
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.SourceEditor',
		name: 'Source Editor Tiptap Extension',
		api: () => import('./toolbar/source-editor.extension.js'),
		meta: {
			alias: 'umbSourceEditor',
			icon: 'icon-code-xml',
			label: '#general_viewSourceCode',
		},
	},
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.Bold',
		name: 'Bold Tiptap Extension',
		api: () => import('./toolbar/bold.extension.js'),
		meta: {
			alias: 'bold',
			icon: 'icon-bold',
			label: 'Bold',
		},
	},
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.Italic',
		name: 'Italic Tiptap Extension',
		api: () => import('./toolbar/italic.extension.js'),
		meta: {
			alias: 'italic',
			icon: 'icon-italic',
			label: 'Italic',
		},
	},
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.Underline',
		name: 'Underline Tiptap Extension',
		api: () => import('./toolbar/underline.extension.js'),
		meta: {
			alias: 'underline',
			icon: 'icon-underline',
			label: 'Underline',
		},
	},
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.Strike',
		name: 'Strike Tiptap Extension',
		api: () => import('./toolbar/strike.extension.js'),
		meta: {
			alias: 'strike',
			icon: 'icon-strikethrough',
			label: 'Strike',
		},
	},
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.TextAlignLeft',
		name: 'Text Align Left Tiptap Extension',
		api: () => import('./toolbar/text-align-left.extension.js'),
		meta: {
			alias: 'text-align-left',
			icon: 'icon-text-align-left',
			label: 'Text Align Left',
		},
	},
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.TextAlignCenter',
		name: 'Text Align Center Tiptap Extension',
		api: () => import('./toolbar/text-align-center.extension.js'),
		meta: {
			alias: 'text-align-center',
			icon: 'icon-text-align-center',
			label: 'Text Align Center',
		},
	},
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.TextAlignRight',
		name: 'Text Align Right Tiptap Extension',
		api: () => import('./toolbar/text-align-right.extension.js'),
		meta: {
			alias: 'text-align-right',
			icon: 'icon-text-align-right',
			label: 'Text Align Right',
		},
	},
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.TextAlignJustify',
		name: 'Text Align Justify Tiptap Extension',
		api: () => import('./toolbar/text-align-justify.extension.js'),
		meta: {
			alias: 'text-align-justify',
			icon: 'icon-text-align-justify',
			label: 'Text Align Justify',
		},
	},
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.Heading1',
		name: 'Heading 1 Tiptap Extension',
		api: () => import('./toolbar/heading1.extension.js'),
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
		name: 'Heading 2 Tiptap Extension',
		api: () => import('./toolbar/heading2.extension.js'),
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
		name: 'Heading 3 Tiptap Extension',
		api: () => import('./toolbar/heading3.extension.js'),
		meta: {
			alias: 'heading3',
			icon: 'icon-heading-3',
			label: 'Heading 3',
		},
	},
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.BulletList',
		name: 'Bullet List Tiptap Extension',
		api: () => import('./toolbar/bullet-list.extension.js'),
		meta: {
			alias: 'bulletList',
			icon: 'icon-bulleted-list',
			label: 'Bullet List',
		},
	},
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.OrderedList',
		name: 'Ordered List Tiptap Extension',
		api: () => import('./toolbar/ordered-list.extension.js'),
		meta: {
			alias: 'orderedList',
			icon: 'icon-ordered-list',
			label: 'Ordered List',
		},
	},
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.Blockquote',
		name: 'Blockquote Tiptap Extension',
		api: () => import('./toolbar/blockquote.extension.js'),
		meta: {
			alias: 'blockquote',
			icon: 'icon-blockquote',
			label: 'Blockquote',
		},
	},
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.Link',
		name: 'Link Tiptap Extension',
		api: () => import('./toolbar/link.extension.js'),
		meta: {
			alias: 'umbLink',
			icon: 'icon-link',
			label: '#defaultdialogs_urlLinkPicker',
		},
	},
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.Unlink',
		name: 'Unlink Tiptap Extension',
		api: () => import('./toolbar/unlink.extension.js'),
		element: () => import('../components/toolbar/tiptap-toolbar-button-disabled.element.js'),
		meta: {
			alias: 'unlink',
			icon: 'icon-unlink',
			label: 'Unlink',
		},
	},
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.CodeBlock',
		name: 'Code Block Tiptap Extension',
		api: () => import('./toolbar/code-block.extension.js'),
		meta: {
			alias: 'codeBlock',
			icon: 'icon-code',
			label: 'Code Block',
		},
	},
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.Subscript',
		name: 'Subscript Tiptap Extension',
		api: () => import('./toolbar/subscript.extension.js'),
		meta: {
			alias: 'subscript',
			icon: 'icon-subscript',
			label: 'Subscript',
		},
	},
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.Superscript',
		name: 'Superscript Tiptap Extension',
		api: () => import('./toolbar/superscript.extension.js'),
		meta: {
			alias: 'superscript',
			icon: 'icon-superscript',
			label: 'Superscript',
		},
	},
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.HorizontalRule',
		name: 'Horizontal Rule Tiptap Extension',
		api: () => import('./toolbar/horizontal-rule.extension.js'),
		meta: {
			alias: 'horizontalRule',
			icon: 'icon-horizontal-rule',
			label: 'Horizontal Rule',
		},
	},
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.Undo',
		name: 'Undo Tiptap Extension',
		api: () => import('./toolbar/undo.extension.js'),
		element: () => import('../components/toolbar/tiptap-toolbar-button-disabled.element.js'),
		meta: {
			alias: 'undo',
			icon: 'icon-undo',
			label: 'Undo',
		},
	},
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.Redo',
		name: 'Redo Tiptap Extension',
		api: () => import('./toolbar/redo.extension.js'),
		element: () => import('../components/toolbar/tiptap-toolbar-button-disabled.element.js'),
		meta: {
			alias: 'redo',
			icon: 'icon-redo',
			label: 'Redo',
		},
	},
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.MediaPicker',
		name: 'Media Picker Tiptap Extension',
		api: () => import('./toolbar/media-picker.extension.js'),
		meta: {
			alias: 'umbMedia',
			icon: 'icon-picture',
			label: 'Media picker',
		},
	},
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.EmbeddedMedia',
		name: 'Embedded Media Tiptap Extension',
		api: () => import('./toolbar/embedded-media.extension.js'),
		meta: {
			alias: 'umbEmbeddedMedia',
			icon: 'icon-embed',
			label: '#general_embed',
		},
	},
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.Table',
		name: 'Table Tiptap Extension',
		api: () => import('./toolbar/table.extension.js'),
		meta: {
			alias: 'table',
			icon: 'icon-table',
			label: 'Table',
		},
	},
];

const extensions = [...coreExtensions, ...toolbarExtensions];

export const manifests = [...kinds, ...extensions];
