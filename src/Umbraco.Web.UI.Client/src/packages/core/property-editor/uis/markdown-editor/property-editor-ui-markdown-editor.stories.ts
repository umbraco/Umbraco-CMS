import { Meta, Story } from '@storybook/web-components';
import type { UmbPropertyEditorUIMarkdownEditorElement } from './property-editor-ui-markdown-editor.element.js';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-markdown-editor.element.js';

export default {
	title: 'Property Editor UIs/Markdown Editor',
	component: 'umb-property-editor-ui-markdown-editor',
	id: 'umb-property-editor-ui-markdown-editor',
} as Meta;

export const AAAOverview: Story<UmbPropertyEditorUIMarkdownEditorElement> = () =>
	html`<umb-property-editor-ui-markdown-editor></umb-property-editor-ui-markdown-editor>`;
AAAOverview.storyName = 'Overview';
