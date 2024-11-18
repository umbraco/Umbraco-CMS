import type { UmbPropertyEditorUINumberElement } from './property-editor-ui-number.element.js';
import type { Meta, StoryFn } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-number.element.js';

export default {
	title: 'Property Editor UIs/Number',
	component: 'umb-property-editor-ui-number',
	id: 'umb-property-editor-ui-number',
} as Meta;

export const AAAOverview: StoryFn<UmbPropertyEditorUINumberElement> = () =>
	html` <umb-property-editor-ui-number></umb-property-editor-ui-number>`;
AAAOverview.storyName = 'Overview';
