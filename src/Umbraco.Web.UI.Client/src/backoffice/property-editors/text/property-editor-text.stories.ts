import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

import type { UmbPropertyEditorTextElement } from './property-editor-text.element';
import './property-editor-text.element';

export default {
	title: 'Property Editors/Text',
	component: 'umb-property-editor-text',
	id: 'umb-property-editor-text',
} as Meta;

export const AAAOverview: Story<UmbPropertyEditorTextElement> = () =>
	html` <umb-property-editor-text></umb-property-editor-text>`;
AAAOverview.storyName = 'Overview';
