import type { UmbPropertyEditorUIMarkdownEditorElement } from './property-editor-ui-markdown-editor.element.js';
import type { Meta, StoryFn } from '@storybook/web-components-vite';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-markdown-editor.element.js';

export default {
	title: 'Extension Type/Property Editor UI/Markdown Editor',
	component: 'umb-property-editor-ui-markdown-editor',
	id: 'umb-property-editor-ui-markdown-editor',
} as Meta;

export const Docs: StoryFn<UmbPropertyEditorUIMarkdownEditorElement> = () =>
	html`<umb-property-editor-ui-markdown-editor></umb-property-editor-ui-markdown-editor>`;
