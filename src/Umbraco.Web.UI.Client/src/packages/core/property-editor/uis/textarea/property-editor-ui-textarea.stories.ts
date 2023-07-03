import { Meta, Story } from '@storybook/web-components';
import type { UmbPropertyEditorUITextareaElement } from './property-editor-ui-textarea.element.js';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-textarea.element.js';

export default {
	title: 'Property Editor UIs/Textarea',
	component: 'umb-property-editor-ui-textarea',
	id: 'umb-property-editor-ui-textarea',
} as Meta;

export const AAAOverview: Story<UmbPropertyEditorUITextareaElement> = () =>
	html` <umb-property-editor-ui-textarea></umb-property-editor-ui-textarea>`;
AAAOverview.storyName = 'Overview';
