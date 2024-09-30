import type { UmbPropertyEditorUiTiptapElement } from './property-editor-ui-tiptap.element.js';
import type { Meta, StoryObj } from '@storybook/web-components';
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
				[
					['Umb.Tiptap.Toolbar.Bold', 'Umb.Tiptap.Toolbar.Italic', 'Umb.Tiptap.Toolbar.Underline'],
					[
						'Umb.Tiptap.Toolbar.TextAlignLeft',
						'Umb.Tiptap.Toolbar.TextAlignCenter',
						'Umb.Tiptap.Toolbar.TextAlignRight',
					],
					['Umb.Tiptap.Toolbar.Heading1', 'Umb.Tiptap.Toolbar.Heading2', 'Umb.Tiptap.Toolbar.Heading3'],
					['Umb.Tiptap.Toolbar.Unlink', 'Umb.Tiptap.Toolbar.Link'],
					['Umb.Tiptap.Toolbar.Embed', 'Umb.Tiptap.Toolbar.MediaPicker', 'Umb.Tiptap.Toolbar.BlockPicker'],
					['Umb.Tiptap.Toolbar.Redo', 'Umb.Tiptap.Toolbar.Undo'],
				],
			],
		],
	},
	{
		alias: 'extensions',
		value: [
			'Umb.Tiptap.Bold',
			'Umb.Tiptap.Italic',
			'Umb.Tiptap.Underline',
			'Umb.Tiptap.Strike',
			'Umb.Tiptap.Blockquote',
			'Umb.Tiptap.CodeBlock',
			'Umb.Tiptap.HorizontalRule',
			'Umb.Tiptap.Figure',
			'Umb.Tiptap.Table',
			'Umb.Tiptap.Link',
			'Umb.Tiptap.Embed',
			'Umb.Tiptap.Image',
			'Umb.Tiptap.Heading',
			'Umb.Tiptap.List',
			'Umb.Tiptap.TextAlign',
			'Umb.Tiptap.MediaUpload',
			'Umb.Tiptap.Block',
		],
	},
]);

const meta: Meta<UmbPropertyEditorUiTiptapElement> = {
	title: 'Property Editor UIs/Tiptap',
	component: 'umb-property-editor-ui-tiptap',
	id: 'umb-property-editor-ui-tiptap',
	args: {
		config: undefined,
		value: {
			blocks: {
				layout: {},
				contentData: [],
				settingsData: [],
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
