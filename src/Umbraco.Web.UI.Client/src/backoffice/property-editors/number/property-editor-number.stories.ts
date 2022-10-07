import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

import type { UmbPropertyEditorNumberElement } from './property-editor-number.element';
import './property-editor-number.element';

export default {
	title: 'Property Editors/Number',
	component: 'umb-property-editor-number',
	id: 'umb-property-editor-number',
} as Meta;

export const AAAOverview: Story<UmbPropertyEditorNumberElement> = () =>
	html` <umb-property-editor-number></umb-property-editor-number>`;
AAAOverview.storyName = 'Overview';
