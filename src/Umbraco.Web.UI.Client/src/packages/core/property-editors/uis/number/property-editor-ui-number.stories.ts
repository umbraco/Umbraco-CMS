import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';

import type { UmbPropertyEditorUINumberElement } from './property-editor-ui-number.element';
import './property-editor-ui-number.element';

export default {
	title: 'Property Editor UIs/Number',
	component: 'umb-property-editor-ui-number',
	id: 'umb-property-editor-ui-number',
} as Meta;

export const AAAOverview: Story<UmbPropertyEditorUINumberElement> = () =>
	html` <umb-property-editor-ui-number></umb-property-editor-ui-number>`;
AAAOverview.storyName = 'Overview';
