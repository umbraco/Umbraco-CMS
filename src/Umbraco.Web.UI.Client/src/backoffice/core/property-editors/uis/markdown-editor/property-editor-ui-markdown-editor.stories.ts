import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';

import type { UmbPropertyEditorUIMarkdownEditorElement } from './property-editor-ui-markdown-editor.element';
import './property-editor-ui-markdown-editor.element';

export default {
	title: 'Property Editor UIs/Markdown Editor',
	component: 'umb-property-editor-ui-markdown-editor',
	id: 'umb-property-editor-ui-markdown-editor',
} as Meta;

export const AAAOverview: Story<UmbPropertyEditorUIMarkdownEditorElement> = () =>
	html`<umb-property-editor-ui-markdown-editor></umb-property-editor-ui-markdown-editor>`;
AAAOverview.storyName = 'Overview';
