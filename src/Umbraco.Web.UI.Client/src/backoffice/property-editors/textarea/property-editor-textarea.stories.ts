import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

import type { UmbPropertyEditorTextareaElement } from './property-editor-textarea.element';
import './property-editor-textarea.element';

export default {
	title: 'Property Editors/Textarea',
	component: 'umb-property-editor-textarea',
	id: 'umb-property-editor-textarea',
} as Meta;

export const AAAOverview: Story<UmbPropertyEditorTextareaElement> = () =>
	html` <umb-property-editor-textarea></umb-property-editor-textarea>`;
AAAOverview.storyName = 'Overview';
