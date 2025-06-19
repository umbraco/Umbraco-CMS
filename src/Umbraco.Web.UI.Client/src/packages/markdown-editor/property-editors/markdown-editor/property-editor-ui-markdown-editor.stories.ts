import type { UmbPropertyEditorUIMarkdownEditorElement } from './property-editor-ui-markdown-editor.element.js';
import type { Meta, StoryFn } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-markdown-editor.element.js';

export default {
	title: 'Property Editor UIs/Markdown Editor',
	component: 'umb-property-editor-ui-markdown-editor',
	id: 'umb-property-editor-ui-markdown-editor',
} as Meta;

export const AAAOverview: StoryFn<UmbPropertyEditorUIMarkdownEditorElement> = () =>
	html`<umb-property-editor-ui-markdown-editor></umb-property-editor-ui-markdown-editor>`;
AAAOverview.storyName = 'Overview';
