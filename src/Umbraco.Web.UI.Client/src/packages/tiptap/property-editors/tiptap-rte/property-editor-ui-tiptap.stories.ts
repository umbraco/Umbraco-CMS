import type { UmbPropertyEditorUiTiptapElement } from './property-editor-ui-tiptap.element.js';
import type { Meta, StoryObj } from '@storybook/web-components-vite';
import { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';

import './property-editor-ui-tiptap.element.js';

const config = new UmbPropertyEditorConfigCollection([
	{
		alias: 'hideLabel',
		value: true,
	},
	{ alias: 'dimensions', value: { height: 500 } },
	{ alias: 'maxImageSize', value: 500 },
	{ alias: 'ignoreUserStartNodes', value: false },
	{
		alias: 'toolbar',
		value: [
			[
				['Umb.Tiptap.Toolbar.SourceEditor'],
				['Umb.Tiptap.Toolbar.Bold', 'Umb.Tiptap.Toolbar.Italic', 'Umb.Tiptap.Toolbar.Underline'],
				['Umb.Tiptap.Toolbar.TextAlignLeft', 'Umb.Tiptap.Toolbar.TextAlignCenter', 'Umb.Tiptap.Toolbar.TextAlignRight'],
				['Umb.Tiptap.Toolbar.BulletList', 'Umb.Tiptap.Toolbar.OrderedList'],
				['Umb.Tiptap.Toolbar.Blockquote', 'Umb.Tiptap.Toolbar.HorizontalRule', 'Umb.Tiptap.Table'],
				['Umb.Tiptap.Toolbar.Link', 'Umb.Tiptap.Toolbar.Unlink'],
				['Umb.Tiptap.Toolbar.MediaPicker', 'Umb.Tiptap.Toolbar.EmbeddedMedia'],
			],
		],
	},
	{
		alias: 'extensions',
		value: [
			'Umb.Tiptap.RichTextEssentials',
			'Umb.Tiptap.Blockquote',
			'Umb.Tiptap.Bold',
			'Umb.Tiptap.BulletList',
			'Umb.Tiptap.Embed',
			'Umb.Tiptap.Figure',
			'Umb.Tiptap.HorizontalRule',
			'Umb.Tiptap.Image',
			'Umb.Tiptap.Italic',
			'Umb.Tiptap.Link',
			'Umb.Tiptap.MediaUpload',
			'Umb.Tiptap.OrderedList',
			'Umb.Tiptap.Subscript',
			'Umb.Tiptap.Superscript',
			'Umb.Tiptap.Table',
			'Umb.Tiptap.TextAlign',
			'Umb.Tiptap.Underline',
		],
	},
]);

const meta: Meta<UmbPropertyEditorUiTiptapElement> = {
	title: 'Extension Type/Property Editor UI/Tiptap',
	component: 'umb-property-editor-ui-tiptap',
	id: 'umb-property-editor-ui-tiptap',
	args: {
		config: undefined,
		value: {
			blocks: {
				layout: {},
				contentData: [],
				settingsData: [],
				expose: [],
			},
			markup: `
			<h2>Tiptap</h2>
			<p>I am a default value for the Tiptap text editor story.</p>
			<p>
				<a href="https://docs.umbraco.com" target="_blank" rel="noopener noreferrer">Umbraco documentation</a>
			</p>
		`,
		},
	},
};

export default meta;
type Story = StoryObj<UmbPropertyEditorUiTiptapElement>;

export const Default: Story = {};

export const DefaultConfig: Story = {
	args: {
		config,
	},
};
