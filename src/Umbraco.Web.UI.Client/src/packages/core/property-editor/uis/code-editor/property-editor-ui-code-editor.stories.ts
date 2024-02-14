import type { Meta, Story } from '@storybook/web-components';
import type { UmbPropertyEditorUICodeEditorElement } from './property-editor-ui-code-editor.element.js';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-code-editor.element.js';

export default {
	title: 'Property Editor UIs/Code Editor',
	component: 'umb-property-editor-ui-code-editor',
	id: 'umb-property-editor-ui-code-editor',
} as Meta;

export const AAAOverview: Story<UmbPropertyEditorUICodeEditorElement> = () =>
	html` <umb-property-editor-ui-code-editor></umb-property-editor-ui-code-editor>`;
AAAOverview.storyName = 'Overview';
